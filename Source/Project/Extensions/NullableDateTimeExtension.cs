using System;

namespace RegionOrebroLan.Caching.Extensions
{
	public static class NullableDateTimeExtension
	{
		#region Methods

		public static DateTime? SpecifyKind(this DateTime? dateTime, DateTimeKind kind)
		{
			if(dateTime == null)
				return null;

			return DateTime.SpecifyKind(dateTime.Value, kind);
		}

		#endregion
	}
}