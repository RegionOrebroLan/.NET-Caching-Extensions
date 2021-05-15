using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	public class RedisOptions : ConnectionStringOptions
	{
		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			builder.Services.AddDistributedRedisCache(options =>
			{
				this.Options?.Bind(options);

				var connectionString = builder.Configuration.GetConnectionString(this.ConnectionStringName);

				if(options.Configuration == null && connectionString != null)
					options.Configuration = connectionString;

				if(options.Configuration == null && options.ConfigurationOptions == null)
					options.ConfigurationOptions = new ConfigurationOptions();

				if(options.ConfigurationOptions == null)
					return;

				var endPointsSection = builder.Configuration.GetSection($"{builder.ConfigurationKey}:{nameof(this.Options)}:{nameof(options.ConfigurationOptions)}:{nameof(options.ConfigurationOptions.EndPoints)}");
				var endPoints = new Dictionary<string, int>();
				endPointsSection?.Bind(endPoints);

				foreach(var endPoint in endPoints)
				{
					options.ConfigurationOptions.EndPoints.Add(endPoint.Key, endPoint.Value);
				}
			});
		}

		#endregion
	}
}