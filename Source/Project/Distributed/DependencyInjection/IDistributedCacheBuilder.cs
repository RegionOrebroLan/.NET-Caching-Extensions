using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegionOrebroLan.DependencyInjection;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection
{
	[CLSCompliant(false)]
	public interface IDistributedCacheBuilder
	{
		#region Properties

		IConfiguration Configuration { get; }
		string ConfigurationKey { get; }
		IHostEnvironment HostEnvironment { get; }
		IInstanceFactory InstanceFactory { get; }
		IServiceCollection Services { get; }

		#endregion

		#region Methods

		void Configure();

		#endregion
	}
}