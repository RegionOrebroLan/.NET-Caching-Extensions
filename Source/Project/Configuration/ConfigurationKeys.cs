namespace RegionOrebroLan.Caching.Configuration
{
	public static class ConfigurationKeys
	{
		#region Fields

		public const string CachingPath = "Caching";
		public const string DistributedCachePath = CachingPath + ":DistributedCache";
		public const string MemoryCachePath = CachingPath + ":MemoryCache";

		#endregion
	}
}