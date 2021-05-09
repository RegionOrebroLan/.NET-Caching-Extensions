using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using RegionOrebroLan.DependencyInjection.Extensions;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	[CLSCompliant(false)]
	public abstract class DistributedCacheOptions
	{
		#region Properties

		public virtual IConfigurationSection Options { get; set; }
		protected internal virtual bool RemoveRegisteredService => true;

		#endregion

		#region Methods

		public virtual void Add(IDistributedCacheBuilder builder)
		{
			try
			{
				if(builder == null)
					throw new ArgumentNullException(nameof(builder));

				if(this.RemoveRegisteredService)
					builder.Services.Remove<IDistributedCache>();

				this.AddInternal(builder);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not add distributed cache with options of type \"{this.GetType()}\".", exception);
			}
		}

		protected internal abstract void AddInternal(IDistributedCacheBuilder builder);

		public virtual void Use(IApplicationBuilder builder)
		{
			try
			{
				this.UseInternal(builder);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException($"Could not use distributed cache with options of type \"{this.GetType()}\".", exception);
			}
		}

		protected internal virtual void UseInternal(IApplicationBuilder builder) { }

		#endregion
	}
}