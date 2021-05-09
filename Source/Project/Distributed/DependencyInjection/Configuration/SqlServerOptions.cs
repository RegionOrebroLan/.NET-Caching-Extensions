using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegionOrebroLan.Caching.Distributed.Configuration.Extensions;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.IO.Extensions;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	[CLSCompliant(false)]
	public class SqlServerOptions : DatabaseOptions
	{
		#region Properties

		public virtual string MigrationsAssembly { get; set; }

		#endregion

		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			var sqlServerCacheOptions = new SqlServerCacheOptions();
			this.BindSqlServerCacheOptions(builder.Configuration, sqlServerCacheOptions);

			builder.Services.AddDbContext<CacheContext, SqlServerCacheContext>(optionsBuilder =>
			{
				optionsBuilder.UseSqlServer(
					sqlServerCacheOptions.ConnectionString,
					options =>
					{
						if(this.MigrationsAssembly != null)
							options.MigrationsAssembly(this.MigrationsAssembly);
					}
				);
			});

			builder.Services.AddDistributedSqlServerCache(options =>
			{
				this.BindSqlServerCacheOptions(builder.Configuration, options);
			});
		}

		protected internal virtual void BindSqlServerCacheOptions(IConfiguration configuration, SqlServerCacheOptions sqlServerCacheOptions)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(sqlServerCacheOptions == null)
				throw new ArgumentNullException(nameof(sqlServerCacheOptions));

			sqlServerCacheOptions.SetDefaults();

			if(this.ConnectionStringName != null)
				sqlServerCacheOptions.ConnectionString = configuration.GetConnectionString(this.ConnectionStringName);

			this.Options?.Bind(sqlServerCacheOptions);

			if(string.IsNullOrWhiteSpace(sqlServerCacheOptions.ConnectionString))
				return;

			var connectionStringBuilder = new SqlConnectionStringBuilder(sqlServerCacheOptions.ConnectionString);

			if(string.IsNullOrEmpty(connectionStringBuilder.AttachDBFilename))
				return;

			connectionStringBuilder.AttachDBFilename = connectionStringBuilder.AttachDBFilename.ResolveDataDirectorySubstitution();

			if(string.IsNullOrEmpty(connectionStringBuilder.InitialCatalog))
				connectionStringBuilder.InitialCatalog = connectionStringBuilder.AttachDBFilename;

			sqlServerCacheOptions.ConnectionString = connectionStringBuilder.ToString();
		}

		protected internal override void UseInternal(IApplicationBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			using(var scope = builder.ApplicationServices.CreateScope())
			{
				scope.ServiceProvider.GetRequiredService<CacheContext>().Database.Migrate();
			}
		}

		#endregion
	}
}