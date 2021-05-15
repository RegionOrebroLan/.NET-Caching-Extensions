using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;
using RegionOrebroLan.Caching.Distributed.Data;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	public abstract class FactoryDatabaseOptions<TDatabaseContext, TDateTime> : DatabaseOptions where TDatabaseContext : CacheContext<TDateTime> where TDateTime : struct
	{
		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			builder.Services.TryAddSingleton<ISystemClock, SystemClock>();
		}

		protected internal override void UseInternal(IApplicationBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			using(var cacheContext = builder.ApplicationServices.GetRequiredService<IDbContextFactory<TDatabaseContext>>().CreateDbContext())
			{
				cacheContext.Database.Migrate();
			}
		}

		#endregion
	}
}