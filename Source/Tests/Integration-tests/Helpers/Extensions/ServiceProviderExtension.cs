using System;
using Microsoft.Extensions.DependencyInjection;
using TestHelpers.Mocks;

namespace IntegrationTests.Helpers.Extensions
{
	public static class ServiceProviderExtension
	{
		#region Methods

		/// <summary>
		/// Use the system-time.
		/// </summary>
		/// <param name="serviceProvider"></param>
		public static void ResetTime(this IServiceProvider serviceProvider)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			serviceProvider.GetRequiredService<SystemClockMock>().Reset();
		}

		public static void SetTime(this IServiceProvider serviceProvider, DateTimeOffset now)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			serviceProvider.GetRequiredService<SystemClockMock>().UtcNow = now;
		}

		#endregion
	}
}