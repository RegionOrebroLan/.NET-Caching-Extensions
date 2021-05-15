namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	/// <summary>
	/// Options used when no type is configured.
	/// </summary>
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