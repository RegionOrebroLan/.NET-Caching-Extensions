using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	[CLSCompliant(false)]
	public class MemoryOptions : DistributedCacheOptions
	{
		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			builder.Services.AddDistributedMemoryCache(options =>
			{
				this.Options?.Bind(options);
			});
		}

		#endregion
	}
}