using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegionOrebroLan.Caching.Distributed.Configuration;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Extensions;

namespace IntegrationTests.Distributed.Configuration
{
	[TestClass]
	public class SqlServerOptionsTest
	{
		#region Properties

		protected internal virtual string DataDirectoryPath => Global.DataDirectoryPath;

		#endregion

		#region Methods

		[TestMethod]
		public async Task BindSqlServerCacheOptions_IfConfigurationIsEmpty_ShouldWorkProperly()
		{
			await Task.CompletedTask;
			var configurationBuilder = Global.CreateConfigurationBuilder("appsettings.json", "appsettings.SqlServer-1.json");
			//configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			//	{ });
			var configuration = configurationBuilder.Build();
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());
			// ReSharper disable UseAwaitUsing
			using(var serviceProvider = services.BuildServiceProvider())
			{
				var sqlServerOptions = (SqlServerOptions)serviceProvider.GetRequiredService<DistributedCacheOptions>();
				var sqlServerCacheOptions = new SqlServerCacheOptions();
				sqlServerOptions.BindSqlServerCacheOptions(new ConfigurationBuilder().Build(), sqlServerCacheOptions);
				Assert.IsNull(sqlServerCacheOptions.ConnectionString);
				Assert.AreEqual("dbo", sqlServerCacheOptions.SchemaName);
				Assert.AreEqual("Cache", sqlServerCacheOptions.TableName);
			}
			// ReSharper restore UseAwaitUsing
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			await Task.CompletedTask;

			var configuration = Global.CreateConfiguration("appsettings.json", $"appsettings.SqlServer-1.json");
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());

			// ReSharper disable UseAwaitUsing
			using(var serviceProvider = services.BuildServiceProvider())
			{
				using(var scope = serviceProvider.CreateScope())
				{
					var cacheContext = scope.ServiceProvider.GetService<CacheContext>();

					if(cacheContext != null)
						await cacheContext.Database.EnsureDeletedAsync();
				}
			}
			// ReSharper restore UseAwaitUsing

			AppDomain.CurrentDomain.SetDataDirectory(null);
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			await Task.CompletedTask;

			AppDomain.CurrentDomain.SetDataDirectory(this.DataDirectoryPath);
		}

		[TestMethod]
		public async Task UseInternal_IfMigrationsAssemblyHasMigrations_ShouldWorkProperly()
		{
			var configurationBuilder = Global.CreateConfigurationBuilder("appsettings.json", "appsettings.SqlServer-1.json");
			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			{
				{"Caching:DistributedCache:MigrationsAssembly", typeof(SqlServerOptions).Assembly.GetName().Name}
			});
			var configuration = configurationBuilder.Build();
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());
			// ReSharper disable All
			using(var serviceProvider = services.BuildServiceProvider())
			{
				var sqlServerOptions = (SqlServerOptions)serviceProvider.GetRequiredService<DistributedCacheOptions>();
				sqlServerOptions.UseInternal(new ApplicationBuilder(serviceProvider));
				var distributedCache = serviceProvider.GetRequiredService<IDistributedCache>();
				const string key = "Key";
				const string value = "Value";
				await distributedCache.SetStringAsync(key, value);
				var actualValue = await distributedCache.GetStringAsync(key);
				Assert.AreEqual(value, actualValue);
			}
			// ReSharper restore All
		}

		[TestMethod]
		[ExpectedException(typeof(SqlException))]
		public async Task UseInternal_IfMigrationsAssemblyHasNoMigrations_ShouldNotAddAnyDatabaseObjectsAndEndUpWithASqlException_WhenSettingTheCache()
		{
			await Task.CompletedTask;
			var configurationBuilder = Global.CreateConfigurationBuilder("appsettings.json", "appsettings.SqlServer-1.json");
			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			{
				{"Caching:DistributedCache:MigrationsAssembly", this.GetType().Assembly.GetName().Name}
			});
			var configuration = configurationBuilder.Build();
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());
			// ReSharper disable All
			using(var serviceProvider = services.BuildServiceProvider())
			{
				var sqlServerOptions = (SqlServerOptions)serviceProvider.GetRequiredService<DistributedCacheOptions>();
				sqlServerOptions.UseInternal(new ApplicationBuilder(serviceProvider));
				var distributedCache = serviceProvider.GetRequiredService<IDistributedCache>();
				const string key = "Key";
				const string value = "Value";
				try
				{
					distributedCache.SetString(key, value);
				}
				catch(SqlException sqlException)
				{
					if(sqlException.Message.Equals("Invalid object name 'dbo.Cache'.", StringComparison.Ordinal))
						throw;
				}
			}
			// ReSharper restore All
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task UseInternal_IfMigrationsAssemblyIsAnEmptyString_ShouldThrowAnArgumentException()
		{
			await Task.CompletedTask;
			var configurationBuilder = Global.CreateConfigurationBuilder("appsettings.json", "appsettings.SqlServer-1.json");
			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			{
				{"Caching:DistributedCache:MigrationsAssembly", ""}
			});
			var configuration = configurationBuilder.Build();
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());
			// ReSharper disable UseAwaitUsing
			using(var serviceProvider = services.BuildServiceProvider())
			{
				var sqlServerOptions = (SqlServerOptions)serviceProvider.GetRequiredService<DistributedCacheOptions>();
				sqlServerOptions.UseInternal(new ApplicationBuilder(serviceProvider));
			}
			// ReSharper restore UseAwaitUsing
		}

		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public async Task UseInternal_IfMigrationsAssemblyIsAnInvalidAssemblyName_ShouldThrowAFileNotFoundException()
		{
			await Task.CompletedTask;
			var configurationBuilder = Global.CreateConfigurationBuilder("appsettings.json", "appsettings.SqlServer-1.json");
			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
			{
				{"Caching:DistributedCache:MigrationsAssembly", "b27cbf67-4fef-4267-bed7-e44e761b0abc"}
			});
			var configuration = configurationBuilder.Build();
			var services = Global.CreateServices(configuration);
			services.AddDistributedCache(configuration, Global.HostEnvironment, new InstanceFactory());
			// ReSharper disable UseAwaitUsing
			using(var serviceProvider = services.BuildServiceProvider())
			{
				var sqlServerOptions = (SqlServerOptions)serviceProvider.GetRequiredService<DistributedCacheOptions>();
				sqlServerOptions.UseInternal(new ApplicationBuilder(serviceProvider));
			}
			// ReSharper restore UseAwaitUsing
		}

		#endregion
	}
}