using System;

namespace Application.Models
{
	public class CacheEntry : ICacheEntry
	{
		#region Constructors

		public CacheEntry(string key, TimeSpan retrievalTime, string value)
		{
			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.RetrievalTime = retrievalTime;
			this.Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		#endregion

		#region Properties

		public virtual string Key { get; }
		public virtual TimeSpan RetrievalTime { get; }
		public virtual string Value { get; }

		#endregion
	}
}