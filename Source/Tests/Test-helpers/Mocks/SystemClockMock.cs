using System;
using Microsoft.Extensions.Internal;

namespace TestHelpers.Mocks
{
	public class SystemClockMock : ISystemClock
	{
		#region Properties

		protected virtual ISystemClock InternalSystemClock { get; } = new SystemClock();
		protected virtual DateTimeOffset InternalUtcNow { get; set; }
		public virtual bool Systemized { get; set; } = true;

		public virtual DateTimeOffset UtcNow
		{
			get => this.Systemized ? this.InternalSystemClock.UtcNow : this.InternalUtcNow;
			set
			{
				this.Systemized = false;
				this.InternalUtcNow = value;
			}
		}

		#endregion

		#region Methods

		public virtual void Reset()
		{
			this.InternalUtcNow = DateTimeOffset.MinValue;
			this.Systemized = true;
		}

		#endregion
	}
}