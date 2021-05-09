using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Caching.Sqlite;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Extensions;
using StackExchange.Redis;

namespace IntegrationTests.Distributed.DependencyInjection.Extensions
{
	[TestClass]
	public class ServiceCollectionExtensionTest
	{
		#region Properties

		protected internal virtual string DataDirectoryPath => Global.DataDirectoryPath;

		#endregion

		#region Methods

		[TestMethod]
		public async Task AddDistributedCache_Empty_Test()
		{
			await this.AddDistributedCacheTest(null, 1);
		}

		[TestMethod]
		public async Task AddDistributedCache_Memory_Test()
		{
			await this.AddDistributedCacheTest(DistributedCacheKind.Memory, 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_Redis_Test_1()
		{
			await this.AddRedisDistributedCacheTest(DistributedCacheKind.Redis1, 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_Redis_Test_2()
		{
			await this.AddRedisDistributedCacheTest(DistributedCacheKind.Redis2, 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_Redis_Test_3()
		{
			await this.AddRedisDistributedCacheTest(DistributedCacheKind.Redis3, 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_Sqlite_Test_1()
		{
			await this.AddDistributedCacheTest(DistributedCacheKind.Sqlite1, 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_Sqlite_Test_2()
		{
			await this.AddDistributedCacheTest(DistributedCacheKind.Sqlite2, 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_SqlServer_Test_1()
		{
			await this.AddDistributedCacheTest(DistributedCacheKind.SqlServer1, 11);
		}

		[TestMethod]
		public async Task AddDistributedCache_SqlServer_Test_2()
		{
			await this.AddDistributedCacheTest(DistributedCacheKind.SqlServer2, 11);
		}

		protected internal virtual async Task AddDistributedCacheTest(DistributedCacheKind? distributedCacheKind, int expectedNumberOfServices)
		{
			var jsonFilePaths = new List<string>
			{
				"appsettings.json"
			};

			if(distributedCacheKind != null)
				jsonFilePaths.Add($"appsettings.{distributedCacheKind}.json");

			var configuration = Global.CreateConfiguration(jsonFilePaths.ToArray());

			var services = Global.CreateServices(configuration);

			var numberOfServicesBefore = services.Count;

			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			Assert.AreEqual(expectedNumberOfServices, services.Count - numberOfServicesBefore);

			await using(var serviceProvider = services.BuildServiceProvider())
			{
				var distributedCacheOptions = serviceProvider.GetRequiredService<DistributedCacheOptions>();

				distributedCacheOptions.Use(new ApplicationBuilder(serviceProvider));

				var distributedCache = serviceProvider.GetService<IDistributedCache>();

				if(distributedCacheKind != null)
				{
					const string key = "Key";
					const string value = "Value";
					await distributedCache.SetStringAsync(key, value);
					var actualValue = await distributedCache.GetStringAsync(key);
					Assert.AreEqual(value, actualValue);
				}
				else
				{
					Assert.IsNull(distributedCache);
				}
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		protected internal virtual async Task AddRedisDistributedCacheTest(DistributedCacheKind distributedCacheKind, int expectedNumberOfServices)
		{
			if(distributedCacheKind is not (DistributedCacheKind.Redis1 or DistributedCacheKind.Redis2 or DistributedCacheKind.Redis3))
				throw new ArgumentException($"Invalid distributed-cache-kind: {distributedCacheKind}", nameof(distributedCacheKind));

			try
			{
				await this.AddDistributedCacheTest(distributedCacheKind, expectedNumberOfServices);
			}
			catch(RedisConnectionException redisConnectionException)
			{
				Assert.Inconclusive($"You need to setup a Redis. You can do it with docker: \"docker run --rm -it -p 6379:6379 redis\". Exception: {redisConnectionException}");
			}
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			foreach(var distributedCacheKind in new[] {DistributedCacheKind.Sqlite1, DistributedCacheKind.SqlServer1})
			{
				var configuration = Global.CreateConfiguration("appsettings.json", $"appsettings.{distributedCacheKind}.json");
				var services = Global.CreateServices(configuration);
				services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

				// ReSharper disable UseAwaitUsing
				using(var serviceProvider = services.BuildServiceProvider())
				{
					// Sqlite
					var sqliteCacheOptions = serviceProvider.GetService<IOptions<SqliteCacheOptions>>();
					if(sqliteCacheOptions != null)
					{
						var path = sqliteCacheOptions.Value.CachePath;
						if(File.Exists(path))
							File.Delete(path);
					}

					// SqlServer
					using(var scope = serviceProvider.CreateScope())
					{
						var cacheContext = scope.ServiceProvider.GetService<CacheContext>();

						if(cacheContext != null)
							await cacheContext.Database.EnsureDeletedAsync();
					}
				}
				// ReSharper restore UseAwaitUsing
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