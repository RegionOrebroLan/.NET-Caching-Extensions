using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using TestHelpers.Mocks.Logging;

namespace IntegrationTests
{
	// ReSharper disable All
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
	public static class Global
	{
		#region Fields

		private static IConfiguration _configuration;
		private static IHostEnvironment _hostEnvironment;
		public static readonly string ProjectDirectoryPath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
		public static readonly string DataDirectoryPath = Path.Combine(ProjectDirectoryPath, "Test-data");

		#endregion

		#region Properties

		public static IConfiguration Configuration
		{
			get
			{
				if(_configuration == null)
					_configuration = CreateConfiguration("appsettings.json");

				return _configuration;
			}
		}

		public static IHostEnvironment HostEnvironment => _hostEnvironment ??= CreateHostEnvironment("Integration-tests");

		#endregion

		#region Methods

		public static IConfiguration CreateConfiguration(params string[] jsonFilePaths)
		{
			return CreateConfiguration(false, jsonFilePaths);
		}

		public static IConfiguration CreateConfiguration(bool optional, params string[] jsonFilePaths)
		{
			var configurationBuilder = CreateConfigurationBuilder(optional, jsonFilePaths);

			return configurationBuilder.Build();
		}

		public static IConfigurationBuilder CreateConfigurationBuilder(params string[] jsonFilePaths)
		{
			return CreateConfigurationBuilder(false, jsonFilePaths);
		}

		public static IConfigurationBuilder CreateConfigurationBuilder(bool optional, params string[] jsonFilePaths)
		{
			var configurationBuilder = new ConfigurationBuilder();
			configurationBuilder.Properties.Add("FileProvider", HostEnvironment.ContentRootFileProvider);

			foreach(var path in jsonFilePaths)
			{
				configurationBuilder.AddJsonFile(path, optional, true);
			}

			return configurationBuilder;
		}

		public static IHostEnvironment CreateHostEnvironment(string environmentName)
		{
			return new HostingEnvironment
			{
				ApplicationName = typeof(Global).Assembly.GetName().Name,
				ContentRootFileProvider = new PhysicalFileProvider(ProjectDirectoryPath),
				ContentRootPath = ProjectDirectoryPath,
				EnvironmentName = environmentName
			};
		}

		public static IServiceCollection CreateServices()
		{
			return CreateServices(Configuration);
		}

		public static IServiceCollection CreateServices(IConfiguration configuration)
		{
			var services = new ServiceCollection();

			services.AddSingleton(configuration);
			services.AddSingleton(HostEnvironment);
			services.AddSingleton<ILoggerFactory, LoggerFactoryMock>();
			services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

			return services;
		}

		#endregion
	}
	// ReSharper restore All
}