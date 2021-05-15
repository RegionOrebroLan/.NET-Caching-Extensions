using System;
using Microsoft.EntityFrameworkCore;
using RegionOrebroLan.Caching.Distributed.Data.Entities;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	public abstract class CacheContext<TDateTime> : DbContext where TDateTime : struct
	{
		#region Constructors

		protected CacheContext(DbContextOptions options) : base(options) { }

		#endregion

		#region Properties

		public virtual DbSet<CacheEntry<TDateTime>> Cache { get; set; }

		#endregion

		#region Methods

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<CacheEntry<TDateTime>>()
				.HasIndex(entity => entity.ExpiresAtTime);
		}

		#endregion
	}

	public abstract class CacheContext<T, TDateTime> : CacheContext<TDateTime> where T : CacheContext<TDateTime> where TDateTime : struct
	{
		#region Constructors

		protected CacheContext(DbContextOptions<T> options) : base(options) { }

		#endregion
	}
}