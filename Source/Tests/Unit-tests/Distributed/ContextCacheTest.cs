using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.Data.Entities;
using TestHelpers;
using TestHelpers.Mocks;
using TestHelpers.Mocks.Distributed;
using TestHelpers.Mocks.Distributed.Configuration;

namespace UnitTests.Distributed
{
	[TestClass]
	public class ContextCacheTest
	{
		#region Methods

		protected internal virtual async Task<DateTimeOffsetCacheMock> CreateContextCacheAsync(DateTimeOffsetCacheOptionsMock options = null, ISystemClock systemClock = null)
		{
			options ??= await this.CreateDateTimeOffsetCacheOptionsMockAsync();
			systemClock ??= new SystemClockMock();

			var optionsMonitorMock = new Mock<IOptionsMonitor<DateTimeOffsetCacheOptionsMock>>();
			optionsMonitorMock.Setup(optionsMonitor => optionsMonitor.CurrentValue).Returns(options);

			return await Task.FromResult(new DateTimeOffsetCacheMock(Mock.Of<IDbContextFactory<SqlServerCacheContext>>(), Mock.Of<ILoggerFactory>(), optionsMonitorMock.Object, systemClock));
		}

		protected internal virtual async Task<DateTimeOffsetCacheOptionsMock> CreateDateTimeOffsetCacheOptionsMockAsync(TimeSpan? cleanupInterval = null, TimeSpan? defaultSlidingExpiration = null)
		{
			var options = new DateTimeOffsetCacheOptionsMock
			{
				CleanupInterval = cleanupInterval
			};

			if(defaultSlidingExpiration != null)
				options.DefaultSlidingExpiration = defaultSlidingExpiration.Value;

			return await Task.FromResult(options);
		}

		[TestMethod]
		public async Task DefaultCleanupInterval_Default_Test()
		{
			Assert.AreEqual(TimeSpan.FromMinutes(30), (await this.CreateContextCacheAsync()).DefaultCleanupInterval);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task GetExpirationInformation_IfOptionsParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			(await this.CreateContextCacheAsync()).GetExpirationInformation(null);
		}

		[TestMethod]
		public async Task GetExpirationInformation_Test()
		{
			var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);

			var systemClock = new SystemClockMock
			{
				UtcNow = now
			};

			var contextCache = await this.CreateContextCacheAsync(systemClock: systemClock);
			var expirationInformation = contextCache.GetExpirationInformation(new DistributedCacheEntryOptions());
			Assert.IsNull(expirationInformation.AbsoluteExpiration);
			Assert.AreEqual(now.AddMinutes(20), expirationInformation.Expires);
			Assert.IsNotNull(expirationInformation.SlidingExpirationInSeconds);
			Assert.AreEqual(20 * 60, expirationInformation.SlidingExpirationInSeconds.Value);

			var absoluteExpiration = now.AddDays(2);
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpiration = absoluteExpiration
			};
			expirationInformation = contextCache.GetExpirationInformation(options);
			Assert.IsNotNull(expirationInformation.AbsoluteExpiration);
			Assert.AreEqual(absoluteExpiration, expirationInformation.AbsoluteExpiration.Value);
			Assert.AreEqual(absoluteExpiration, expirationInformation.Expires);
			Assert.IsNull(expirationInformation.SlidingExpirationInSeconds);

			absoluteExpiration = now.AddDays(5);
			var absoluteExpirationRelativeToNow = TimeSpan.FromDays(5);
			options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
			};
			expirationInformation = contextCache.GetExpirationInformation(options);
			Assert.IsNotNull(expirationInformation.AbsoluteExpiration);
			Assert.AreEqual(absoluteExpiration, expirationInformation.AbsoluteExpiration.Value);
			Assert.AreEqual(absoluteExpiration, expirationInformation.Expires);
			Assert.IsNull(expirationInformation.SlidingExpirationInSeconds);

			options = new DistributedCacheEntryOptions
			{
				SlidingExpiration = TimeSpan.FromMinutes(50)
			};
			expirationInformation = contextCache.GetExpirationInformation(options);
			Assert.IsNull(expirationInformation.AbsoluteExpiration);
			Assert.AreEqual(now.AddMinutes(50), expirationInformation.Expires);
			Assert.IsNotNull(expirationInformation.SlidingExpirationInSeconds);
			Assert.AreEqual(50 * 60, expirationInformation.SlidingExpirationInSeconds.Value);

			options = new DistributedCacheEntryOptions
			{
				AbsoluteExpiration = now.AddDays(5),
				AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(15),
				SlidingExpiration = TimeSpan.FromMinutes(40)
			};
			expirationInformation = contextCache.GetExpirationInformation(options);
			Assert.IsNotNull(expirationInformation.AbsoluteExpiration);
			Assert.AreEqual(now.AddDays(15), expirationInformation.AbsoluteExpiration.Value);
			Assert.AreEqual(now.AddMinutes(40), expirationInformation.Expires);
			Assert.IsNotNull(expirationInformation.SlidingExpirationInSeconds);
			Assert.AreEqual(40 * 60, expirationInformation.SlidingExpirationInSeconds.Value);

			options = new DistributedCacheEntryOptions
			{
				AbsoluteExpiration = now.AddDays(5),
				SlidingExpiration = TimeSpan.FromMinutes(40)
			};
			expirationInformation = contextCache.GetExpirationInformation(options);
			Assert.IsNotNull(expirationInformation.AbsoluteExpiration);
			Assert.AreEqual(now.AddDays(5), expirationInformation.AbsoluteExpiration.Value);
			Assert.AreEqual(now.AddMinutes(40), expirationInformation.Expires);
			Assert.IsNotNull(expirationInformation.SlidingExpirationInSeconds);
			Assert.AreEqual(40 * 60, expirationInformation.SlidingExpirationInSeconds.Value);

			options = new DistributedCacheEntryOptions
			{
				AbsoluteExpiration = now.AddDays(5),
				AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(15)
			};
			expirationInformation = contextCache.GetExpirationInformation(options);
			Assert.IsNotNull(expirationInformation.AbsoluteExpiration);
			Assert.AreEqual(now.AddDays(15), expirationInformation.AbsoluteExpiration.Value);
			Assert.AreEqual(now.AddDays(15), expirationInformation.Expires);
			Assert.IsNull(expirationInformation.SlidingExpirationInSeconds);
		}

		[TestMethod]
		public async Task GetRefreshedExpiration_Test()
		{
			var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);

			var systemClock = new SystemClockMock
			{
				UtcNow = now
			};

			var contextCache = await this.CreateContextCacheAsync(systemClock: systemClock);
			var initialExpires = now.AddSeconds(10);

			var cacheEntry = new CacheEntry<DateTimeOffset>
			{
				ExpiresAtTime = initialExpires
			};
			Assert.AreEqual(initialExpires, contextCache.GetRefreshedExpiration(cacheEntry));

			cacheEntry = new CacheEntry<DateTimeOffset>
			{
				ExpiresAtTime = initialExpires,
				SlidingExpirationInSeconds = 60
			};
			Assert.AreEqual(now.AddSeconds(60), contextCache.GetRefreshedExpiration(cacheEntry));

			cacheEntry = new CacheEntry<DateTimeOffset>
			{
				AbsoluteExpiration = now.AddHours(5),
				ExpiresAtTime = initialExpires
			};
			Assert.AreEqual(initialExpires, contextCache.GetRefreshedExpiration(cacheEntry));

			cacheEntry = new CacheEntry<DateTimeOffset>
			{
				AbsoluteExpiration = now.AddHours(5),
				ExpiresAtTime = initialExpires,
				SlidingExpirationInSeconds = 60
			};
			Assert.AreEqual(now.AddSeconds(60), contextCache.GetRefreshedExpiration(cacheEntry));

			cacheEntry = new CacheEntry<DateTimeOffset>
			{
				AbsoluteExpiration = now.AddSeconds(40),
				ExpiresAtTime = initialExpires,
				SlidingExpirationInSeconds = 60
			};
			Assert.AreEqual(now.AddSeconds(40), contextCache.GetRefreshedExpiration(cacheEntry));
		}

		[TestMethod]
		public async Task LastCleanup_Default_Test()
		{
			Assert.AreEqual(DateTimeOffset.MinValue, (await this.CreateContextCacheAsync()).LastCleanup);
		}

		#endregion
	}
}