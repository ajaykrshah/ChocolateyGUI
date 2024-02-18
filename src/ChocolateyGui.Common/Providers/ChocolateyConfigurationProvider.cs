// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Providers
{
	using System;
	using System.Linq;
	using chocolatey.infrastructure.filesystem;

	public class ChocolateyConfigurationProvider : IChocolateyConfigurationProvider
	{
		#region Declarations

		private readonly IFileSystem _fileSystem;

		#endregion

		#region Constructors

		public ChocolateyConfigurationProvider(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;

			GetChocolateyInstallLocation();
			DetermineIfChocolateyExecutableIsBeingUsed();
		}

		#endregion

		#region IChocolateyConfigurationProvider implementation

		public String ChocolateyInstall { get; private set; }
		public Boolean IsChocolateyExecutableBeingUsed { get; private set; }

		#endregion

		#region Private implementation

		private void DetermineIfChocolateyExecutableIsBeingUsed()
		{
#if DEBUG
			this.IsChocolateyExecutableBeingUsed = true;
#else
			var exePath = _fileSystem.CombinePaths(this.ChocolateyInstall, "choco.exe");

			if (_fileSystem.FileExists(exePath))
			{
				this.IsChocolateyExecutableBeingUsed = true;
			}
#endif
		}

		private void GetChocolateyInstallLocation()
		{
#if DEBUG
			this.ChocolateyInstall = _fileSystem.GetDirectoryName(_fileSystem.GetCurrentAssemblyPath());
#else
			this.ChocolateyInstall = Environment.GetEnvironmentVariable("ChocolateyInstall");
			if (String.IsNullOrWhiteSpace(this.ChocolateyInstall))
			{
				var pathVar = Environment.GetEnvironmentVariable("PATH");
				if (!String.IsNullOrWhiteSpace(pathVar))
				{
					this.ChocolateyInstall =
						pathVar.Split(';')
						       .SingleOrDefault(
							       path => path.IndexOf("Chocolatey", StringComparison.OrdinalIgnoreCase) > -1);
				}
			}

			if (String.IsNullOrWhiteSpace(this.ChocolateyInstall))
			{
				this.ChocolateyInstall = String.Empty;
			}
#endif
		}

		#endregion
	}
}