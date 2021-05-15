using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Helpers.Extensions;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Caching.Distributed.Data.Entities;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Extensions;
using TestHelpers;
using TestHelpers.Mocks.Distributed;
using DateTimeOffsetCacheContext = RegionOrebroLan.Caching.Distributed.Data.SqlServerCacheContext;

namespace IntegrationTests.Distributed
{
	[TestClass]
	public class DateTimeOffsetContextCacheTest
	{
		#region Properties

		protected internal virtual string DataDirectoryPath => Global.DataDirectoryPath;

		#endregion

		#region Methods

		[TestMethod]
		public async Task CleanupIfNecessary_Test()
		{
			var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				// Prepare
				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					for(var i = 0; i < 10; i++)
					{
						dateTimeOffsetCacheContext.Cache.Add(new CacheEntry<DateTimeOffset>
						{
							ExpiresAtTime = now.AddYears(i),
							Id = i.ToString(CultureInfo.InvariantCulture),
							Value = new byte[i]
						});
					}

					Assert.AreEqual(10, await dateTimeOffsetCacheContext.SaveChangesAsync());
				}

				now = now.AddYears(4).AddMonths(6);
				serviceProvider.SetTime(now);
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();
				Assert.AreEqual(DateTimeOffset.MaxValue, dateTimeOffsetContextCache.LastCleanup);
				dateTimeOffsetContextCache.LastCleanup = DateTimeOffset.MinValue;
				dateTimeOffsetContextCache.CleanupIfNecessary();
				Thread.Sleep(500);
				Assert.AreEqual(now, dateTimeOffsetContextCache.LastCleanup);

				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(5, await dateTimeOffsetCacheContext.Cache.CountAsync());
					Assert.AreEqual(5, await dateTimeOffsetCacheContext.Cache.CountAsync(cacheEntry => cacheEntry.ExpiresAtTime > now));
				}
			}
		}

		protected internal virtual async Task<IConfiguration> CreateConfigurationAsync()
		{
			var jsonFilePaths = new List<string>
			{
				"appsettings.json",
				"appsettings.SqlServer1.json"
			};

			var configurationBuilder = Global.CreateConfigurationBuilder(jsonFilePaths.ToArray());

			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			{
				{"Caching:DistributedCache:Type", "TestHelpers.Mocks.Distributed.DependencyInjection.Configuration.DateTimeOffsetContextOptionsMock, Test-helpers"}
			});

			return await Task.FromResult(configurationBuilder.Build());
		}

		protected internal virtual async Task<ServiceProvider> CreateServiceProviderAsync()
		{
			var configuration = await this.CreateConfigurationAsync();
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			var serviceProvider = services.BuildServiceProvider();

			var options = serviceProvider.GetRequiredService<DistributedCacheOptions>();
			options.Use(new ApplicationBuilder(serviceProvider));

			// To avoid ongoing work between the tests.
			((DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>()).LastCleanup = DateTimeOffset.MaxValue;

			return await Task.FromResult(serviceProvider);
		}

		[TestMethod]
		public async Task Get_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				// ReSharper disable MethodHasAsyncOverload

				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();
				var value = dateTimeOffsetContextCache.Get(key);
				Assert.IsNull(value);

				dateTimeOffsetContextCache.Set(key, Array.Empty<byte>());
				value = dateTimeOffsetContextCache.Get(key);
				Assert.IsNotNull(value);

				var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);
				serviceProvider.SetTime(now);
				dateTimeOffsetContextCache.Set(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				serviceProvider.SetTime(now.AddSeconds(2));
				value = dateTimeOffsetContextCache.Get(key);
				Assert.IsNull(value);

				// ReSharper restore MethodHasAsyncOverload
			}
		}

		[TestMethod]
		public async Task GetAsync_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();
				var value = await dateTimeOffsetContextCache.GetAsync(key);
				Assert.IsNull(value);

				await dateTimeOffsetContextCache.SetAsync(key, Array.Empty<byte>());
				value = await dateTimeOffsetContextCache.GetAsync(key);
				Assert.IsNotNull(value);

				var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);
				serviceProvider.SetTime(now);
				await dateTimeOffsetContextCache.SetAsync(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				serviceProvider.SetTime(now.AddSeconds(2));
				value = await dateTimeOffsetContextCache.GetAsync(key);
				Assert.IsNull(value);
			}
		}

		[TestMethod]
		public async Task Refresh_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);
				serviceProvider.SetTime(now);
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();

				// ReSharper disable All

				dateTimeOffsetContextCache.Set(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					var cacheEntry = dateTimeOffsetCacheContext.Cache.Find(key);
					Assert.AreEqual(now.AddSeconds(1), cacheEntry.ExpiresAtTime);
				}

				serviceProvider.SetTime(now.AddSeconds(1));
				dateTimeOffsetContextCache.Refresh(key);
				using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					var cacheEntry = dateTimeOffsetCacheContext.Cache.Find(key);
					Assert.AreEqual(now.AddSeconds(2), cacheEntry.ExpiresAtTime);
				}

				// ReSharper restore All
			}
		}

		[TestMethod]
		public async Task RefreshAsync_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);
				serviceProvider.SetTime(now);
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();

				await dateTimeOffsetContextCache.SetAsync(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					var cacheEntry = await dateTimeOffsetCacheContext.Cache.FindAsync(key);
					Assert.AreEqual(now.AddSeconds(1), cacheEntry.ExpiresAtTime);
				}

				serviceProvider.SetTime(now.AddSeconds(1));
				await dateTimeOffsetContextCache.RefreshAsync(key);
				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					var cacheEntry = await dateTimeOffsetCacheContext.Cache.FindAsync(key);
					Assert.AreEqual(now.AddSeconds(2), cacheEntry.ExpiresAtTime);
				}
			}
		}

		[TestMethod]
		public async Task Remove_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();

				// ReSharper disable MethodHasAsyncOverload

				dateTimeOffsetContextCache.Set(key, Array.Empty<byte>());
				Assert.IsNotNull(dateTimeOffsetContextCache.Get(key));

				dateTimeOffsetContextCache.Remove(key);
				Assert.IsNull(dateTimeOffsetContextCache.Get(key));

				// ReSharper restore MethodHasAsyncOverload
			}
		}

		[TestMethod]
		public async Task RemoveAsync_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();

				await dateTimeOffsetContextCache.SetAsync(key, Array.Empty<byte>());
				Assert.IsNotNull(await dateTimeOffsetContextCache.GetAsync(key));

				await dateTimeOffsetContextCache.RemoveAsync(key);
				Assert.IsNull(await dateTimeOffsetContextCache.GetAsync(key));
			}
		}

		[TestMethod]
		public async Task RemoveExpiredCacheEntries_Test()
		{
			var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				// Prepare
				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					for(var i = 0; i < 10; i++)
					{
						dateTimeOffsetCacheContext.Cache.Add(new CacheEntry<DateTimeOffset>
						{
							ExpiresAtTime = now.AddYears(i),
							Id = i.ToString(CultureInfo.InvariantCulture),
							Value = new byte[i]
						});
					}

					Assert.AreEqual(10, await dateTimeOffsetCacheContext.SaveChangesAsync());
				}

				now = now.AddYears(4).AddMonths(6);
				serviceProvider.SetTime(now);
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();

				// ReSharper disable MethodHasAsyncOverload
				Assert.AreEqual(5, dateTimeOffsetContextCache.RemoveExpiredCacheEntries());
				// ReSharper restore MethodHasAsyncOverload

				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(5, await dateTimeOffsetCacheContext.Cache.CountAsync());
					Assert.AreEqual(5, await dateTimeOffsetCacheContext.Cache.CountAsync(cacheEntry => cacheEntry.ExpiresAtTime > now));
				}
			}
		}

		[TestMethod]
		public async Task RemoveExpiredCacheEntriesAsync_Test()
		{
			var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				// Prepare
				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					for(var i = 0; i < 10; i++)
					{
						dateTimeOffsetCacheContext.Cache.Add(new CacheEntry<DateTimeOffset>
						{
							ExpiresAtTime = now.AddYears(i),
							Id = i.ToString(CultureInfo.InvariantCulture),
							Value = new byte[i]
						});
					}

					Assert.AreEqual(10, await dateTimeOffsetCacheContext.SaveChangesAsync());
				}

				now = now.AddYears(4).AddMonths(6);
				serviceProvider.SetTime(now);
				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();

				Assert.AreEqual(5, await dateTimeOffsetContextCache.RemoveExpiredCacheEntriesAsync());

				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(5, await dateTimeOffsetCacheContext.Cache.CountAsync());
					Assert.AreEqual(5, await dateTimeOffsetCacheContext.Cache.CountAsync(cacheEntry => cacheEntry.ExpiresAtTime > now));
				}
			}
		}

		[TestMethod]
		public async Task Set_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				serviceProvider.SetTime(await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000));

				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();
				// ReSharper disable MethodHasAsyncOverload
				dateTimeOffsetContextCache.Set(key, Array.Empty<byte>());
				// ReSharper restore MethodHasAsyncOverload

				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(1, await dateTimeOffsetCacheContext.Cache.CountAsync());

					var cacheEntry = await dateTimeOffsetCacheContext.Cache.FindAsync(key);
					Assert.IsNull(cacheEntry.AbsoluteExpiration);
					Assert.AreEqual(await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000, minute: 20), cacheEntry.ExpiresAtTime);
					Assert.AreEqual(key, cacheEntry.Id);
					Assert.AreEqual(Convert.ToInt64(TimeSpan.FromMinutes(20).TotalSeconds), cacheEntry.SlidingExpirationInSeconds);
					Assert.IsTrue(Array.Empty<byte>().SequenceEqual(cacheEntry.Value));
				}
			}
		}

		[TestMethod]
		public async Task SetAsync_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				serviceProvider.SetTime(await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000));

				var dateTimeOffsetContextCache = (DateTimeOffsetCacheMock)serviceProvider.GetRequiredService<IDistributedCache>();
				await dateTimeOffsetContextCache.SetAsync(key, Array.Empty<byte>());

				await using(var dateTimeOffsetCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(1, await dateTimeOffsetCacheContext.Cache.CountAsync());

					var cacheEntry = await dateTimeOffsetCacheContext.Cache.FindAsync(key);
					Assert.IsNull(cacheEntry.AbsoluteExpiration);
					Assert.AreEqual(await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000, minute: 20), cacheEntry.ExpiresAtTime);
					Assert.AreEqual(key, cacheEntry.Id);
					Assert.AreEqual(Convert.ToInt64(TimeSpan.FromMinutes(20).TotalSeconds), cacheEntry.SlidingExpirationInSeconds);
					Assert.IsTrue(Array.Empty<byte>().SequenceEqual(cacheEntry.Value));
				}
			}
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			var configuration = await this.CreateConfigurationAsync();
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			await using(var serviceProvider = services.BuildServiceProvider())
			{
				await using(var cacheContext = serviceProvider.GetRequiredService<IDbContextFactory<DateTimeOffsetCacheContext>>().CreateDbContext())
				{
					await cacheContext.Database.EnsureDeletedAsync();
				}
			}

			AppDomain.CurrentDomain.SetDataDirectory(null);
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			await Task.CompletedTask;

			AppDomain.CurrentDomain.SetDataDirectory(this.DataDirectoryPath);
		}

		#endregion
	}
}