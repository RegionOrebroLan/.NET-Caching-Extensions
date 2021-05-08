using System.ComponentModel.DataAnnotations;

namespace Application.Models.Views.DistributedCache
{
	public class SetForm
	{
		#region Fields

		public const int MaximumLengthForText = 255;

		#endregion

		#region Properties

		public virtual uint? AbsoluteExpirationInMinutes { get; set; }

		[MaxLength(MaximumLengthForText)]
		[Required(ErrorMessage = "\"{0}\" is required.")]
		public virtual string Key { get; set; }

		public virtual uint? SlidingExpirationInMinutes { get; set; }

		[MaxLength(MaximumLengthForText)]
		[Required(ErrorMessage = "\"{0}\" is required.")]
		public virtual string Value { get; set; }

		#endregion
	}
}