using System;
using System.Threading.Tasks;

namespace TestHelpers
{
	public static class DateTimeHelper
	{
		#region Methods

		public static async Task<DateTime> CreateUtcDateTimeAsync(int year, int month = 1, int day = 1, int hour = 0, int minute = 0, int second = 0, int milliSecond = 0)
		{
			return await Task.FromResult(new DateTime(year, month, day, hour, minute, second, milliSecond, DateTimeKind.Utc));
		}

		#endregion
	}
}