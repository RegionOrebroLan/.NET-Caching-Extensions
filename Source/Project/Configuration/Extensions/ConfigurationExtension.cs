using System;
using Microsoft.Extensions.Configuration;

namespace RegionOrebroLan.Caching.Configuration.Extensions
{
	public static class ConfigurationExtension
	{
		#region Methods

		public static string GetConnectionString(this IConfiguration configuration, IConfigurationSection optionsSection)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(optionsSection == null)
				throw new ArgumentNullException(nameof(optionsSection));

			return configuration.GetConnectionString(optionsSection.GetConnectionStringName());
		}

		#endregion
	}
}