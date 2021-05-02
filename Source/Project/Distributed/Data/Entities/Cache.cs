using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RegionOrebroLan.Caching.Distributed.Data.Entities
{
	[CLSCompliant(false)]
	[SuppressMessage("Naming", "CA1724:Type names should not match namespaces")]
	public class Cache
	{
		#region Properties

		public virtual DateTimeOffset? AbsoluteExpiration { get; set; }
		public virtual DateTimeOffset ExpiresAtTime { get; set; }

		[MaxLength(449)]
		public virtual string Id { get; set; }

		public virtual long? SlidingExpirationInSeconds { get; set; }

		[Required]
		[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
		public virtual byte[] Value { get; set; }

		#endregion
	}
}