using System;
using Microsoft.EntityFrameworkCore;
using RegionOrebroLan.Caching.Distributed.Data.Entities;
using RegionOrebroLan.Caching.Extensions;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	public abstract class DateTimeCacheContext<T> : CacheContext<T, DateTime> where T : CacheContext<T, DateTime>
	{
		#region Constructors

		protected DateTimeCacheContext(DbContextOptions<T> options) : base(options) { }

		#endregion

		#region Methods

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			base.OnModelCreating(modelBuilder);

			var builder = modelBuilder.Entity<CacheEntry<DateTime>>();

			builder
				.Property(property => property.AbsoluteExpiration)
				.HasConversion(_ => _, absoluteExpiration => absoluteExpiration.SpecifyKind(DateTimeKind.Utc));

			builder
				.Property(property => property.ExpiresAtTime)
				.HasConversion(_ => _, expiresAtTime => DateTime.SpecifyKind(expiresAtTime, DateTimeKind.Utc));
		}

		#endregion
	}
}