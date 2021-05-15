using System;
using Microsoft.EntityFrameworkCore;
using RegionOrebroLan.Caching.Distributed.Data.Entities;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	public class SqliteCacheContext : DateTimeCacheContext<SqliteCacheContext>
	{
		#region Constructors

		public SqliteCacheContext(DbContextOptions<SqliteCacheContext> options) : base(options) { }

		#endregion

		#region Methods

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			base.OnModelCreating(modelBuilder);

			var builder = modelBuilder.Entity<CacheEntry<DateTime>>();

			builder
				.Property(property => property.Id)
				.UseCollation("NOCASE");
		}

		#endregion
	}
}