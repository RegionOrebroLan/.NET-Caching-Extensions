using System;
using RegionOrebroLan.Caching.Distributed.DependencyInjection;

namespace RegionOrebroLan.Caching.Distributed.Configuration
{
	/// <summary>
	/// Options used when no type is configured.
	/// </summary>
	[CLSCompliant(false)]
	public class EmptyOptions : DistributedCacheOptions
	{
		#region Properties

		protected internal override bool RemoveRegisteredService => false;

		#endregion

		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			// Do nothing.
		}

		#endregion
	}
}