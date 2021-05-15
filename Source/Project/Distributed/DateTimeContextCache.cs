using System;
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

namespace RegionOrebroLan.Caching.Distributed
{
	public abstract class DateTimeContextCache<TDatabaseContext, TOptions> : ContextCache<TDatabaseContext, DateTime, TOptions>, IDistributedCache where TDatabaseContext : CacheContext<TDatabaseContext, DateTime> where TOptions : DatabaseContextCacheOptions
	{
		#region Constructors

		protected DateTimeContextCache(IDbContextFactory<TDatabaseContext> databaseContextFactory, TimeSpan defaultCleanupInterval, ILoggerFactory loggerFactory, IOptionsMonitor<TOptions> optionsMonitor, ISystemClock systemClock) : base(databaseContextFactory, defaultCleanupInterval, loggerFactory, optionsMonitor, systemClock) { }

		#endregion

		#region Methods

		protected internal override bool CacheEntryHasExpired(CacheEntry<DateTime> cacheEntry)
		{
			if(cacheEntry == null)
				return true;

			return this.SystemClock.UtcNow.UtcDateTime > cacheEntry.ExpiresAtTime;
		}

		protected internal override IQueryable<CacheEntry<DateTime>> GetExpiredCacheEntries(TDatabaseContext cacheContext)
		{
			if(cacheContext == null)
				throw new ArgumentNullException(nameof(cacheContext));

			var now = this.SystemClock.UtcNow.UtcDateTime;

			return cacheContext.Cache.Where(entry => now > entry.ExpiresAtTime);
		}

		protected internal virtual bool RefreshCacheEntry(CacheEntry<DateTime> cacheEntry)
		{
			if(cacheEntry == null)
				throw new ArgumentNullException(nameof(cacheEntry));

			var expiration = new DateTimeOffset(cacheEntry.ExpiresAtTime);

			var refreshedExpiration = this.GetRefreshedExpiration(new CacheEntry<DateTimeOffset>
			{
				AbsoluteExpiration = cacheEntry.AbsoluteExpiration != null ? new DateTimeOffset(cacheEntry.AbsoluteExpiration.Value) : null,
				ExpiresAtTime = expiration,
				SlidingExpirationInSeconds = cacheEntry.SlidingExpirationInSeconds
			});

			if(expiration == refreshedExpiration)
				return false;

			cacheEntry.ExpiresAtTime = refreshedExpiration.UtcDateTime;

			return true;
		}

		protected internal override void RefreshCacheEntry(TDatabaseContext cacheContext, CacheEntry<DateTime> cacheEntry)
		{
			if(cacheContext == null)
				throw new ArgumentNullException(nameof(cacheContext));

			if(!this.RefreshCacheEntry(cacheEntry))
				return;

			cacheContext.SaveChanges();
		}

		protected internal override async Task RefreshCacheEntryAsync(TDatabaseContext cacheContext, CacheEntry<DateTime> cacheEntry, CancellationToken token = default)
		{
			if(cacheContext == null)
				throw new ArgumentNullException(nameof(cacheContext));

			token.ThrowIfCancellationRequested();

			if(!this.RefreshCacheEntry(cacheEntry))
				return;

			await cacheContext.SaveChangesAsync(token).ConfigureAwait(false);
		}

		protected internal override void SetCacheEntryExpiration(CacheEntry<DateTime> cacheEntry, DistributedCacheEntryOptions options)
		{
			if(cacheEntry == null)
				throw new ArgumentNullException(nameof(cacheEntry));

			var expirationInformation = this.GetExpirationInformation(options);

			cacheEntry.AbsoluteExpiration = expirationInformation.AbsoluteExpiration?.UtcDateTime;
			cacheEntry.ExpiresAtTime = expirationInformation.Expires.UtcDateTime;
			cacheEntry.SlidingExpirationInSeconds = expirationInformation.SlidingExpirationInSeconds;
		}

		protected internal override async Task SetCacheEntryExpirationAsync(CacheEntry<DateTime> cacheEntry, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();

			await Task.CompletedTask.ConfigureAwait(false);

			this.SetCacheEntryExpiration(cacheEntry, options);
		}

		#endregion
	}
}