// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows;
	using Autofac;
	using AutoMapper;
	using Caliburn.Micro;
	using chocolatey;
	using chocolatey.infrastructure.filesystem;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Startup;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.ViewModels.Items;
	using ChocolateyGui.Common.Windows.Utilities;
	using ChocolateyGui.Common.Windows.ViewModels;
	using LiteDB;
	using Serilog;

	public class Bootstrapper : BootstrapperBase
	{
		private static readonly IFileSystem _fileSystem = new DotNetFileSystem();
#pragma warning disable SA1202
		public static readonly String ChocolateyGuiInstallLocation = Bootstrapper._fileSystem.GetDirectoryName(Bootstrapper._fileSystem.GetCurrentAssemblyPath());
		public static readonly String ChocolateyInstallEnvironmentVariableName = "ChocolateyInstall";
#if FORCE_CHOCOLATEY_OFFICIAL_KEY
        // always look at the official location of the machine installation
        public static readonly string ChocolateyInstallLocation = Environment.GetEnvironmentVariable(ChocolateyInstallEnvironmentVariableName) ?? _fileSystem.GetDirectoryName(_fileSystem.GetCurrentAssemblyPath());
        public static readonly string LicensedGuiAssemblyLocation = _fileSystem.CombinePaths(ChocolateyInstallLocation, "extensions", "chocolateygui", "chocolateygui.licensed.dll");
#elif DEBUG
		public static readonly String ChocolateyInstallLocation = Bootstrapper._fileSystem.GetDirectoryName(Bootstrapper._fileSystem.GetCurrentAssemblyPath());
		public static readonly String LicensedGuiAssemblyLocation = Bootstrapper._fileSystem.CombinePaths(Bootstrapper.ChocolateyInstallLocation, "extensions", "chocolateygui", "chocolateygui.licensed.dll");
#else
        // Install locations is Chocolatey.dll or choco.exe - In Release mode
        // we might be testing on a server or in the local debugger. Either way,
        // start from the assembly location and if unfound, head to the machine
        // locations instead. This is a merge of official and Debug modes.
        private static Assembly _assemblyForLocation = Assembly.GetEntryAssembly() != null ? Assembly.GetEntryAssembly() : Assembly.GetExecutingAssembly();
        public static readonly string ChocolateyInstallLocation = _fileSystem.FileExists(_fileSystem.CombinePaths(_fileSystem.GetDirectoryName(_assemblyForLocation.CodeBase), "chocolatey.dll")) ||
                                                                  _fileSystem.FileExists(_fileSystem.CombinePaths(_fileSystem.GetDirectoryName(_assemblyForLocation.CodeBase), "choco.exe")) ?
                _fileSystem.GetDirectoryName(_assemblyForLocation.CodeBase) :
                !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ChocolateyInstallEnvironmentVariableName)) ?
                    Environment.GetEnvironmentVariable(ChocolateyInstallEnvironmentVariableName) :
                    @"C:\ProgramData\Chocolatey"
            ;

        // when being used as a reference, start by looking next to Chocolatey, then in a subfolder.
        public static readonly string LicensedGuiAssemblyLocation = _fileSystem.FileExists(_fileSystem.CombinePaths(ChocolateyInstallLocation, "chocolateygui.licensed.dll")) ? _fileSystem.CombinePaths(ChocolateyInstallLocation, "chocolateygui.licensed.dll") : _fileSystem.CombinePaths(ChocolateyInstallLocation, "extensions", "chocolateygui", "chocolateygui.licensed.dll");
#endif
		public static readonly String ChocolateyGuiCommonAssemblyLocation = Bootstrapper._fileSystem.CombinePaths(Bootstrapper.ChocolateyGuiInstallLocation, "ChocolateyGui.Common.dll");
		public static readonly String ChocolateyGuiCommonWindowsAssemblyLocation = Bootstrapper._fileSystem.CombinePaths(Bootstrapper.ChocolateyGuiInstallLocation, "ChocolateyGui.Common.Windows.dll");
		public static readonly String ChocolateyGuiCommonAssemblySimpleName = "ChocolateyGui.Common";
		public static readonly String ChocolateyGuiCommonWindowsAssemblySimpleName = "ChocolateyGui.Common.Windows";
		public static readonly String UnofficialChocolateyGuiPublicKey = "ffc115b9f4eb5c26";
		public static readonly String OfficialChocolateyGuiPublicKey = "dfd1909b30b79d8b";
		public static readonly String Name = "Chocolatey GUI";
		public static readonly String LicensedChocolateyGuiAssemblySimpleName = "chocolateygui.licensed";
#pragma warning restore SA1202

		public Bootstrapper()
		{
			Initialize();

			AppDomain.CurrentDomain.UnhandledException += Bootstrapper.CurrentDomain_UnhandledException;
		}

		public static IContainer Container { get; private set; }
		public static ILogger Logger { get; private set; }
		public static Boolean IsExiting { get; private set; }
		public static String ApplicationFilesPath { get; } = Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
		public static String AppDataPath { get; } = LogSetup.GetAppDataPath(Bootstrapper.Name);
		public static String LocalAppDataPath { get; } = LogSetup.GetLocalAppDataPath(Bootstrapper.Name);
		public static String UserConfigurationDatabaseName { get; } = "UserDatabase";
		public static String GlobalConfigurationDatabaseName { get; } = "GlobalDatabase";

		public Task OnExitAsync()
		{
			Bootstrapper.IsExiting = true;
			Log.CloseAndFlush();
			Bootstrapper.FinalizeDatabaseTransaction();
			Bootstrapper.Container.Dispose();
			return Task.FromResult(true);
		}

		protected override void Configure()
		{
			var logPath = LogSetup.GetLogsFolderPath("Logs");

			LogSetup.Execute();

			var directPath = Path.Combine(logPath, "ChocolateyGui.{Date}.log");

			var logConfig = new LoggerConfiguration()
			                .WriteTo.Async(config =>
				                               config.RollingFile(directPath, retainedFileCountLimit: 10, fileSizeLimitBytes: 150 * 1000 * 1000))
			                .SetDefaultLevel();

			Bootstrapper.Logger = Log.Logger = logConfig.CreateLogger();

#if FORCE_CHOCOLATEY_OFFICIAL_KEY
            var chocolateyGuiPublicKey = OfficialChocolateyGuiPublicKey;
#else
			var chocolateyGuiPublicKey = Bootstrapper.UnofficialChocolateyGuiPublicKey;
#endif

			Bootstrapper.Container = AutoFacConfiguration.RegisterAutoFac(Bootstrapper.LicensedChocolateyGuiAssemblySimpleName, Bootstrapper.LicensedGuiAssemblyLocation, chocolateyGuiPublicKey);
		}

		protected override async void OnStartup(Object sender, StartupEventArgs e)
		{
			try
			{
				// Do not remove! Load Chocolatey once so all config gets set
				// properly for future calls
				var choco = Lets.GetChocolatey(initializeLogging: false);

				Mapper.Initialize(config => { config.CreateMap<Package, IPackageViewModel>().ConstructUsing(rc => Bootstrapper.Container.Resolve<IPackageViewModel>()); });

				var packageService = Bootstrapper.Container.Resolve<IChocolateyService>();
				var features = await packageService.GetFeatures();

				var backgroundFeature = features.FirstOrDefault(feature => String.Equals(feature.Name, "useBackgroundService", StringComparison.OrdinalIgnoreCase));
				var elevationProvider = Elevation.Instance;
				elevationProvider.IsBackgroundRunning = backgroundFeature?.Enabled ?? false;

				var splashScreen = Bootstrapper.Container.Resolve<ISplashScreenService>();
				splashScreen.Close(TimeSpan.FromMilliseconds(300));

				DisplayRootViewFor<ShellViewModel>();
			}
			catch (Exception ex)
			{
				var messageFormat = Bootstrapper.L(nameof(Resources.Fatal_Startup_Error_Formatted), ex.Message);

				ChocolateyMessageBox.Show(messageFormat);
				Bootstrapper.Logger.Fatal(ex, Bootstrapper.L(nameof(Resources.Fatal_Startup_Error)));
				await OnExitAsync();
			}
		}

		protected override Object GetInstance(Type service, String key)
		{
			if (service == null)
			{
				throw new ArgumentNullException(nameof(service));
			}

			if (String.IsNullOrWhiteSpace(key))
			{
				if (Bootstrapper.Container.IsRegistered(service))
				{
					return Bootstrapper.Container.Resolve(service);
				}
			}
			else
			{
				if (Bootstrapper.Container.IsRegisteredWithName(key, service))
				{
					return Bootstrapper.Container.ResolveNamed(key, service);
				}
			}

			throw new Exception(Bootstrapper.L(
				                    nameof(Resources.Application_ContainerError),
				                    key ?? service.Name));
		}

		protected override IEnumerable<Object> GetAllInstances(Type service)
		{
			return Bootstrapper.Container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<Object>;
		}

		protected override void BuildUp(Object instance)
		{
			Bootstrapper.Container.InjectProperties(instance);
		}

		protected override void OnExit(Object sender, EventArgs e)
		{
			Bootstrapper.Logger.Information(Bootstrapper.L(nameof(Resources.Application_Exiting)));
		}

		private static void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e)
		{
			Bootstrapper.FinalizeDatabaseTransaction();
			if (e.IsTerminating)
			{
				Bootstrapper.Logger.Fatal(Bootstrapper.L(nameof(Resources.Application_UnhandledException)), e.ExceptionObject as Exception);
				if (Bootstrapper.IsExiting)
				{
					return;
				}

				var message = Bootstrapper.L(nameof(Resources.Bootstrapper_UnhandledException));

				ChocolateyMessageBox.Show(
					e.ExceptionObject.ToString(),
					message,
					MessageBoxButton.OK,
					MessageBoxImage.Error,
					MessageBoxResult.OK,
					MessageBoxOptions.ServiceNotification);
			}
			else
			{
				Bootstrapper.Logger.Error(Bootstrapper.L(nameof(Resources.Application_UnhandledException)), e.ExceptionObject as Exception);
			}
		}

		private static void FinalizeDatabaseTransaction()
		{
			if (Bootstrapper.Container != null)
			{
				if (Bootstrapper.Container.IsRegisteredWithName<LiteDatabase>(Bootstrapper.GlobalConfigurationDatabaseName))
				{
					var globalDatabase = Bootstrapper.Container.ResolveNamed<LiteDatabase>(Bootstrapper.GlobalConfigurationDatabaseName);
					globalDatabase.Dispose();
				}

				if (Bootstrapper.Container.IsRegisteredWithName<LiteDatabase>(Bootstrapper.UserConfigurationDatabaseName))
				{
					var userDatabase = Bootstrapper.Container.ResolveNamed<LiteDatabase>(Bootstrapper.UserConfigurationDatabaseName);
					userDatabase.Dispose();
				}
			}
		}

		private static String L(String key)
		{
			return TranslationSource.Instance[key];
		}

		private static String L(String key, params Object[] parameters)
		{
			return TranslationSource.Instance[key, parameters];
		}
	}
}