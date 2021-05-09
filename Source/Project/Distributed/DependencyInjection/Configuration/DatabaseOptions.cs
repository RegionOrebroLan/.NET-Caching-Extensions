using System;

namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	[CLSCompliant(false)]
	public abstract class DatabaseOptions : DistributedCacheOptions
	{
		#region Properties

		public virtual string ConnectionStringName { get; set; }

		#endregion
	}
}