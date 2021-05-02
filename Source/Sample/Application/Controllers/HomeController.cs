using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Application.Models;
using Application.Models.Views.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace Application.Controllers
{
	public class HomeController : Controller
	{
		#region Constructors

		public HomeController(ISystemClock systemClock)
		{
			this.SystemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		}

		#endregion

		#region Properties

		protected internal virtual ISystemClock SystemClock { get; }

		#endregion

		#region Methods

		[HttpPost]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public virtual async Task<IActionResult> Add(int? absoluteExpirationInMinutes, string key, int? slidingExpirationInMinutes, string value)
		{
			var model = await this.CreateModelAsync();

			if(model.DistributedCache == null)
				throw new InvalidOperationException("The IDistributedCache is not setup as a service.");

			try
			{
				var options = await this.CreateCacheOptionsAsync(absoluteExpirationInMinutes, slidingExpirationInMinutes);

				await model.DistributedCache.SetStringAsync(key, value, options);
			}
			catch(Exception exception)
			{
				model.Exception = new InvalidOperationException("Could not add cache-entry.", exception);

				return this.View("Index", model);
			}

			return this.RedirectToAction("Index", new {key});
		}

		protected internal virtual async Task<DistributedCacheEntryOptions> CreateCacheOptionsAsync(int? absoluteExpirationInMinutes, int? slidingExpirationInMinutes)
		{
			var options = new DistributedCacheEntryOptions();

			if(absoluteExpirationInMinutes != null)
				options.AbsoluteExpirationRelativeToNow = absoluteExpirationInMinutes.Value == 0 ? TimeSpan.FromMilliseconds(1) : TimeSpan.FromMinutes(absoluteExpirationInMinutes.Value);

			if(slidingExpirationInMinutes != null)
				options.SlidingExpiration = slidingExpirationInMinutes.Value == 0 ? TimeSpan.FromMilliseconds(1) : TimeSpan.FromMinutes(slidingExpirationInMinutes.Value);

			return await Task.FromResult(options);
		}

		protected internal virtual async Task<IndexViewModel> CreateModelAsync(string key = null)
		{
			var distributedCache = await this.GetDistributedCacheAsync();

			var model = new IndexViewModel(distributedCache);

			// ReSharper disable All
			if(key != null)
			{
				model.Key = key;

				var start = this.SystemClock.UtcNow;
				var value = distributedCache.GetString(key);
				var stop = this.SystemClock.UtcNow;

				if(value != null)
					model.CacheEntry = new CacheEntry(key, stop - start, value);
			}
			// ReSharper restore All

			return model;
		}

		/// <summary>
		/// We do not inject IDistributedCache in the constructor. Maybe it is not set up as a service. So we try to get it from the service-provider.
		/// </summary>
		/// <returns></returns>
		protected internal virtual async Task<IDistributedCache> GetDistributedCacheAsync()
		{
			return await Task.FromResult(this.HttpContext.RequestServices.GetService<IDistributedCache>());
		}

		public virtual async Task<IActionResult> Index(string key)
		{
			var model = await this.CreateModelAsync(key);

			return await Task.FromResult(this.View(model));
		}

		[HttpPost]
		public virtual async Task<IActionResult> Remove(string key)
		{
			var distributedCache = await this.GetDistributedCacheAsync();

			if(distributedCache != null)
				await distributedCache.RemoveAsync(key);

			return this.RedirectToAction("Index", new {key});
		}

		#endregion
	}
}