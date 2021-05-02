using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RegionOrebroLan.Caching.Distributed.Data.Entities;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	[CLSCompliant(false)]
	public abstract class CacheContext : DbContext, ICacheContext
	{
		#region Constructors

		protected CacheContext(DbContextOptions options) : base(options) { }

		#endregion

		#region Properties

		public virtual DbSet<Cache> Cache { get; set; }

		#endregion

		#region Methods

		public virtual void Migrate()
		{
			this.Database.Migrate();
		}

		public virtual async Task MigrateAsync(CancellationToken cancellationToken = default)
		{
			await this.Database.MigrateAsync(cancellationToken);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			base.OnModelCreating(modelBuilder);

			var builder = modelBuilder.Entity<Cache>();

			builder
				.HasKey(entity => entity.Id)
				.IsClustered();

			builder
				.HasIndex(entity => entity.ExpiresAtTime);
		}

		#endregion
	}

	[CLSCompliant(false)]
	public abstract class CacheContext<T> : CacheContext where T : CacheContext
	{
		#region Constructors

		protected CacheContext(DbContextOptions<T> options) : base(options) { }

		#endregion
	}
}