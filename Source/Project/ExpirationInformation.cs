using System;

namespace RegionOrebroLan.Caching
{
	public class ExpirationInformation : IExpirationInformation
	{
		#region Properties

		public virtual DateTimeOffset? AbsoluteExpiration { get; set; }
		public virtual DateTimeOffset Expires { get; set; }
		public virtual long? SlidingExpirationInSeconds { get; set; }

		#endregion
	}
}