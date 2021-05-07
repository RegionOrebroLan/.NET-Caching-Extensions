using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using NeoSmart.Caching.Sqlite;
using RegionOrebroLan.Caching.Distributed.DependencyInjection;
using RegionOrebroLan.IO.Extensions;

namespace RegionOrebroLan.Caching.Distributed.Configuration
{
	[CLSCompliant(false)]
	public class SqliteOptions : DatabaseOptions
	{
		#region Methods

		protected internal override void AddInternal(IDistributedCacheBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			builder.Services.AddSqliteCache(options =>
			{
				if(this.ConnectionStringName != null)
				{
					var connectionString = builder.Configuration.GetConnectionString(this.ConnectionStringName);

					var connectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);

					if(!string.IsNullOrEmpty(connectionStringBuilder.DataSource))
						options.CachePath = connectionStringBuilder.DataSource;
				}

				this.Options?.Bind(options);

				options.CachePath = options.CachePath.ResolveDataDirectorySubstitution();
			});
		}

		#endregion
	}
}