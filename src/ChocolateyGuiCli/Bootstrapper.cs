// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGuiCli
{
	using System;
	using System.IO;
	using System.Reflection;
	using Autofac;
	using chocolatey.infrastructure.filesystem;
	using ChocolateyGui.Common;
	using ChocolateyGui.Common.Startup;
	using ChocolateyGui.Common.Utilities;
	using Serilog;
	using Serilog.Events;

	public static class Bootstrapper
	{
		private static readonly IFileSystem _fileSystem = new DotNetFileSystem();
#pragma warning disable SA1202

		// Due to an unknown reason, we can not use chocolateys own get_current_assembly() function here,
		// as it will be returning the path to the choco.exe executable instead.
		public static readonly String ChocolateyGuiInstallLocation = Bootstrapper._fileSystem.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", String.Empty));
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
		internal static ILogger Logger { get; private set; }
		internal static IContainer Container { get; private set; }
		internal static String AppDataPath { get; } = LogSetup.GetAppDataPath(Bootstrapper.Name);
		internal static String LocalAppDataPath { get; } = LogSetup.GetLocalAppDataPath(Bootstrapper.Name);
		internal static String UserConfigurationDatabaseName { get; } = "UserDatabase";
		internal static String GlobalConfigurationDatabaseName { get; } = "GlobalDatabase";

		internal static void Configure()
		{
			var logPath = LogSetup.GetLogsFolderPath("Logs");

			LogSetup.Execute();

			var directPath = Path.Combine(logPath, "ChocolateyGuiCli.{Date}.log");

			var logConfig = new LoggerConfiguration()
			                .WriteTo.Sink(new ColouredConsoleSink(), LogEventLevel.Information)
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
	}
}