using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	[CLSCompliant(false)]
	public class SqlServerCacheContextFactory : IDesignTimeDbContextFactory<SqlServerCacheContext>
	{
		#region Methods

		public virtual SqlServerCacheContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqlServerCacheContext>();
			optionsBuilder.UseSqlServer("A value that can not be empty just to be able to create/update migrations.");

			return new SqlServerCacheContext(optionsBuilder.Options);
		}

		#endregion
	}
}