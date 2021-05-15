using System;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Helpers.Options
{
	public class ConfigureSqlServerCacheOptions : IConfigureOptions<SqlServerCacheOptions>
	{
		#region Constructors

		public ConfigureSqlServerCacheOptions(ISystemClock systemClock)
		{
			this.SystemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		}

		#endregion

		#region Properties

		protected internal virtual ISystemClock SystemClock { get; }

		#endregion

		#region Methods

		public virtual void Configure(SqlServerCacheOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			options.SystemClock = this.SystemClock;
		}

		#endregion
	}
}