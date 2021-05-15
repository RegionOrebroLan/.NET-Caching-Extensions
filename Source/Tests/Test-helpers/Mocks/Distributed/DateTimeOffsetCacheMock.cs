using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Caching.Distributed;
using RegionOrebroLan.Caching.Distributed.Data;
using TestHelpers.Mocks.Distributed.Configuration;

namespace TestHelpers.Mocks.Distributed
{
	public class DateTimeOffsetCacheMock : DateTimeOffsetContextCache<SqlServerCacheContext, DateTimeOffsetCacheOptionsMock>
	{
		#region Constructors

		public DateTimeOffsetCacheMock(IDbContextFactory<SqlServerCacheContext> databaseContextFactory, ILoggerFactory loggerFactory, IOptionsMonitor<DateTimeOffsetCacheOptionsMock> optionsMonitor, ISystemClock systemClock) : base(databaseContextFactory, TimeSpan.FromMinutes(30), loggerFactory, optionsMonitor, systemClock) { }

		#endregion
	}
}