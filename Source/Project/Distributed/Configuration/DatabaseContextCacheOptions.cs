using System;

namespace RegionOrebroLan.Caching.Distributed.Configuration
{
	public abstract class DatabaseContextCacheOptions
	{
		#region Properties

		public virtual TimeSpan? CleanupInterval { get; set; } = TimeSpan.FromMinutes(30);
		public virtual TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);

		#endregion
	}
}