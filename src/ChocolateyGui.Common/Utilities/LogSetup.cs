// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Utilities
{
	using System;
	using System.IO;

	public static class LogSetup
	{
		#region Declarations

		private static String _appDataPath;
		private static String _localAppDataPath;
		private static String _logsFolderPath;

		#endregion

		#region Public interface

		public static void Execute()
		{
			if (!Directory.Exists(LogSetup._localAppDataPath))
			{
				Directory.CreateDirectory(LogSetup._localAppDataPath);
			}

			if (!Directory.Exists(LogSetup._logsFolderPath))
			{
				Directory.CreateDirectory(LogSetup._logsFolderPath);
			}
		}

		public static String GetAppDataPath(String applicationName)
		{
			LogSetup._appDataPath = Path.Combine(
				Environment.GetFolderPath(
					Environment.SpecialFolder.CommonApplicationData,
					Environment.SpecialFolderOption.DoNotVerify),
				applicationName);

			return LogSetup._appDataPath;
		}

		public static String GetLocalAppDataPath(String applicationName)
		{
			LogSetup._localAppDataPath = Path.Combine(
				Environment.GetFolderPath(
					Environment.SpecialFolder.LocalApplicationData,
					Environment.SpecialFolderOption.DoNotVerify),
				applicationName);

			return LogSetup._localAppDataPath;
		}

		public static String GetLogsFolderPath(String folderName)
		{
			LogSetup._logsFolderPath = Path.Combine(LogSetup._appDataPath, folderName);

			return LogSetup._logsFolderPath;
		}

		#endregion
	}
}