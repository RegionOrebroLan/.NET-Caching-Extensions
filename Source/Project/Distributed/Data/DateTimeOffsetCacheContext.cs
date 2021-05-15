using System;
using Microsoft.EntityFrameworkCore;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	public abstract class DateTimeOffsetCacheContext<T> : CacheContext<T, DateTimeOffset> where T : CacheContext<T, DateTimeOffset>
	{
		#region Constructors

		protected DateTimeOffsetCacheContext(DbContextOptions<T> options) : base(options) { }

		#endregion
	}
}