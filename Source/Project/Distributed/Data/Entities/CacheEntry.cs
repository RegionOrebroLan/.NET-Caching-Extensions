using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RegionOrebroLan.Caching.Distributed.Data.Entities
{
	public class CacheEntry<TDateTime> where TDateTime : struct
	{
		#region Properties

		public virtual TDateTime? AbsoluteExpiration { get; set; }
		public virtual TDateTime ExpiresAtTime { get; set; }

		[MaxLength(449)]
		public virtual string Id { get; set; }

		public virtual long? SlidingExpirationInSeconds { get; set; }

		[Required]
		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		public virtual byte[] Value { get; set; }

		#endregion
	}
}