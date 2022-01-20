using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
			await this.AddDistributedCacheTest(DistributedCacheKind.Memory, 3);
		}

		[TestMethod]
		public async Task AddDistributedCache_Redis_Test_1()
		{
			await this.AddRedisDistributedCacheTest(DistributedCacheKind.Redis1, 3);
		}

		[TestMethod]
		public async Task AddDistributedCache_Redis_Test_2()
		{
			await this.AddRedisDistributedCacheTest(DistributedCacheKind.Redis2, 3);
		}

		[TestMethod]
		public async Task AddDistributedCache_Redis_Test_3()
		{
			await this.AddRedisDistributedCacheTest(DistributedCacheKind.Redis3, 3);
		}

		[TestMethod]
		public async Task AddDistributedCache_Redis_Test_4()
		{
			await this.AddRedisDistributedCacheTest(DistributedCacheKind.Redis4, 3);
		}

		[TestMethod]
		public async Task AddDistributedCache_Sqlite_Test()
		{
#if NET6_0_OR_GREATER
			const int expectedNumberOfServices = 8;
#else
			const int expectedNumberOfServices = 7;
#endif
			await this.AddDistributedCacheTest(DistributedCacheKind.Sqlite, expectedNumberOfServices);
		}

		[TestMethod]
		public async Task AddDistributedCache_SqlServer_Test_1()
		{
			await this.AddDistributedCacheTest(DistributedCacheKind.SqlServer1, 6);
		}

		[TestMethod]
		public async Task AddDistributedCache_SqlServer_Test_2()
		{
			await this.AddDistributedCacheTest(DistributedCacheKind.SqlServer2, 6);
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

		protected internal virtual async Task AddRedisDistributedCacheTest(DistributedCacheKind distributedCacheKind, int expectedNumberOfServices)
		{
			if(distributedCacheKind is not (DistributedCacheKind.Redis1 or DistributedCacheKind.Redis2 or DistributedCacheKind.Redis3 or DistributedCacheKind.Redis4))
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
			foreach(var distributedCacheKind in new[] { DistributedCacheKind.Sqlite, DistributedCacheKind.SqlServer1 })
			{
				var configuration = Global.CreateConfiguration("appsettings.json", $"appsettings.{distributedCacheKind}.json");
				var services = Global.CreateServices(configuration);
				services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

				await using(var serviceProvider = services.BuildServiceProvider())
				{
					using(var scope = serviceProvider.CreateScope())
					{
						var sqliteCacheContext = scope.ServiceProvider.GetService<SqliteCacheContext>();

						if(sqliteCacheContext != null)
							await sqliteCacheContext.Database.EnsureDeletedAsync();

						var sqlServerCacheContext = scope.ServiceProvider.GetService<SqlServerCacheContext>();

						if(sqlServerCacheContext != null)
							await sqlServerCacheContext.Database.EnsureDeletedAsync();
					}
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