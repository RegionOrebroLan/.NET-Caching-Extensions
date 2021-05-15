using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Caching.Distributed.Configuration;
using RegionOrebroLan.Caching.Distributed.Data;

namespace RegionOrebroLan.Caching.Distributed
{
	public class SqliteCache : DateTimeContextCache<SqliteCacheContext, SqliteCacheOptions>
	{
		#region Constructors

		public SqliteCache(IDbContextFactory<SqliteCacheContext> databaseContextFactory, ILoggerFactory loggerFactory, IOptionsMonitor<SqliteCacheOptions> optionsMonitor, ISystemClock systemClock) : base(databaseContextFactory, TimeSpan.FromMinutes(30), loggerFactory, optionsMonitor, systemClock) { }

		#endregion
	}
}