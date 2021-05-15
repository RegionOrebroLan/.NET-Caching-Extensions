using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegionOrebroLan.Caching.Distributed.Configuration;
using RegionOrebroLan.Caching.Distributed.Data;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	public class SqliteOptions : FactoryDatabaseOptions<SqliteCacheContext, DateTime>
	{
		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			base.AddInternal(builder);

			builder.Services.AddDbContextFactory<SqliteCacheContext>(optionsBuilder =>
			{
				optionsBuilder.UseSqlite(
					builder.Configuration.GetConnectionString(this.ConnectionStringName),
					options =>
					{
						if(this.MigrationsAssembly != null)
							options.MigrationsAssembly(this.MigrationsAssembly);
					});
			});

			builder.Services.AddSingleton<IDistributedCache, SqliteCache>();

			builder.Services.Configure<SqliteCacheOptions>(options =>
			{
				this.Options?.Bind(options);
			});
		}

		#endregion
	}
}