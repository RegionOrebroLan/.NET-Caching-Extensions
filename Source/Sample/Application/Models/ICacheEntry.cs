using System;

namespace Application.Models
{
	public interface ICacheEntry
	{
		#region Properties

		string Key { get; }
		TimeSpan RetrievalTime { get; }
		string Value { get; }

		#endregion
	}
}