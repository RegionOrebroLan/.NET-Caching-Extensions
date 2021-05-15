using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegionOrebroLan.Caching.Distributed.Data;
using RegionOrebroLan.Caching.Distributed.DependencyInjection;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using TestHelpers.Mocks.Distributed.Configuration;

namespace TestHelpers.Mocks.Distributed.DependencyInjection.Configuration
{
	public class DateTimeOffsetContextOptionsMock : FactoryDatabaseOptions<SqlServerCacheContext, DateTimeOffset>
	{
		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			base.AddInternal(builder);

			var sqlServerOptions = new SqlServerOptions
			{
				ConnectionStringName = this.ConnectionStringName,
				MigrationsAssembly = this.MigrationsAssembly,
				Options = this.Options
			};

			var sqlServerCacheOptions = new SqlServerCacheOptions();
			sqlServerOptions.BindSqlServerCacheOptions(builder.Configuration, sqlServerCacheOptions);

			builder.Services.AddDbContextFactory<SqlServerCacheContext>(optionsBuilder =>
			{
				optionsBuilder.UseSqlServer(sqlServerCacheOptions.ConnectionString,
					options =>
					{
						if(this.MigrationsAssembly != null)
							options.MigrationsAssembly(this.MigrationsAssembly);
					});
			});

			builder.Services.AddSingleton<IDistributedCache, DateTimeOffsetCacheMock>();

			builder.Services.Configure<DateTimeOffsetCacheOptionsMock>(options =>
			{
				this.Options?.Bind(options);
			});
		}

		#endregion
	}
}