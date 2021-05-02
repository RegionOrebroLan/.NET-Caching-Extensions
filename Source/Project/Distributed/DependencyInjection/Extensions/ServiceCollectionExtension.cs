using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using RegionOrebroLan.Caching.Configuration;
using RegionOrebroLan.Caching.Configuration.Extensions;
using RegionOrebroLan.Caching.Distributed.Configuration;
using RegionOrebroLan.Caching.Distributed.Configuration.Extensions;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Extensions;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions
{
	public static class ServiceCollectionExtension
	{
		#region Methods

		public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration, string configurationKey = ConfigurationKeys.DistributedCachePath)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var distributedCacheConfigurationSection = configuration.GetSection(configurationKey);
			services.Configure<DistributedCacheOptions>(distributedCacheConfigurationSection);
			var distributedCacheOptions = new DistributedCacheOptions();
			distributedCacheConfigurationSection.Bind(distributedCacheOptions);

			services.AddDistributedCache(configuration, distributedCacheOptions.Options, distributedCacheOptions.Provider);

			return services;
		}

		public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration, IConfigurationSection optionsSection, DistributedCacheProvider provider)
		{
			return provider switch
			{
				DistributedCacheProvider.None => services,
				DistributedCacheProvider.Memory => services.AddMemoryDistributedCache(optionsSection),
				DistributedCacheProvider.Redis => services.AddRedisDistributedCache(optionsSection),
				DistributedCacheProvider.Sqlite => services.AddSqliteDistributedCache(configuration, optionsSection),
				DistributedCacheProvider.SqlServer => services.AddSqlServerDistributedCache(configuration, optionsSection),
				_ => throw new ArgumentOutOfRangeException(nameof(provider), provider, $"\"{provider}\" is not a supported distributed-cache provider.")
			};
		}

		public static IServiceCollection AddMemoryDistributedCache(this IServiceCollection services, IConfigurationSection optionsSection)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(optionsSection == null)
				throw new ArgumentNullException(nameof(optionsSection));

			return services.AddDistributedMemoryCache(optionsSection.Bind);
		}

		public static IServiceCollection AddRedisDistributedCache(this IServiceCollection services, IConfigurationSection optionsSection)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(optionsSection == null)
				throw new ArgumentNullException(nameof(optionsSection));

			return services.AddDistributedRedisCache(optionsSection.Bind);
		}

		public static IServiceCollection AddSqliteDistributedCache(this IServiceCollection services, IConfiguration configuration, IConfigurationSection optionsSection)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(optionsSection == null)
				throw new ArgumentNullException(nameof(optionsSection));

			return services.AddSqliteCache(options =>
			{
				var connectionString = configuration.GetConnectionString(optionsSection);

				if(!string.IsNullOrWhiteSpace(connectionString))
				{
					var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);

					if(!string.IsNullOrEmpty(connectionStringBuilder.DataSource))
						options.CachePath = connectionStringBuilder.DataSource;
				}

				optionsSection.Bind(options);

				options.CachePath = ResolveDataDirectorySubstitution(options.CachePath);
			});
		}

		public static IServiceCollection AddSqlServerDistributedCache(this IServiceCollection services, IConfiguration configuration, IConfigurationSection optionsSection)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var sqlServerCacheOptions = new SqlServerCacheOptions();
			BindSqlServerCacheOptions(configuration, optionsSection, sqlServerCacheOptions);

			services.AddDbContext<SqlServerCacheContext>(builder =>
			{
				builder.UseSqlServer(sqlServerCacheOptions.ConnectionString);
			});
			services.AddScoped<ICacheContext, SqlServerCacheContext>();

			return services.AddDistributedSqlServerCache(options =>
			{
				BindSqlServerCacheOptions(configuration, optionsSection, options);
			});
		}

		[CLSCompliant(false)]
		public static void BindSqlServerCacheOptions(IConfiguration configuration, IConfigurationSection optionsSection, SqlServerCacheOptions sqlServerCacheOptions)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(optionsSection == null)
				throw new ArgumentNullException(nameof(optionsSection));

			if(sqlServerCacheOptions == null)
				throw new ArgumentNullException(nameof(sqlServerCacheOptions));

			sqlServerCacheOptions.SetDefaults();

			sqlServerCacheOptions.ConnectionString = configuration.GetConnectionString(optionsSection);

			optionsSection.Bind(sqlServerCacheOptions);

			var connectionString = sqlServerCacheOptions.ConnectionString;

			if(string.IsNullOrWhiteSpace(connectionString))
				return;

			var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

			if(string.IsNullOrEmpty(connectionStringBuilder.AttachDBFilename))
				return;

			connectionStringBuilder.AttachDBFilename = ResolveDataDirectorySubstitution(connectionStringBuilder.AttachDBFilename);

			if(string.IsNullOrEmpty(connectionStringBuilder.InitialCatalog))
				connectionStringBuilder.InitialCatalog = connectionStringBuilder.AttachDBFilename;

			sqlServerCacheOptions.ConnectionString = connectionStringBuilder.ToString();
		}

		[SuppressMessage("Style", "IDE0057:Use range operator")]
		public static string ResolveDataDirectorySubstitution(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
				return value;

			if(!value.StartsWith(ConnectionStringSubstitutions.DataDirectory, StringComparison.OrdinalIgnoreCase))
				return value;

			var dataDirectory = AppDomain.CurrentDomain.GetDataDirectory();

			if(dataDirectory == null)
				throw new InvalidOperationException("The data-directory is not set for the application-domain.");

			var slashCharacters = new[] {'/', '\\'};

			return dataDirectory.TrimEnd(slashCharacters) + '\\' + value.Substring(ConnectionStringSubstitutions.DataDirectory.Length).TrimStart(slashCharacters);
		}

		#endregion
	}
}