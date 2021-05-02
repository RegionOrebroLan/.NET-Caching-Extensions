using Microsoft.Extensions.Configuration;

namespace RegionOrebroLan.Caching.Distributed.Configuration
{
	public class DistributedCacheOptions
	{
		#region Properties

		public virtual IConfigurationSection Options { get; set; }
		public virtual DistributedCacheProvider Provider { get; set; } = DistributedCacheProvider.None;

		#endregion
	}
}