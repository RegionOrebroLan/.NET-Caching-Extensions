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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Caching.Distributed;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.Data.Entities;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Extensions;
using TestHelpers;

namespace IntegrationTests.Distributed
{
	[TestClass]
	public class SqliteCacheTest
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
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					for(var i = 0; i < 10; i++)
					{
						sqliteCacheContext.Cache.Add(new CacheEntry<DateTime>
						{
							ExpiresAtTime = now.AddYears(i).UtcDateTime,
							Id = i.ToString(CultureInfo.InvariantCulture),
							Value = new byte[i]
						});
					}

					Assert.AreEqual(10, await sqliteCacheContext.SaveChangesAsync());
				}

				now = now.AddYears(4).AddMonths(6);
				serviceProvider.SetTime(now);
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();
				Assert.AreEqual(DateTimeOffset.MaxValue, sqliteCache.LastCleanup);
				sqliteCache.LastCleanup = DateTimeOffset.MinValue;
				sqliteCache.CleanupIfNecessary();
				Thread.Sleep(500);
				Assert.AreEqual(now, sqliteCache.LastCleanup);

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(5, await sqliteCacheContext.Cache.CountAsync());
					Assert.AreEqual(5, await sqliteCacheContext.Cache.CountAsync(cacheEntry => cacheEntry.ExpiresAtTime > now.UtcDateTime));
				}
			}
		}

		protected internal virtual async Task<ServiceProvider> CreateServiceProviderAsync()
		{
			var jsonFilePaths = new List<string>
			{
				"appsettings.json",
				"appsettings.Sqlite.json"
			};

			var configuration = Global.CreateConfiguration(jsonFilePaths.ToArray());
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			var serviceProvider = services.BuildServiceProvider();

			var options = serviceProvider.GetRequiredService<DistributedCacheOptions>();
			options.Use(new ApplicationBuilder(serviceProvider));

			// To avoid ongoing work between the tests.
			((SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>()).LastCleanup = DateTimeOffset.MaxValue;

			return await Task.FromResult(serviceProvider);
		}

		[TestMethod]
		public async Task Get_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				// ReSharper disable MethodHasAsyncOverload

				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();
				var value = sqliteCache.Get(key);
				Assert.IsNull(value);

				sqliteCache.Set(key, Array.Empty<byte>());
				value = sqliteCache.Get(key);
				Assert.IsNotNull(value);

				var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);
				serviceProvider.SetTime(now);
				sqliteCache.Set(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				serviceProvider.SetTime(now.AddSeconds(2));
				value = sqliteCache.Get(key);
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
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();
				var value = await sqliteCache.GetAsync(key);
				Assert.IsNull(value);

				await sqliteCache.SetAsync(key, Array.Empty<byte>());
				value = await sqliteCache.GetAsync(key);
				Assert.IsNotNull(value);

				var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);
				serviceProvider.SetTime(now);
				await sqliteCache.SetAsync(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				serviceProvider.SetTime(now.AddSeconds(2));
				value = await sqliteCache.GetAsync(key);
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
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();

				// ReSharper disable All

				sqliteCache.Set(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					var cacheEntry = sqliteCacheContext.Cache.Find(key);
					Assert.AreEqual(now.UtcDateTime.AddSeconds(1), cacheEntry.ExpiresAtTime);
				}

				serviceProvider.SetTime(now.AddSeconds(1));
				sqliteCache.Refresh(key);
				using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					var cacheEntry = sqliteCacheContext.Cache.Find(key);
					Assert.AreEqual(now.UtcDateTime.AddSeconds(2), cacheEntry.ExpiresAtTime);
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
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();

				await sqliteCache.SetAsync(key, Array.Empty<byte>(), new DistributedCacheEntryOptions {SlidingExpiration = TimeSpan.FromSeconds(1)});
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					var cacheEntry = await sqliteCacheContext.Cache.FindAsync(key);
					Assert.AreEqual(now.UtcDateTime.AddSeconds(1), cacheEntry.ExpiresAtTime);
				}

				serviceProvider.SetTime(now.AddSeconds(1));
				await sqliteCache.RefreshAsync(key);
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					var cacheEntry = await sqliteCacheContext.Cache.FindAsync(key);
					Assert.AreEqual(now.UtcDateTime.AddSeconds(2), cacheEntry.ExpiresAtTime);
				}
			}
		}

		[TestMethod]
		public async Task Remove_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();

				// ReSharper disable MethodHasAsyncOverload

				sqliteCache.Set(key, Array.Empty<byte>());
				Assert.IsNotNull(sqliteCache.Get(key));

				sqliteCache.Remove(key);
				Assert.IsNull(sqliteCache.Get(key));

				// ReSharper restore MethodHasAsyncOverload
			}
		}

		[TestMethod]
		public async Task RemoveAsync_Test()
		{
			const string key = "Key";

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();

				await sqliteCache.SetAsync(key, Array.Empty<byte>());
				Assert.IsNotNull(await sqliteCache.GetAsync(key));

				await sqliteCache.RemoveAsync(key);
				Assert.IsNull(await sqliteCache.GetAsync(key));
			}
		}

		[TestMethod]
		public async Task RemoveExpiredCacheEntries_Test()
		{
			var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				// Prepare
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					for(var i = 0; i < 10; i++)
					{
						sqliteCacheContext.Cache.Add(new CacheEntry<DateTime>
						{
							ExpiresAtTime = now.AddYears(i).UtcDateTime,
							Id = i.ToString(CultureInfo.InvariantCulture),
							Value = new byte[i]
						});
					}

					Assert.AreEqual(10, await sqliteCacheContext.SaveChangesAsync());
				}

				now = now.AddYears(4).AddMonths(6);
				serviceProvider.SetTime(now);
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();

				// ReSharper disable MethodHasAsyncOverload
				Assert.AreEqual(5, sqliteCache.RemoveExpiredCacheEntries());
				// ReSharper restore MethodHasAsyncOverload

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(5, await sqliteCacheContext.Cache.CountAsync());
					Assert.AreEqual(5, await sqliteCacheContext.Cache.CountAsync(cacheEntry => cacheEntry.ExpiresAtTime > now.UtcDateTime));
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
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					for(var i = 0; i < 10; i++)
					{
						sqliteCacheContext.Cache.Add(new CacheEntry<DateTime>
						{
							ExpiresAtTime = now.AddYears(i).UtcDateTime,
							Id = i.ToString(CultureInfo.InvariantCulture),
							Value = new byte[i]
						});
					}

					Assert.AreEqual(10, await sqliteCacheContext.SaveChangesAsync());
				}

				now = now.AddYears(4).AddMonths(6);
				serviceProvider.SetTime(now);
				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();

				Assert.AreEqual(5, await sqliteCache.RemoveExpiredCacheEntriesAsync());

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(5, await sqliteCacheContext.Cache.CountAsync());
					Assert.AreEqual(5, await sqliteCacheContext.Cache.CountAsync(cacheEntry => cacheEntry.ExpiresAtTime > now.UtcDateTime));
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

				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();
				// ReSharper disable MethodHasAsyncOverload
				sqliteCache.Set(key, Array.Empty<byte>());
				// ReSharper restore MethodHasAsyncOverload

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(1, await sqliteCacheContext.Cache.CountAsync());

					var cacheEntry = await sqliteCacheContext.Cache.FindAsync(key);
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

				var sqliteCache = (SqliteCache)serviceProvider.GetRequiredService<IDistributedCache>();
				await sqliteCache.SetAsync(key, Array.Empty<byte>());

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(1, await sqliteCacheContext.Cache.CountAsync());

					var cacheEntry = await sqliteCacheContext.Cache.FindAsync(key);
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
			var configuration = Global.CreateConfiguration("appsettings.json", "appsettings.Sqlite.json");
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			await using(var serviceProvider = services.BuildServiceProvider())
			{
				await using(var cacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
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