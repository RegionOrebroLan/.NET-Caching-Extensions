using System;
using Microsoft.EntityFrameworkCore;
using RegionOrebroLan.Caching.Distributed.Data.Entities;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	public class SqlServerCacheContext : DateTimeOffsetCacheContext<SqlServerCacheContext>
	{
		#region Constructors

		public SqlServerCacheContext(DbContextOptions<SqlServerCacheContext> options) : base(options) { }

		#endregion

		#region Methods

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<CacheEntry<DateTimeOffset>>()
				.HasKey(entity => entity.Id)
				.IsClustered();
		}

		#endregion
	}
}