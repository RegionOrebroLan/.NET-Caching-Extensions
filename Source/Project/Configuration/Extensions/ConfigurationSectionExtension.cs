using System;
using Microsoft.Extensions.Configuration;

namespace RegionOrebroLan.Caching.Configuration.Extensions
{
	public static class ConfigurationSectionExtension
	{
		#region Methods

		public static string GetConnectionStringName(this IConfigurationSection configurationSection)
		{
			if(configurationSection == null)
				throw new ArgumentNullException(nameof(configurationSection));

			return configurationSection.GetSection("ConnectionStringName").Value;
		}

		#endregion
	}
}