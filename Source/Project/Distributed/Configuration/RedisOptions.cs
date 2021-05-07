using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RegionOrebroLan.Caching.Distributed.DependencyInjection;

namespace RegionOrebroLan.Caching.Distributed.Configuration
{
	[CLSCompliant(false)]
	public class RedisOptions : DistributedCacheOptions
	{
		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			builder.Services.AddDistributedRedisCache(options =>
			{
				this.Options?.Bind(options);
			});
		}

		#endregion
	}
}