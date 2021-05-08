using Microsoft.Extensions.Caching.Distributed;

namespace Application.Models.Views.DistributedCache
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

		public virtual Alert Alert { get; set; }
		public virtual IDistributedCache DistributedCache { get; }
		public virtual string Key { get; set; }
		public virtual Alert SetAlert { get; set; }
		public virtual SetForm SetForm { get; set; } = new SetForm();

		#endregion
	}
}