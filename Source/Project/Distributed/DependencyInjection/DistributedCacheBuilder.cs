using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegionOrebroLan.Caching.Configuration;
using RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration;
using RegionOrebroLan.Configuration;
using RegionOrebroLan.DependencyInjection;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection
{
	public class DistributedCacheBuilder : IDistributedCacheBuilder
	{
		#region Constructors

		public DistributedCacheBuilder(IConfiguration configuration, IHostEnvironment hostEnvironment, IInstanceFactory instanceFactory, IServiceCollection services)
		{
			this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
			this.InstanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
			this.Services = services ?? throw new ArgumentNullException(nameof(services));
		}

		#endregion

		#region Properties

		public virtual IConfiguration Configuration { get; }
		public virtual string ConfigurationKey { get; set; } = ConfigurationKeys.DistributedCachePath;
		public virtual IHostEnvironment HostEnvironment { get; }
		public virtual IInstanceFactory InstanceFactory { get; }
		public virtual IServiceCollection Services { get; }

		#endregion

		#region Methods

		public virtual void Configure()
		{
			try
			{
				var configurationSection = this.Configuration.GetSection(this.ConfigurationKey);

				var dynamicOptions = new DynamicOptions();
				configurationSection.Bind(dynamicOptions);

				var distributedCacheOptions = dynamicOptions.Type != null ? (DistributedCacheOptions)this.InstanceFactory.Create(dynamicOptions.Type) : new EmptyOptions();

				configurationSection.Bind(distributedCacheOptions);

				distributedCacheOptions.Add(this);

				this.Services.AddSingleton(distributedCacheOptions);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException("Could not configure distributed cache.", exception);
			}
		}

		#endregion
	}
}