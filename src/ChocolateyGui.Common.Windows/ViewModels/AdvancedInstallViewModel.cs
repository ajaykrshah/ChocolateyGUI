// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Input;
	using chocolatey;
	using ChocolateyGui.Common.Base;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Windows.Commands;
	using ChocolateyGui.Common.Windows.Controls.Dialogs;
	using ChocolateyGui.Common.Windows.Utilities;
	using NuGet.Versioning;

	public class AdvancedInstallViewModel : ObservableBase, IClosableChildWindow<AdvancedInstallViewModel>
	{
		#region Declarations

		private readonly IChocolateyService _chocolateyService;
		private readonly IPersistenceService _persistenceService;
		private Boolean _allowDowngrade;
		private Boolean _allowEmptyChecksums;
		private Boolean _allowEmptyChecksumsSecure;
		private Boolean _applyInstallArgumentsToDependencies;
		private Boolean _applyPackageParametersToDependencies;
		private List<String> _availableChecksumTypes;
		private NotifyTaskCompletion<ObservableCollection<String>> _availableVersions;
		private String _cacheLocation;
		private CancellationTokenSource _cts;
		private String _downloadChecksum;
		private String _downloadChecksum64bit;
		private String _downloadChecksumType;
		private String _downloadChecksumType64bit;
		private Int32 _executionTimeoutInSeconds;
		private Boolean _forceDependencies;
		private Boolean _forcex86;
		private Boolean _ignoreChecksums;
		private Boolean _ignoreDependencies;
		private Boolean _ignoreHttpCache;
		private Boolean _includePreRelease;
		private String _installArguments;
		private String _logFile;
		private Boolean _notSilent;
		private Boolean _overrideArguments;
		private String _packageParamaters;
		private readonly String _packageVersion;
		private Boolean _preRelease;
		private Boolean _requireChecksums;
		private String _selectedVersion;
		private Boolean _skipPowerShell;

		#endregion

		#region Constructors

		public AdvancedInstallViewModel(
			IChocolateyService chocolateyService,
			IPersistenceService persistenceService,
			NuGetVersion packageVersion)
		{
			_chocolateyService = chocolateyService;
			_persistenceService = persistenceService;

			_cts = new CancellationTokenSource();

			_packageVersion = packageVersion.ToNormalizedStringChecked();
			this.SelectedVersion = _packageVersion;

			FetchAvailableVersions();

			this.AvailableChecksumTypes = new List<String> { "md5", "sha1", "sha256", "sha512" };
			this.InstallCommand = new RelayCommand(
				o => { this.Close?.Invoke(this); },
				o => String.IsNullOrEmpty(this.SelectedVersion) || this.SelectedVersion == Resources.AdvancedChocolateyDialog_LatestVersion || NuGetVersion.TryParse(this.SelectedVersion, out _));
			this.CancelCommand = new RelayCommand(
				o =>
				{
					_cts.Cancel();
					this.Close?.Invoke(null);
				},
				o => true);

			this.BrowseLogFileCommand = new RelayCommand(BrowseLogFile);
			this.BrowseCacheLocationCommand = new RelayCommand(BrowseCacheLocation);

			SetDefaults();
		}

		#endregion

		#region Properties

		public Boolean AllowDowngrade
		{
			get => _allowDowngrade;
			set => SetPropertyValue(ref _allowDowngrade, value);
		}

		public Boolean AllowEmptyChecksums
		{
			get => _allowEmptyChecksums;
			set
			{
				SetPropertyValue(ref _allowEmptyChecksums, value);

				if (value)
				{
					this.RequireChecksums = false;
				}
			}
		}

		public Boolean AllowEmptyChecksumsSecure
		{
			get => _allowEmptyChecksumsSecure;
			set
			{
				SetPropertyValue(ref _allowEmptyChecksumsSecure, value);

				if (value)
				{
					this.RequireChecksums = false;
				}
			}
		}

		public Boolean ApplyInstallArgumentsToDependencies
		{
			get => _applyInstallArgumentsToDependencies;
			set => SetPropertyValue(ref _applyInstallArgumentsToDependencies, value);
		}

		public Boolean ApplyPackageParametersToDependencies
		{
			get => _applyPackageParametersToDependencies;
			set => SetPropertyValue(ref _applyPackageParametersToDependencies, value);
		}

		public List<String> AvailableChecksumTypes
		{
			get => _availableChecksumTypes;
			set => SetPropertyValue(ref _availableChecksumTypes, value);
		}

		public NotifyTaskCompletion<ObservableCollection<String>> AvailableVersions
		{
			get => _availableVersions;
			set => SetPropertyValue(ref _availableVersions, value);
		}

		public ICommand BrowseCacheLocationCommand { get; }
		public ICommand BrowseLogFileCommand { get; }

		public String CacheLocation
		{
			get => _cacheLocation;
			set => SetPropertyValue(ref _cacheLocation, value);
		}

		public ICommand CancelCommand { get; }

		public String DownloadChecksum
		{
			get => _downloadChecksum;
			set => SetPropertyValue(ref _downloadChecksum, value);
		}

		public String DownloadChecksum64bit
		{
			get => _downloadChecksum64bit;
			set => SetPropertyValue(ref _downloadChecksum64bit, value);
		}

		public String DownloadChecksumType
		{
			get => _downloadChecksumType;
			set
			{
				SetPropertyValue(ref _downloadChecksumType, value);
				this.DownloadChecksumType64bit = value;
			}
		}

		public String DownloadChecksumType64bit
		{
			get => _downloadChecksumType64bit;
			set => SetPropertyValue(ref _downloadChecksumType64bit, value);
		}

		public Int32 ExecutionTimeoutInSeconds
		{
			get => _executionTimeoutInSeconds;
			set => SetPropertyValue(ref _executionTimeoutInSeconds, value);
		}

		public Boolean ForceDependencies
		{
			get => _forceDependencies;
			set
			{
				SetPropertyValue(ref _forceDependencies, value);

				if (value)
				{
					this.IgnoreDependencies = false;
				}
			}
		}

		public Boolean Forcex86
		{
			get => _forcex86;
			set => SetPropertyValue(ref _forcex86, value);
		}

		public Boolean IgnoreChecksums
		{
			get => _ignoreChecksums;
			set
			{
				SetPropertyValue(ref _ignoreChecksums, value);

				if (value)
				{
					this.RequireChecksums = false;
				}
			}
		}

		public Boolean IgnoreDependencies
		{
			get => _ignoreDependencies;
			set
			{
				SetPropertyValue(ref _ignoreDependencies, value);

				if (value)
				{
					this.ForceDependencies = false;
				}
			}
		}

		public Boolean IgnoreHttpCache
		{
			get => _ignoreHttpCache;
			set => SetPropertyValue(ref _ignoreHttpCache, value);
		}

		public Boolean IgnoreHttpCacheIsAvailable => ChocolateyConfigurationExtensions.HasCacheExpirationInMinutes();

		public Boolean IncludePreRelease
		{
			get => _includePreRelease;
			set
			{
				if (SetPropertyValue(ref _includePreRelease, value))
				{
					_cts.Cancel();
					_cts.Dispose();
					_cts = new CancellationTokenSource();

					FetchAvailableVersions();
				}
			}
		}

		public String InstallArguments
		{
			get => _installArguments;
			set => SetPropertyValue(ref _installArguments, value);
		}

		public ICommand InstallCommand { get; }

		public String LogFile
		{
			get => _logFile;
			set => SetPropertyValue(ref _logFile, value);
		}

		public Boolean NotSilent
		{
			get => _notSilent;
			set
			{
				SetPropertyValue(ref _notSilent, value);

				if (value)
				{
					this.OverrideArguments = false;
				}
			}
		}

		public Boolean OverrideArguments
		{
			get => _overrideArguments;
			set
			{
				SetPropertyValue(ref _overrideArguments, value);

				if (value)
				{
					this.NotSilent = false;
				}
			}
		}

		public String PackageParameters
		{
			get => _packageParamaters;
			set => SetPropertyValue(ref _packageParamaters, value);
		}

		public Boolean PreRelease
		{
			get => _preRelease;
			set => SetPropertyValue(ref _preRelease, value);
		}

		public Boolean RequireChecksums
		{
			get => _requireChecksums;
			set
			{
				SetPropertyValue(ref _requireChecksums, value);

				if (value)
				{
					this.IgnoreChecksums = false;
					this.AllowEmptyChecksums = false;
					this.AllowEmptyChecksumsSecure = false;
				}
			}
		}

		public String SelectedVersion
		{
			get => _selectedVersion;
			set
			{
				SetPropertyValue(ref _selectedVersion, value);
				OnSelectedVersionChanged(value);
			}
		}

		public Boolean SkipPowerShell
		{
			get => _skipPowerShell;
			set
			{
				SetPropertyValue(ref _skipPowerShell, value);

				if (value)
				{
					this.OverrideArguments = false;
					this.NotSilent = false;
				}
			}
		}

		#endregion

		#region IClosableChildWindow<AdvancedInstallViewModel> implementation

		/// <inheritdoc />
		public Action<AdvancedInstallViewModel> Close { get; set; }

		#endregion

		#region Private implementation

		private void BrowseCacheLocation(Object value)
		{
			var description = L(nameof(Resources.AdvancedChocolateyDialog_CacheLocation_BrowseDescription));
			var cacheDirectory = _persistenceService.GetFolderPath(this.CacheLocation, description);

			if (!String.IsNullOrEmpty(cacheDirectory))
			{
				this.CacheLocation = cacheDirectory;
			}
		}

		private void BrowseLogFile(Object value)
		{
			var filter = "{0}|{1}|{2}".FormatWith(
				L(nameof(Resources.FilePicker_LogFiles)) + "|*.log;*.klg",
				L(nameof(Resources.FilePicker_TextFiles)) + "|*.txt;*.text;*.plain",
				L(nameof(Resources.FilePicker_AllFiles)) + "|*.*");

			var logFile = _persistenceService.GetFilePath("log", filter);

			if (!String.IsNullOrEmpty(logFile))
			{
				this.LogFile = logFile;
			}
		}

		private void FetchAvailableVersions()
		{
			var availableVersions = new ObservableCollection<String>();
			availableVersions.Add(Resources.AdvancedChocolateyDialog_LatestVersion);

			if (!String.IsNullOrEmpty(_packageVersion))
			{
				availableVersions.Add(_packageVersion);
			}

			this.AvailableVersions =
				new NotifyTaskCompletion<ObservableCollection<String>>(Task.FromResult(availableVersions));
		}

		private void OnSelectedVersionChanged(String stringVersion)
		{
			NuGetVersion version;

			if (NuGetVersion.TryParse(stringVersion, out version))
			{
				this.PreRelease = version.IsPrerelease;
			}
		}

		private void SetDefaults()
		{
			var choco = Lets.GetChocolatey();
			var config = choco.GetConfiguration();
			this.DownloadChecksumType = "md5";
			this.DownloadChecksumType64bit = "md5";
			this.ExecutionTimeoutInSeconds = config.CommandExecutionTimeoutSeconds;
			this.CacheLocation = config.CacheLocation;
			this.LogFile = config.AdditionalLogFileLocation;
		}

		#endregion
	}
}