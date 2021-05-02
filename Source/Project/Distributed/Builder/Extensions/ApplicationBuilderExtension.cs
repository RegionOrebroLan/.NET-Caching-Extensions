using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Caching.Distributed.Configuration;
using RegionOrebroLan.Caching.Distributed.Data;

namespace RegionOrebroLan.Caching.Distributed.Builder.Extensions
{
	public static class ApplicationBuilderExtension
	{
		#region Methods

		[CLSCompliant(false)]
		public static IApplicationBuilder UseDistributedCache(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			var distributedCacheOptions = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<DistributedCacheOptions>>();
			var provider = distributedCacheOptions.Value.Provider;

			return provider switch
			{
				DistributedCacheProvider.None => applicationBuilder,
				DistributedCacheProvider.Memory => applicationBuilder.UseMemoryDistributedCache(),
				DistributedCacheProvider.Redis => applicationBuilder.UseRedisDistributedCache(),
				DistributedCacheProvider.Sqlite => applicationBuilder.UseSqliteDistributedCache(),
				DistributedCacheProvider.SqlServer => applicationBuilder.UseSqlServerDistributedCache(),
				_ => throw new ArgumentOutOfRangeException(nameof(applicationBuilder), provider, $"\"{provider}\" is not a supported distributed-cache provider.")
			};
		}

		[CLSCompliant(false)]
		public static IApplicationBuilder UseMemoryDistributedCache(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			return applicationBuilder;
		}

		[CLSCompliant(false)]
		public static IApplicationBuilder UseRedisDistributedCache(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			return applicationBuilder;
		}

		[CLSCompliant(false)]
		public static IApplicationBuilder UseSqliteDistributedCache(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			return applicationBuilder;
		}

		[CLSCompliant(false)]
		public static IApplicationBuilder UseSqlServerDistributedCache(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			using(var scope = applicationBuilder.ApplicationServices.CreateScope())
			{
				scope.ServiceProvider.GetRequiredService<ICacheContext>().Migrate();
			}

			return applicationBuilder;
		}

		#endregion
	}
}