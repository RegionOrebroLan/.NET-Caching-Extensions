using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Models;
using Application.Models.Views.DistributedCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace Application.Controllers
{
	public class DistributedCacheController : Controller
	{
		#region Constructors

		public DistributedCacheController(ISystemClock systemClock)
		{
			this.SystemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		}

		#endregion

		#region Properties

		protected internal virtual ISystemClock SystemClock { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<Alert> CreateAlertAsync(string information, AlertMode mode, string heading = null, IEnumerable<string> details = null)
		{
			var alert = await Task.FromResult(new Alert
			{
				Heading = heading ?? await this.GetHeadingAsync(mode),
				Information = information,
				Mode = mode
			});

			foreach(var item in details ?? Enumerable.Empty<string>())
			{
				alert.Details.Add(item);
			}

			return alert;
		}

		protected internal virtual async Task<DistributedCacheEntryOptions> CreateCacheOptionsAsync(uint? absoluteExpirationInMinutes, uint? slidingExpirationInMinutes)
		{
			var options = new DistributedCacheEntryOptions();

			if(absoluteExpirationInMinutes != null)
				options.AbsoluteExpirationRelativeToNow = absoluteExpirationInMinutes.Value == 0 ? TimeSpan.FromMilliseconds(1) : TimeSpan.FromMinutes(absoluteExpirationInMinutes.Value);

			if(slidingExpirationInMinutes != null)
				options.SlidingExpiration = slidingExpirationInMinutes.Value == 0 ? TimeSpan.FromMilliseconds(1) : TimeSpan.FromMinutes(slidingExpirationInMinutes.Value);

			return await Task.FromResult(options);
		}

		protected internal virtual async Task<Alert> CreateConfirmationAlertAsync(string information, string heading = null, IEnumerable<string> details = null)
		{
			return await this.CreateAlertAsync(information, AlertMode.Success, heading, details);
		}

		protected internal virtual async Task<Alert> CreateErrorAlertAsync(string error, string heading = null, IEnumerable<string> details = null)
		{
			return await this.CreateAlertAsync(error, AlertMode.Danger, heading, details);
		}

		protected internal virtual async Task<IndexViewModel> CreateModelAsync(string key = null)
		{
			var distributedCache = await this.GetDistributedCacheAsync();

			var model = new IndexViewModel(distributedCache);

			// ReSharper disable All
			if(key != null)
			{
				if(model.DistributedCache == null)
				{
					model.Alert = await this.CreateErrorAlertAsync("The IDistributedCache is not setup as a service.");
				}
				else
				{
					model.Key = key;

					var start = this.SystemClock.UtcNow;
					var value = distributedCache.GetString(key);
					var stop = this.SystemClock.UtcNow;

					if(value != null)
					{
						var details = new List<string>
						{
							$"Value: <strong>{value}</strong>",
							$"Retrieval-time: <strong>{stop - start}</strong>"
						};

						model.Alert = await this.CreateConfirmationAlertAsync($"The key \"{key}\" exists in the cache.", $"Key: \"{key}\"", details);
					}
					else
					{
						model.Alert = await this.CreateWarningAlertAsync($"The key \"{key}\" does not exist in the cache.", $"Key: \"{key}\"");
					}
				}
			}
			// ReSharper restore All

			return model;
		}

		protected internal virtual async Task<string> CreateUrlAsync(string action, string fragment, object values = null)
		{
			return await Task.FromResult(this.Url.Action(action, null, values, null, null, fragment));
		}

		protected internal virtual async Task<Alert> CreateWarningAlertAsync(string warning, string heading = null)
		{
			return await this.CreateAlertAsync(warning, AlertMode.Warning, heading);
		}

		/// <summary>
		/// We do not inject IDistributedCache in the constructor. Maybe it is not set up as a service. So we try to get it from the service-provider.
		/// </summary>
		/// <returns></returns>
		protected internal virtual async Task<IDistributedCache> GetDistributedCacheAsync()
		{
			return await Task.FromResult(this.HttpContext.RequestServices.GetService<IDistributedCache>());
		}

		protected internal virtual async Task<string> GetHeadingAsync(AlertMode mode)
		{
			await Task.CompletedTask;

			return mode switch
			{
				AlertMode.Danger => "Error",
				AlertMode.Info => "Information",
				AlertMode.Success => "Confirmation",
				AlertMode.Warning => "Warning",
				_ => "Unknown"
			};
		}

		public virtual async Task<IActionResult> Index(string key)
		{
			var model = await this.CreateModelAsync(key);

			return await Task.FromResult(this.View(model));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Remove(string key)
		{
			var url = await this.CreateUrlAsync("Index", "get", key != null ? new {key} : null);

			// ReSharper disable InvertIf
			if(key != null)
			{
				var distributedCache = await this.GetDistributedCacheAsync();

				if(distributedCache != null)
					await distributedCache.RemoveAsync(key);
			}
			// ReSharper restore InvertIf

			return this.Redirect(url);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Set(SetForm form)
		{
			var model = await this.CreateModelAsync();

			if(model.DistributedCache == null)
			{
				model.SetAlert = await this.CreateErrorAlertAsync("The IDistributedCache is not setup as a service.");
			}
			else if(form == null)
			{
				model.SetAlert = await this.CreateErrorAlertAsync("The form is null.");
			}
			else if(!this.ModelState.IsValid)
			{
				model.SetAlert = await this.CreateErrorAlertAsync("Input-error", null, this.ModelState.SelectMany(entry => entry.Value.Errors).Select(item => item.ErrorMessage));
			}
			else
			{
				try
				{
					var options = await this.CreateCacheOptionsAsync(form.AbsoluteExpirationInMinutes, form.SlidingExpirationInMinutes);

					await model.DistributedCache.SetStringAsync(form.Key, form.Value, options);

					return this.Redirect(await this.CreateUrlAsync("Index", "get", new {key = form.Key}));
				}
				catch(Exception exception)
				{
					model.SetAlert = await this.CreateErrorAlertAsync(new InvalidOperationException("Could not add cache-entry.", exception).ToString());
				}
			}

			model.SetForm = form;

			return this.View("Index", model);
		}

		#endregion
	}
}