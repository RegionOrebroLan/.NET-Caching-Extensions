using System;

namespace RegionOrebroLan.Caching
{
	public interface IExpirationInformation
	{
		#region Properties

		DateTimeOffset? AbsoluteExpiration { get; }
		DateTimeOffset Expires { get; }
		long? SlidingExpirationInSeconds { get; }

		#endregion
	}
}