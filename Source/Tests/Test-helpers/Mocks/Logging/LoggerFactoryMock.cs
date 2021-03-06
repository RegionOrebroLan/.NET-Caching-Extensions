using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TestHelpers.Mocks.Logging
{
	[SuppressMessage("Design", "CA1063:Implement IDisposable Correctly")]
	public class LoggerFactoryMock : ILoggerFactory
	{
		#region Constructors

		public LoggerFactoryMock(LoggerFactory internalLoggerFactory)
		{
			this.InternalLoggerFactory = internalLoggerFactory ?? throw new ArgumentNullException(nameof(internalLoggerFactory));
		}

		#endregion

		#region Properties

		public virtual bool Enabled { get; set; } = true;
		protected virtual LoggerFactory InternalLoggerFactory { get; }
		public virtual IEnumerable<LogMock> Logs => this.LogsInternal.ToArray();
		protected virtual IList<LogMock> LogsInternal { get; } = new List<LogMock>();

		#endregion

		#region Methods

		public virtual void AddProvider(ILoggerProvider provider)
		{
			this.InternalLoggerFactory.AddProvider(provider);
		}

		public virtual ILogger CreateLogger(string categoryName)
		{
			return new LoggerMock(this.InternalLoggerFactory.CreateLogger(categoryName), this.LogsInternal)
			{
				EnabledMode = LogLevelEnabledMode.Configuration
			};
		}

		[SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
		public virtual void Dispose()
		{
			this.InternalLoggerFactory.Dispose();
		}

		#endregion
	}
}