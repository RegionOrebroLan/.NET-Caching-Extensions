namespace RegionOrebroLan.Caching.Distributed.DependencyInjection.Configuration
{
	public abstract class DatabaseOptions : ConnectionStringOptions
	{
		#region Properties

		public virtual string MigrationsAssembly { get; set; }

		#endregion
	}
}