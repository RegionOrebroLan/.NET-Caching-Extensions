using System;
using System.Threading;
using System.Threading.Tasks;

namespace RegionOrebroLan.Caching.Distributed.Data
{
	[CLSCompliant(false)]
	public interface ICacheContext
	{
		#region Methods

		void Migrate();
		Task MigrateAsync(CancellationToken cancellationToken = default);

		#endregion
	}
}