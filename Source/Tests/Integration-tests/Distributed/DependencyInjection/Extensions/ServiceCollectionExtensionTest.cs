using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSmart.Caching.Sqlite;
using RegionOrebroLan.Caching.Distributed.Configuration;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Extensions;

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
			await Task.CompletedTask;
			var configuration = Global.CreateConfiguration("appsettings.json");
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());
			// ReSharper disable UseAwaitUsing
			using(var serviceProvider = services.BuildServiceProvider())
			{
				var distributedCacheOptions = serviceProvider.GetRequiredService<DistributedCacheOptions>();
				Assert.IsTrue(distributedCacheOptions is EmptyOptions);
				distributedCacheOptions.Use(new ApplicationBuilder(serviceProvider));
				var distributedCache = serviceProvider.GetService<IDistributedCache>();
				Assert.IsNull(distributedCache);
			}
			// ReSharper restore UseAwaitUsing
		}

		[TestMethod]
		public async Task AddDistributedCache_Memory_Test()
		{
			await this.AddDistributedCache_Test("Memory", 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_Sqlite_Test_1()
		{
			await this.AddDistributedCache_Test("Sqlite-1", 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_Sqlite_Test_2()
		{
			await this.AddDistributedCache_Test("Sqlite-2", 8);
		}

		[TestMethod]
		public async Task AddDistributedCache_SqlServer_Test_1()
		{
			await this.AddDistributedCache_Test("SqlServer-1", 11);
		}

		[TestMethod]
		public async Task AddDistributedCache_SqlServer_Test_2()
		{
			await this.AddDistributedCache_Test("SqlServer-2", 11);
		}

		protected internal virtual async Task AddDistributedCache_Test(string environment, int numberOfServices)
		{
			var configuration = Global.CreateConfiguration("appsettings.json", $"appsettings.{environment}.json");

			var services = Global.CreateServices(configuration);

			var numberOfServicesBefore = services.Count;

			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			Assert.AreEqual(numberOfServices, services.Count - numberOfServicesBefore);

			// ReSharper disable UseAwaitUsing
			using(var serviceProvider = services.BuildServiceProvider())
			{
				var distributedCacheOptions = serviceProvider.GetRequiredService<DistributedCacheOptions>();

				distributedCacheOptions.Use(new ApplicationBuilder(serviceProvider));

				var distributedCache = serviceProvider.GetRequiredService<IDistributedCache>();

				const string key = "Key";
				const string value = "Value";
				await distributedCache.SetStringAsync(key, value);
				var actualValue = await distributedCache.GetStringAsync(key);
				Assert.AreEqual(value, actualValue);
			}
			// ReSharper restore UseAwaitUsing
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			foreach(var environment in new[] {"Sqlite-1", "SqlServer-1"})
			{
				var configuration = Global.CreateConfiguration("appsettings.json", $"appsettings.{environment}.json");
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