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
	public abstract class DateTimeOffsetContextCache<TDatabaseContext, TOptions> : ContextCache<TDatabaseContext, DateTimeOffset, TOptions>, IDistributedCache where TDatabaseContext : CacheContext<TDatabaseContext, DateTimeOffset> where TOptions : DatabaseContextCacheOptions
	{
		#region Constructors

		protected DateTimeOffsetContextCache(IDbContextFactory<TDatabaseContext> databaseContextFactory, TimeSpan defaultCleanupInterval, ILoggerFactory loggerFactory, IOptionsMonitor<TOptions> optionsMonitor, ISystemClock systemClock) : base(databaseContextFactory, defaultCleanupInterval, loggerFactory, optionsMonitor, systemClock) { }

		#endregion

		#region Methods

		protected internal override bool CacheEntryHasExpired(CacheEntry<DateTimeOffset> cacheEntry)
		{
			if(cacheEntry == null)
				return true;

			return this.SystemClock.UtcNow > cacheEntry.ExpiresAtTime;
		}

		protected internal override IQueryable<CacheEntry<DateTimeOffset>> GetExpiredCacheEntries(TDatabaseContext cacheContext)
		{
			if(cacheContext == null)
				throw new ArgumentNullException(nameof(cacheContext));

			var now = this.SystemClock.UtcNow;

			return cacheContext.Cache.Where(entry => now > entry.ExpiresAtTime);
		}

		protected internal virtual bool RefreshCacheEntry(CacheEntry<DateTimeOffset> cacheEntry)
		{
			if(cacheEntry == null)
				throw new ArgumentNullException(nameof(cacheEntry));

			var refreshedExpiration = this.GetRefreshedExpiration(cacheEntry);

			if(cacheEntry.ExpiresAtTime == refreshedExpiration)
				return false;

			cacheEntry.ExpiresAtTime = refreshedExpiration.UtcDateTime;

			return true;
		}

		protected internal override void RefreshCacheEntry(TDatabaseContext cacheContext, CacheEntry<DateTimeOffset> cacheEntry)
		{
			if(cacheContext == null)
				throw new ArgumentNullException(nameof(cacheContext));

			if(!this.RefreshCacheEntry(cacheEntry))
				return;

			cacheContext.SaveChanges();
		}

		protected internal override async Task RefreshCacheEntryAsync(TDatabaseContext cacheContext, CacheEntry<DateTimeOffset> cacheEntry, CancellationToken token = default)
		{
			if(cacheContext == null)
				throw new ArgumentNullException(nameof(cacheContext));

			token.ThrowIfCancellationRequested();

			if(!this.RefreshCacheEntry(cacheEntry))
				return;

			await cacheContext.SaveChangesAsync(token).ConfigureAwait(false);
		}

		protected internal override void SetCacheEntryExpiration(CacheEntry<DateTimeOffset> cacheEntry, DistributedCacheEntryOptions options)
		{
			if(cacheEntry == null)
				throw new ArgumentNullException(nameof(cacheEntry));

			var expirationInformation = this.GetExpirationInformation(options);

			cacheEntry.AbsoluteExpiration = expirationInformation.AbsoluteExpiration;
			cacheEntry.ExpiresAtTime = expirationInformation.Expires;
			cacheEntry.SlidingExpirationInSeconds = expirationInformation.SlidingExpirationInSeconds;
		}

		protected internal override async Task SetCacheEntryExpirationAsync(CacheEntry<DateTimeOffset> cacheEntry, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();

			await Task.CompletedTask.ConfigureAwait(false);

			this.SetCacheEntryExpiration(cacheEntry, options);
		}

		#endregion
	}
}