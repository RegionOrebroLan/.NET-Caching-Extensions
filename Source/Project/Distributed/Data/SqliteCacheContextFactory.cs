using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	public class SqliteCacheContextFactory : IDesignTimeDbContextFactory<SqliteCacheContext>
	{
		#region Methods

		public virtual SqliteCacheContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqliteCacheContext>();
			optionsBuilder.UseSqlite("A value that can not be empty just to be able to create/update migrations.");

			return new SqliteCacheContext(optionsBuilder.Options);
		}

		#endregion
	}
}