using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using RegionOrebroLan.Logging.Extensions;

namespace RegionOrebroLan.Caching.Distributed.Builder.Extensions
{
	public static class ApplicationBuilderExtension
	{
		#region Methods

		[CLSCompliant(false)]
		public static IApplicationBuilder UseDistributedCache(this IApplicationBuilder applicationBuilder)
		{
			try
			{
				if(applicationBuilder == null)
					throw new ArgumentNullException(nameof(applicationBuilder));

				var distributedCacheOptions = applicationBuilder.ApplicationServices.GetRequiredService<DistributedCacheOptions>();
				var logger = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ApplicationBuilderExtension));

				distributedCacheOptions.Use(applicationBuilder);

				logger.LogDebugIfEnabled($"Distributed cache options are {distributedCacheOptions.GetType()}.");
				var distributedCache = applicationBuilder.ApplicationServices.GetService<IDistributedCache>();
				logger.LogDebugIfEnabled($"Registered service for {nameof(IDistributedCache)} is {distributedCache?.GetType().FullName ?? "null"}.");

				return applicationBuilder;
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException("Could not use distributed cache.", exception);
			}
		}

		#endregion
	}
}