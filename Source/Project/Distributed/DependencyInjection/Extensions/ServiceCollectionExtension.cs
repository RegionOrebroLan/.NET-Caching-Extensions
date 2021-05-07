using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegionOrebroLan.Caching.Configuration;
using RegionOrebroLan.DependencyInjection;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Extensions
{
	[CLSCompliant(false)]
	public static class ServiceCollectionExtension
	{
		#region Methods

		public static IDistributedCacheBuilder AddDistributedCache(this IServiceCollection services, IConfiguration configuration, IHostEnvironment hostEnvironment, IInstanceFactory instanceFactory)
		{
			return services.AddDistributedCache(configuration, ConfigurationKeys.DistributedCachePath, hostEnvironment, instanceFactory);
		}

		public static IDistributedCacheBuilder AddDistributedCache(this IServiceCollection services, IConfiguration configuration, string configurationKey, IHostEnvironment hostEnvironment, IInstanceFactory instanceFactory)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			var distributedCacheBuilder = new DistributedCacheBuilder(configuration, hostEnvironment, instanceFactory, services)
			{
				ConfigurationKey = configurationKey
			};

			distributedCacheBuilder.Configure();

			return distributedCacheBuilder;
		}

		#endregion
	}
}