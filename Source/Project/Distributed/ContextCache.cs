using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Caching.Distributed.Configuration;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.Data.Entities;
using RegionOrebroLan.Logging.Extensions;

namespace RegionOrebroLan.Caching.Distributed
{
	public abstract class ContextCache<TDatabaseContext, TDateTime, TOptions> where TDatabaseContext : CacheContext<TDatabaseContext, TDateTime> where TDateTime : struct where TOptions : DatabaseContextCacheOptions
	{
		#region Constructors

		protected ContextCache(IDbContextFactory<TDatabaseContext> databaseContextFactory, TimeSpan defaultCleanupInterval, ILoggerFactory loggerFactory, IOptionsMonitor<TOptions> optionsMonitor, ISystemClock systemClock)
		{
			this.DatabaseContextFactory = databaseContextFactory ?? throw new ArgumentNullException(nameof(databaseContextFactory));
			this.DefaultCleanupInterval = defaultCleanupInterval;
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
			this.SystemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		}

		#endregion

		#region Properties

		protected internal virtual object CleanupLock { get; } = new();
		protected internal virtual IDbContextFactory<TDatabaseContext> DatabaseContextFactory { get; }
		protected internal TimeSpan DefaultCleanupInterval { get; }
		protected internal virtual DateTimeOffset LastCleanup { get; set; } = DateTimeOffset.MinValue;
		protected internal virtual ILogger Logger { get; }
		protected internal virtual IOptionsMonitor<TOptions> OptionsMonitor { get; }
		protected internal virtual ISystemClock SystemClock { get; }

		#endregion

		#region Methods

		protected internal abstract bool CacheEntryHasExpired(CacheEntry<TDateTime> cacheEntry);

		public virtual void CleanupIfNecessary()
		{
			lock(this.CleanupLock)
			{
				var cleanupInterval = this.OptionsMonitor.CurrentValue.CleanupInterval ?? this.DefaultCleanupInterval;
				var now = this.SystemClock.UtcNow;

				if(now - this.LastCleanup <= cleanupInterval)
					return;

				this.LastCleanup = now;

				this.Logger.LogTraceIfEnabled("CleanupIfNecessary: starting cleanup...");
				Task.Run(this.RemoveExpiredCacheEntries);
			}
		}

		[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
		public virtual byte[] Get(string key)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			byte[] value = null;

			using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				var cacheEntry = cacheContext.Cache.Find(key);

				if(!this.CacheEntryHasExpired(cacheEntry))
				{
					this.RefreshCacheEntry(cacheContext, cacheEntry);
					value = cacheEntry.Value;
				}
			}

			this.CleanupIfNecessary();

			return value;
		}

		public virtual async Task<byte[]> GetAsync(string key, CancellationToken token = default)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			token.ThrowIfCancellationRequested();

			byte[] value = null;

			await using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				var cacheEntry = await cacheContext.Cache.FindAsync(new object[] { key }, token).ConfigureAwait(false);

				if(!this.CacheEntryHasExpired(cacheEntry))
				{
					await this.RefreshCacheEntryAsync(cacheContext, cacheEntry, token).ConfigureAwait(false);
					value = cacheEntry.Value;
				}
			}

			this.CleanupIfNecessary();

			return value;
		}

		protected internal virtual IExpirationInformation GetExpirationInformation(DistributedCacheEntryOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			var now = this.SystemClock.UtcNow;
			var resolvedOptions = new DistributedCacheEntryOptions();

			if(options.AbsoluteExpiration == null && options.AbsoluteExpirationRelativeToNow == null && options.SlidingExpiration == null)
			{
				resolvedOptions.SlidingExpiration = this.OptionsMonitor.CurrentValue.DefaultSlidingExpiration;
			}
			else
			{
				resolvedOptions.AbsoluteExpiration = options.AbsoluteExpiration;
				resolvedOptions.AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow;
				resolvedOptions.SlidingExpiration = options.SlidingExpiration;
			}

			if(resolvedOptions.AbsoluteExpirationRelativeToNow != null)
				resolvedOptions.AbsoluteExpiration = now.Add(resolvedOptions.AbsoluteExpirationRelativeToNow.Value);
			else if(resolvedOptions.AbsoluteExpiration != null && resolvedOptions.AbsoluteExpiration.Value <= now)
				throw new ArgumentException("The absolute expiration value must be in the future.", nameof(options));

			// ReSharper disable PossibleInvalidOperationException
			var expires = resolvedOptions.SlidingExpiration == null ? resolvedOptions.AbsoluteExpiration.Value : now.Add(resolvedOptions.SlidingExpiration.Value);
			// ReSharper restore PossibleInvalidOperationException

			return new ExpirationInformation
			{
				AbsoluteExpiration = resolvedOptions.AbsoluteExpiration,
				Expires = expires,
				SlidingExpirationInSeconds = resolvedOptions.SlidingExpiration != null ? Convert.ToInt64(resolvedOptions.SlidingExpiration.Value.TotalSeconds) : null
			};
		}

		protected internal abstract IQueryable<CacheEntry<TDateTime>> GetExpiredCacheEntries(TDatabaseContext cacheContext);

		protected internal virtual DateTimeOffset GetRefreshedExpiration(CacheEntry<DateTimeOffset> cacheEntry)
		{
			if(cacheEntry == null)
				throw new ArgumentNullException(nameof(cacheEntry));

			var now = this.SystemClock.UtcNow;

			// ReSharper disable InvertIf
			if(cacheEntry.SlidingExpirationInSeconds != null)
			{
				var slidingExpiration = TimeSpan.FromSeconds(cacheEntry.SlidingExpirationInSeconds.Value);

				if(cacheEntry.AbsoluteExpiration != null && cacheEntry.AbsoluteExpiration.Value - now <= slidingExpiration)
					return cacheEntry.AbsoluteExpiration.Value;

				return now.Add(slidingExpiration);
			}
			// ReSharper restore InvertIf

			return cacheEntry.ExpiresAtTime;
		}

		public virtual void Refresh(string key)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				var cacheEntry = cacheContext.Cache.Find(key);

				if(cacheEntry != null)
					this.RefreshCacheEntry(cacheContext, cacheEntry);
			}

			this.CleanupIfNecessary();
		}

		public virtual async Task RefreshAsync(string key, CancellationToken token = default)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			token.ThrowIfCancellationRequested();

			await using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				var cacheEntry = await cacheContext.Cache.FindAsync(new object[] { key }, token).ConfigureAwait(false);

				if(cacheEntry != null)
					await this.RefreshCacheEntryAsync(cacheContext, cacheEntry, token).ConfigureAwait(false);
			}

			this.CleanupIfNecessary();
		}

		protected internal abstract void RefreshCacheEntry(TDatabaseContext cacheContext, CacheEntry<TDateTime> cacheEntry);
		protected internal abstract Task RefreshCacheEntryAsync(TDatabaseContext cacheContext, CacheEntry<TDateTime> cacheEntry, CancellationToken token = default);

		public virtual void Remove(string key)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				cacheContext.Cache.Remove(cacheContext.Cache.Find(key));
				cacheContext.SaveChanges();
			}

			this.CleanupIfNecessary();
		}

		public virtual async Task RemoveAsync(string key, CancellationToken token = default)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			token.ThrowIfCancellationRequested();

			await using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				cacheContext.Cache.Remove(await cacheContext.Cache.FindAsync(new object[] { key }, token).ConfigureAwait(false));
				await cacheContext.SaveChangesAsync(token).ConfigureAwait(false);
			}

			this.CleanupIfNecessary();
		}

		public virtual int RemoveExpiredCacheEntries()
		{
			using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				var now = this.SystemClock.UtcNow;
				cacheContext.Cache.RemoveRange(this.GetExpiredCacheEntries(cacheContext));
				var removed = cacheContext.SaveChanges();
				this.Logger.LogTraceIfEnabled($"RemoveExpiredCacheEntries: Removed {removed} expired cache-entries.");
				return removed;
			}
		}

		public virtual async Task<int> RemoveExpiredCacheEntriesAsync()
		{
			await using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				cacheContext.Cache.RemoveRange(this.GetExpiredCacheEntries(cacheContext));
				var removed = await cacheContext.SaveChangesAsync().ConfigureAwait(false);
				this.Logger.LogTraceIfEnabled($"RemoveExpiredCacheEntriesAsync: Removed {removed} expired cache-entries.");
				return removed;
			}
		}

		[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
		public virtual void Set(string key, byte[] value, DistributedCacheEntryOptions options)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			if(value == null)
				throw new ArgumentNullException(nameof(value));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				var cacheEntry = cacheContext.Cache.Find(key) ?? cacheContext.Cache.Add(new CacheEntry<TDateTime> { Id = key }).Entity;

				cacheEntry.Value = value;

				this.SetCacheEntryExpiration(cacheEntry, options);

				cacheContext.SaveChanges();
			}

			this.CleanupIfNecessary();
		}

		public virtual async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			if(value == null)
				throw new ArgumentNullException(nameof(value));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			token.ThrowIfCancellationRequested();

			await using(var cacheContext = this.DatabaseContextFactory.CreateDbContext())
			{
				var cacheEntry = await cacheContext.Cache.FindAsync(new object[] { key }, token).ConfigureAwait(false) ?? (await cacheContext.Cache.AddAsync(new CacheEntry<TDateTime> { Id = key }, token).ConfigureAwait(false)).Entity;

				cacheEntry.Value = value;

				await this.SetCacheEntryExpirationAsync(cacheEntry, options, token).ConfigureAwait(false);

				await cacheContext.SaveChangesAsync(token).ConfigureAwait(false);
			}

			this.CleanupIfNecessary();
		}

		protected internal abstract void SetCacheEntryExpiration(CacheEntry<TDateTime> cacheEntry, DistributedCacheEntryOptions options);
		protected internal abstract Task SetCacheEntryExpirationAsync(CacheEntry<TDateTime> cacheEntry, DistributedCacheEntryOptions options, CancellationToken token = default);

		#endregion
	}
}