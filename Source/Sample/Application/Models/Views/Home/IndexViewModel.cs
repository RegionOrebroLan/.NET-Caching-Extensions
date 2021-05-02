using System;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Models.Views.Home
{
	public class IndexViewModel
	{
		#region Constructors

		public IndexViewModel(IDistributedCache distributedCache)
		{
			this.DistributedCache = distributedCache;
		}

		#endregion

		#region Properties

		public virtual ICacheEntry CacheEntry { get; set; }
		public virtual IDistributedCache DistributedCache { get; }
		public virtual Exception Exception { get; set; }
		public virtual string Key { get; set; }

		#endregion
	}
}