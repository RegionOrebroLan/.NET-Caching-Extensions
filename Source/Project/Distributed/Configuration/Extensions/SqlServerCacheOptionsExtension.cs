using System;
using Microsoft.Extensions.Caching.SqlServer;
using RegionOrebroLan.Caching.Distributed.Data.Entities;

namespace RegionOrebroLan.Caching.Distributed.Configuration.Extensions
{
	public static class SqlServerCacheOptionsExtension
	{
		#region Methods

		[CLSCompliant(false)]
		public static void SetDefaults(this SqlServerCacheOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			options.SchemaName = "dbo";
			options.TableName = nameof(Cache);
		}

		#endregion
	}
}