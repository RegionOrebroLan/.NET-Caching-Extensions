using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.Data.Entities;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Extensions;
using TestHelpers;

namespace IntegrationTests.Distributed.Data
{
	[TestClass]
	public class SqliteCacheContextTest
	{
		#region Properties

		protected internal virtual string DataDirectoryPath => Global.DataDirectoryPath;

		#endregion

		#region Methods

		[TestMethod]
		public async Task AbsoluteExpiration_Test()
		{
			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {Id = "1", Value = Array.Empty<byte>(), AbsoluteExpiration = await DateTimeHelper.CreateUtcDateTimeAsync(2010, 1, 1)});
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {Id = "2", Value = Array.Empty<byte>(), AbsoluteExpiration = await DateTimeHelper.CreateUtcDateTimeAsync(2011, 1, 1)});
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {Id = "3", Value = Array.Empty<byte>(), AbsoluteExpiration = await DateTimeHelper.CreateUtcDateTimeAsync(2012, 1, 1)});
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {Id = "4", Value = Array.Empty<byte>()});
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {Id = "5", Value = Array.Empty<byte>()});
					await sqliteCacheContext.SaveChangesAsync();
				}

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					foreach(var cacheEntry in sqliteCacheContext.Cache)
					{
						if(cacheEntry.AbsoluteExpiration != null)
							Assert.AreEqual(DateTimeKind.Utc, cacheEntry.AbsoluteExpiration.Value.Kind);
					}
				}

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					var dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2015, 1, 1);
					Assert.AreEqual(5, await sqliteCacheContext.Cache.CountAsync(entry => entry.AbsoluteExpiration == null || entry.AbsoluteExpiration < dateTime));

					dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2015, 1, 1);
					Assert.AreEqual(3, await sqliteCacheContext.Cache.CountAsync(entry => entry.AbsoluteExpiration < dateTime));

					dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2011, 12, 31);
					Assert.AreEqual(4, await sqliteCacheContext.Cache.CountAsync(item => item.AbsoluteExpiration == null || item.AbsoluteExpiration.Value < dateTime));

					dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2011, 12, 31);
					Assert.AreEqual(2, await sqliteCacheContext.Cache.CountAsync(item => item.AbsoluteExpiration.Value < dateTime));

					dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2010, 12, 31);
					Assert.AreEqual(3, await sqliteCacheContext.Cache.CountAsync(item => item.AbsoluteExpiration == null || item.AbsoluteExpiration.Value < dateTime));

					dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2010, 12, 31);
					Assert.AreEqual(1, await sqliteCacheContext.Cache.CountAsync(item => item.AbsoluteExpiration.Value < dateTime));

					dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2009, 12, 31);
					Assert.AreEqual(2, await sqliteCacheContext.Cache.CountAsync(item => item.AbsoluteExpiration == null || item.AbsoluteExpiration.Value < dateTime));

					dateTime = await DateTimeHelper.CreateUtcDateTimeAsync(2009, 12, 31);
					Assert.AreEqual(0, await sqliteCacheContext.Cache.CountAsync(item => item.AbsoluteExpiration.Value < dateTime));
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

			return await Task.FromResult(serviceProvider);
		}

		[TestMethod]
		public async Task ExpiresAtTime_Default_Test()
		{
			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {Id = "1", Value = Array.Empty<byte>()});
					await sqliteCacheContext.SaveChangesAsync();
				}

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					Assert.AreEqual(DateTime.MinValue, (await sqliteCacheContext.Cache.FirstAsync()).ExpiresAtTime);
					Assert.AreEqual(DateTimeKind.Utc, (await sqliteCacheContext.Cache.FirstAsync()).ExpiresAtTime.Kind);
				}
			}
		}

		[TestMethod]
		public async Task ExpiresAtTime_Test()
		{
			var expiresAtTime = await DateTimeHelper.CreateUtcDateTimeAsync(2000);

			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {ExpiresAtTime = expiresAtTime, Id = "1", Value = Array.Empty<byte>()});
					await sqliteCacheContext.SaveChangesAsync();
				}

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					var cacheEntry = await sqliteCacheContext.Cache.FirstAsync();
					Assert.AreEqual(expiresAtTime, cacheEntry.ExpiresAtTime);
					Assert.AreEqual(DateTimeKind.Utc, cacheEntry.ExpiresAtTime.Kind);
				}
			}
		}

		[TestMethod]
		public async Task Id_ShouldBeCaseInsensitive()
		{
			await using(var serviceProvider = await this.CreateServiceProviderAsync())
			{
				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					sqliteCacheContext.Cache.Add(new CacheEntry<DateTime> {Id = "Test-Id", Value = Array.Empty<byte>()});
					await sqliteCacheContext.SaveChangesAsync();
				}

				await using(var sqliteCacheContext = serviceProvider.GetRequiredService<IDbContextFactory<SqliteCacheContext>>().CreateDbContext())
				{
					Assert.IsNotNull(await sqliteCacheContext.Cache.FindAsync("Test-id"));
					Assert.IsNotNull(await sqliteCacheContext.Cache.FindAsync("TEST-ID"));
					Assert.IsNotNull(await sqliteCacheContext.Cache.FindAsync("test-id"));
					Assert.IsNotNull(await sqliteCacheContext.Cache.FirstOrDefaultAsync(item => item.Id == "Test-id"));
					Assert.IsNotNull(await sqliteCacheContext.Cache.FirstOrDefaultAsync(item => item.Id == "TEST-ID"));
					Assert.IsNotNull(await sqliteCacheContext.Cache.FirstOrDefaultAsync(item => item.Id == "test-id"));
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