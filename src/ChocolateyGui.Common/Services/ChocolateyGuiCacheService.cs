// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;
	using chocolatey.infrastructure.filesystem;

	public class ChocolateyGuiCacheService : IChocolateyGuiCacheService
	{
		#region Declarations

		private readonly IFileStorageService _fileStorageService;
		private readonly IFileSystem _fileSystem;
		private readonly String _localAppDataPath = String.Empty;

		#endregion

		#region Constructors

		public ChocolateyGuiCacheService(IFileStorageService fileStorageService, IFileSystem fileSystem)
		{
			_fileStorageService = fileStorageService;
			_fileSystem = fileSystem;

			_localAppDataPath = _fileSystem.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify), "Chocolatey GUI");
		}

		#endregion

		#region IChocolateyGuiCacheService implementation

		public void PurgeIcons()
		{
			_fileStorageService.DeleteAllFiles();
		}

		public void PurgeOutdatedPackages()
		{
			var outdatedPackagesFile = _fileSystem.CombinePaths(_localAppDataPath, "outdatedPackages.xml");
			var outdatedPackagesBackupFile = _fileSystem.CombinePaths(_localAppDataPath, "outdatedPackages.xml.backup");

			if (_fileSystem.FileExists(outdatedPackagesFile))
			{
				_fileSystem.DeleteFile(outdatedPackagesFile);
			}

			if (_fileSystem.FileExists(outdatedPackagesBackupFile))
			{
				_fileSystem.DeleteFile(outdatedPackagesBackupFile);
			}
		}

		#endregion
	}
}