using System;
using Microsoft.EntityFrameworkCore;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	[CLSCompliant(false)]
	public class SqlServerCacheContext : CacheContext<SqlServerCacheContext>
	{
		#region Constructors

		public SqlServerCacheContext(DbContextOptions<SqlServerCacheContext> options) : base(options) { }

		#endregion
	}
}