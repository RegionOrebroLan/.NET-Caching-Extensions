namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	public abstract class ConnectionStringOptions : DistributedCacheOptions
	{
		#region Properties

		public virtual string ConnectionStringName { get; set; }

		#endregion
	}
}