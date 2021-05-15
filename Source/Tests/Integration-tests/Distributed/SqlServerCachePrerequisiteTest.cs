using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationTests.Helpers.Extensions;
using IntegrationTests.Helpers.Options;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Extensions;
using TestHelpers;

namespace IntegrationTests.Distributed
{
	[TestClass]
	public class SqlServerCachePrerequisiteTest
	{
		#region Properties

		protected internal virtual string DataDirectoryPath => Global.DataDirectoryPath;

		#endregion

		#region Methods

		protected internal virtual async Task<ServiceProvider> CreateServiceProviderAsync(DateTimeOffset? now = null)
		{
			var jsonFilePaths = new List<string>
			{
				"appsettings.json",
				"appsettings.SqlServer1.json"
			};

			var configuration = Global.CreateConfiguration(jsonFilePaths.ToArray());
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());
			services.AddSingleton<IConfigureOptions<SqlServerCacheOptions>, ConfigureSqlServerCacheOptions>();
			var serviceProvider = services.BuildServiceProvider();

			if(now != null)
				serviceProvider.SetTime(now.Value);

			var options = serviceProvider.GetRequiredService<DistributedCacheOptions>();
			options.Use(new ApplicationBuilder(serviceProvider));

			return await Task.FromResult(serviceProvider);
		}

		[TestMethod]
		public async Task GetAsync_Test()
		{
			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var sqlServerCache = serviceProvider.GetRequiredService<IDistributedCache>();
				var value = await sqlServerCache.GetAsync("Test");
				Assert.IsNull(value);
			}
		}

		[TestMethod]
		public async Task Set_Test()
		{
			var now = await DateTimeOffsetHelper.CreateDateTimeOffsetAsync(2000);

			await using(var serviceProvider = await this.CreateServiceProviderAsync(now))
			{
				var sqlServerCacheOptions = serviceProvider.GetRequiredService<IOptions<SqlServerCacheOptions>>().Value;
				Assert.AreEqual(TimeSpan.FromMinutes(20), sqlServerCacheOptions.DefaultSlidingExpiration);
				Assert.IsNull(sqlServerCacheOptions.ExpiredItemsDeletionInterval);

				var sqlServerCache = (SqlServerCache)serviceProvider.GetRequiredService<IDistributedCache>();
				// ReSharper disable MethodHasAsyncOverload
				sqlServerCache.Set("1", Array.Empty<byte>(), new DistributedCacheEntryOptions());
				// ReSharper restore MethodHasAsyncOverload

				using(var scope = serviceProvider.CreateScope())
				{
					var sqlServerCacheContext = scope.ServiceProvider.GetRequiredService<SqlServerCacheContext>();
					Assert.AreEqual(1, await sqlServerCacheContext.Cache.CountAsync());
					var cacheEntry = await sqlServerCacheContext.Cache.FindAsync("1");
					Assert.IsNotNull(cacheEntry);
					Assert.IsNull(cacheEntry.AbsoluteExpiration);
					Assert.AreEqual(now.Add(sqlServerCacheOptions.DefaultSlidingExpiration), cacheEntry.ExpiresAtTime);
					Assert.AreEqual("1", cacheEntry.Id);
					Assert.AreEqual((uint)sqlServerCacheOptions.DefaultSlidingExpiration.TotalSeconds, cacheEntry.SlidingExpirationInSeconds);
					Assert.IsTrue(Array.Empty<byte>().SequenceEqual(cacheEntry.Value));
				}
			}
		}

		[TestMethod]
		public async Task SetAsync_Test()
		{
			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				var sqlServerCache = (SqlServerCache)serviceProvider.GetRequiredService<IDistributedCache>();
				// ReSharper disable MethodHasAsyncOverload
				sqlServerCache.Set("1", Array.Empty<byte>(), new DistributedCacheEntryOptions());
				// ReSharper restore MethodHasAsyncOverload
			}
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			var configuration = Global.CreateConfiguration("appsettings.json", "appsettings.SqlServer1.json");
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			await using(var serviceProvider = services.BuildServiceProvider())
			{
				using(var scope = serviceProvider.CreateScope())
				{
					await scope.ServiceProvider.GetRequiredService<SqlServerCacheContext>().Database.EnsureDeletedAsync();
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