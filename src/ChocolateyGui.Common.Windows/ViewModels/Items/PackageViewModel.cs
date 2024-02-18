// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels.Items
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.Caching;
	using System.Threading.Tasks;
	using AutoMapper;
	using Caliburn.Micro;
	using chocolatey;
	using ChocolateyGui.Common.Base;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.ViewModels.Items;
	using ChocolateyGui.Common.Windows.Services;
	using ChocolateyGui.Common.Windows.Views;
	using MahApps.Metro.Controls.Dialogs;
	using NuGet.Versioning;
	using Serilog;
	using Action = System.Action;

	[DebuggerDisplay("Id = {Id}, Version = {Version}")]
	public class PackageViewModel :
		ObservableBase,
		IPackageViewModel,
		IHandle<PackageHasUpdateMessage>,
		IHandle<FeatureModifiedMessage>
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<PackageViewModel>();
		private readonly IAllowedCommandsService _allowedCommandsService;
		private readonly MemoryCache _cache = MemoryCache.Default;
		private readonly IChocolateyGuiCacheService _chocolateyGuiCacheService;
		private readonly IChocolateyService _chocolateyService;
		private readonly IConfigService _configService;
		private readonly IDialogService _dialogService;
		private readonly IEventAggregator _eventAggregator;
		private readonly IMapper _mapper;
		private readonly IPackageArgumentsService _packageArgumentsService;
		private readonly IPersistenceService _persistenceService;
		private readonly IProgressService _progressService;
		private String[] _authors;
		private String _copyright;
		private DateTime _created;
		private String _dependencies;
		private String _description;
		private Int64 _downloadCount;
		private String _galleryDetailsUrl;
		private String _iconUrl = String.Empty;
		private String _id;
		private Int32 _index;
		private Boolean _isInstalled;
		private Boolean _isOutdated;
		private Boolean _isPinned;
		private Boolean _isPrerelease;
		private String _language;
		private DateTime _lastUpdated;
		private NuGetVersion _latestVersion;
		private String _licenseUrl = String.Empty;
		private String[] _owners;
		private String _packageHash;
		private String _packageHashAlgorithm;
		private Int64 _packageSize;
		private String _projectUrl = String.Empty;
		private DateTimeOffset _published;
		private String _releaseNotes;
		private String _reportAbuseUrl = String.Empty;
		private String _requireLicenseAcceptance;
		private Uri _source;
		private String _summary;
		private String _tags;
		private String _title;
		private NuGetVersion _version;
		private Int64 _versionDownloadCount;

		#endregion

		#region Constructors

		public PackageViewModel(
			IChocolateyService chocolateyService,
			IEventAggregator eventAggregator,
			IMapper mapper,
			IDialogService dialogService,
			IProgressService progressService,
			IChocolateyGuiCacheService chocolateyGuiCacheService,
			IConfigService configService,
			IAllowedCommandsService allowedCommandsService,
			IPackageArgumentsService packageArgumentsService,
			IPersistenceService persistenceService)
		{
			_chocolateyService = chocolateyService;
			_eventAggregator = eventAggregator;
			_mapper = mapper;
			_dialogService = dialogService;
			_progressService = progressService;
			eventAggregator?.Subscribe(this);
			_chocolateyGuiCacheService = chocolateyGuiCacheService;
			_configService = configService;
			_allowedCommandsService = allowedCommandsService;
			_packageArgumentsService = packageArgumentsService;
			_persistenceService = persistenceService;
		}

		#endregion

		#region Properties

		public Boolean CanInstall => !this.IsInstalled;
		public Boolean CanPin => !this.IsPinned && this.IsInstalled;
		public Boolean CanReinstall => this.IsInstalled;
		public Boolean CanUninstall => this.IsInstalled;
		public Boolean CanUnpin => this.IsPinned && this.IsInstalled;

		public DateTime Created
		{
			get => _created;
			set => SetPropertyValue(ref _created, value);
		}

		public Boolean IsDownloadCountAvailable => this.DownloadCount != -1 && !(_configService.GetEffectiveConfiguration().HidePackageDownloadCount ?? false);
		public Boolean IsInstallAllowed => _allowedCommandsService.IsInstallCommandAllowed;
		public Boolean IsPackageSizeAvailable => this.PackageSize != -1;
		public Boolean IsPinAllowed => _allowedCommandsService.IsPinCommandAllowed;
		public Boolean IsReinstallAllowed => _allowedCommandsService.IsInstallCommandAllowed;
		public Boolean IsUninstallAllowed => _allowedCommandsService.IsUninstallCommandAllowed;
		public Boolean IsUnpinAllowed => _allowedCommandsService.IsPinCommandAllowed;
		public Boolean IsUpgradeAllowed => _allowedCommandsService.IsUpgradeCommandAllowed;

		public DateTime LastUpdated
		{
			get => _lastUpdated;
			set => SetPropertyValue(ref _lastUpdated, value);
		}

		public String LowerCaseId => this.Id.ToLowerInvariant();

		#endregion

		#region Public interface

		public async Task Pin()
		{
			try
			{
				using (await StartProgressDialog(L(nameof(Resources.PackageViewModel_PinningPackage)), L(nameof(Resources.PackageViewModel_PinningPackage)), this.Id))
				{
					var result = await _chocolateyService.PinPackage(this.Id, this.Version.ToNormalizedStringChecked());

					if (!result.Successful)
					{
						var exceptionMessage = result.Exception == null
							? String.Empty
							: L(nameof(Resources.ChocolateyRemotePackageService_ExceptionFormat), result.Exception);

						var message = L(
							nameof(Resources.ChocolateyRemotePackageService_PinFailedMessage), this.Id, this.Version,
							String.Join("\n", result.Messages),
							exceptionMessage);

						await _dialogService.ShowMessageAsync(
							L(nameof(Resources.ChocolateyRemotePackageService_PinFailedTitle)),
							message);

						PackageViewModel.Logger.Warning(result.Exception, "Failed to pin {Package}, version {Version}. Errors: {Errors}", this.Id, this.Version, result.Messages);

						return;
					}

					this.IsPinned = true;
					_eventAggregator.BeginPublishOnUIThread(new PackageChangedMessage(this.Id, PackageChangeType.Pinned, this.Version));
				}
			}
			catch (Exception ex)
			{
				PackageViewModel.Logger.Error(ex, "Ran into an error while pinning {Id}, version {Version}.", this.Id, this.Version);

				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_FailedToPin)),
					L(nameof(Resources.PackageViewModel_RanIntoPinningError), this.Id, ex.Message));
			}
		}

		public async Task Reinstall()
		{
			try
			{
				var confirmationResult = MessageDialogResult.Affirmative;
				if (!_configService.GetEffectiveConfiguration().SkipModalDialogConfirmation.GetValueOrDefault(false))
				{
					confirmationResult = await _dialogService.ShowConfirmationMessageAsync(
						L(nameof(Resources.Dialog_AreYouSureTitle)),
						L(nameof(Resources.Dialog_AreYouSureReinstallMessage), this.Id));
				}

				if (confirmationResult == MessageDialogResult.Affirmative)
				{
					using (await StartProgressDialog(L(nameof(Resources.PackageViewModel_ReinstallingPackage)), L(nameof(Resources.PackageViewModel_ReinstallingPackage)), this.Id))
					{
						await _chocolateyService.InstallPackage(this.Id, this.Version.ToNormalizedStringChecked(), this.Source, true);
						_chocolateyGuiCacheService.PurgeOutdatedPackages();
						await _eventAggregator.PublishOnUIThreadAsync(new PackageChangedMessage(this.Id, PackageChangeType.Installed, this.Version));
					}
				}
			}
			catch (Exception ex)
			{
				PackageViewModel.Logger.Error(ex, "Ran into an error while reinstalling {Id}, version {Version}.", this.Id, this.Version);
				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_FailedToReinstall)),
					L(nameof(Resources.PackageViewModel_RanIntoInstallError), this.Id, ex.Message));
			}
		}

		public async Task ShowArguments()
		{
			// TODO: Add legacy handling for packages installed prior to v2.0.0.
			var decryptedArguments = _packageArgumentsService.DecryptPackageArgumentsFile(this.Id, this.Version.ToNormalizedStringChecked()).ToList();

			if (decryptedArguments.Count == 0)
			{
				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_ArgumentsForPackageFormat), this.Title),
					L(nameof(Resources.PackageViewModel_NoArgumentsAvailableForPackage)));
			}
			else
			{
				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_ArgumentsForPackageFormat), this.Title),
					String.Join(Environment.NewLine, decryptedArguments));
			}
		}

		public async Task Unpin()
		{
			try
			{
				using (await StartProgressDialog(L(nameof(Resources.PackageViewModel_UnpinningPackage)), L(nameof(Resources.PackageViewModel_UnpinningPackage)), this.Id))
				{
					var result = await _chocolateyService.UnpinPackage(this.Id, this.Version.ToNormalizedStringChecked());

					if (!result.Successful)
					{
						var exceptionMessage = result.Exception == null
							? String.Empty
							: L(nameof(Resources.ChocolateyRemotePackageService_ExceptionFormat), result.Exception);

						var message = L(
							nameof(Resources.ChocolateyRemotePackageService_UnpinFailedMessage), this.Id, this.Version,
							String.Join("\n", result.Messages),
							exceptionMessage);

						await _dialogService.ShowMessageAsync(
							L(nameof(Resources.ChocolateyRemotePackageService_UninstallFailedTitle)),
							message);

						PackageViewModel.Logger.Warning(result.Exception, "Failed to unpin {Package}, version {Version}. Errors: {Errors}", this.Id, this.Version, result.Messages);

						return;
					}

					this.IsPinned = false;
					_eventAggregator.BeginPublishOnUIThread(new PackageChangedMessage(this.Id, PackageChangeType.Unpinned, this.Version));
				}
			}
			catch (Exception ex)
			{
				PackageViewModel.Logger.Error(ex, "Ran into an error while unpinning {Id}, version {Version}.", this.Id, this.Version);

				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_FailedToUnpin)),
					L(nameof(Resources.PackageViewModel_RanIntoUnpinError), this.Id, ex.Message));
			}
		}

		#endregion

		#region IHandle<FeatureModifiedMessage> implementation

		public void Handle(FeatureModifiedMessage message)
		{
			NotifyPropertyChanged(nameof(PackageViewModel.IsDownloadCountAvailable));
		}

		#endregion

		#region IHandle<PackageHasUpdateMessage> implementation

		public void Handle(PackageHasUpdateMessage message)
		{
			if (!String.Equals(message.Id, this.Id, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			this.LatestVersion = message.Version;
			this.IsOutdated = true;
		}

		#endregion

		#region IPackageViewModel implementation

		public String[] Authors
		{
			get => _authors;
			set => SetPropertyValue(ref _authors, value);
		}

		public Boolean CanUpdate => this.IsInstalled && !this.IsPinned && this.IsOutdated;

		public String Copyright
		{
			get => _copyright;
			set => SetPropertyValue(ref _copyright, value);
		}

		public String Dependencies
		{
			get => _dependencies;
			set => SetPropertyValue(ref _dependencies, value);
		}

		public String Description
		{
			get => _description;
			set => SetPropertyValue(ref _description, value);
		}

		public Int64 DownloadCount
		{
			get => _downloadCount;
			set => SetPropertyValue(ref _downloadCount, value);
		}

		public String GalleryDetailsUrl
		{
			get => _galleryDetailsUrl;
			set => SetPropertyValue(ref _galleryDetailsUrl, value);
		}

		public String IconUrl
		{
			get => _iconUrl;
			set => SetPropertyValue(ref _iconUrl, value);
		}

		public String Id
		{
			get => _id;
			set => SetPropertyValue(ref _id, value);
		}

		public Int32 Index
		{
			get => _index;
			set => SetPropertyValue(ref _index, value);
		}

		public async Task Install()
		{
			await InstallPackage(this.Version.ToNormalizedStringChecked());
		}

		public async Task InstallAdvanced()
		{
			var dataContext = new AdvancedInstallViewModel(_chocolateyService, _persistenceService, this.Version);

			var result = await _dialogService.ShowChildWindowAsync<AdvancedInstallViewModel, AdvancedInstallViewModel>(
				L(nameof(Resources.AdvancedChocolateyDialog_Title_Install)),
				new AdvancedInstallView { DataContext = dataContext },
				dataContext);

			// null means that the Cancel button was clicked
			if (result != null)
			{
				if (String.Equals(result.SelectedVersion, Resources.AdvancedChocolateyDialog_LatestVersion, StringComparison.OrdinalIgnoreCase))
				{
					result.SelectedVersion = null;
				}

				var advancedOptions = _mapper.Map<AdvancedInstall>(result);

				await InstallPackage(result.SelectedVersion, advancedOptions);
			}
		}

		public Boolean IsInstalled
		{
			get => _isInstalled;
			set
			{
				if (SetPropertyValue(ref _isInstalled, value))
				{
					NotifyPropertyChanged(nameof(PackageViewModel.CanUpdate));
				}
			}
		}

		public Boolean IsOutdated
		{
			get => _isOutdated;
			set
			{
				if (SetPropertyValue(ref _isOutdated, value))
				{
					NotifyPropertyChanged(nameof(PackageViewModel.CanUpdate));
				}
			}
		}

		public Boolean IsPinned
		{
			get => _isPinned;
			set
			{
				if (SetPropertyValue(ref _isPinned, value))
				{
					NotifyPropertyChanged(nameof(PackageViewModel.CanUpdate));
				}
			}
		}

		public Boolean IsPrerelease
		{
			get => _isPrerelease;
			set => SetPropertyValue(ref _isPrerelease, value);
		}

		public String Language
		{
			get => _language;
			set => SetPropertyValue(ref _language, value);
		}

		public NuGetVersion LatestVersion
		{
			get => _latestVersion;
			set => SetPropertyValue(ref _latestVersion, value);
		}

		public String LicenseUrl
		{
			get => _licenseUrl;
			set => SetPropertyValue(ref _licenseUrl, value);
		}

		public String[] Owners
		{
			get => _owners;
			set => SetPropertyValue(ref _owners, value);
		}

		public String PackageHash
		{
			get => _packageHash;
			set => SetPropertyValue(ref _packageHash, value);
		}

		public String PackageHashAlgorithm
		{
			get => _packageHashAlgorithm;
			set => SetPropertyValue(ref _packageHashAlgorithm, value);
		}

		public Int64 PackageSize
		{
			get => _packageSize;
			set => SetPropertyValue(ref _packageSize, value);
		}

		public String ProjectUrl
		{
			get => _projectUrl;
			set => SetPropertyValue(ref _projectUrl, value);
		}

		public DateTimeOffset Published
		{
			get => _published;
			set => SetPropertyValue(ref _published, value);
		}

		public String ReleaseNotes
		{
			get => _releaseNotes;
			set => SetPropertyValue(ref _releaseNotes, value);
		}

		public String ReportAbuseUrl
		{
			get => _reportAbuseUrl;
			set => SetPropertyValue(ref _reportAbuseUrl, value);
		}

		public String RequireLicenseAcceptance
		{
			get => _requireLicenseAcceptance;
			set => SetPropertyValue(ref _requireLicenseAcceptance, value);
		}

		public Uri Source
		{
			get => _source;
			set => SetPropertyValue(ref _source, value);
		}

		public String Summary
		{
			get => _summary;
			set => SetPropertyValue(ref _summary, value);
		}

		public String Tags
		{
			get => _tags;
			set => SetPropertyValue(ref _tags, value);
		}

		public String Title
		{
			get => String.IsNullOrWhiteSpace(_title) ? this.Id : _title;
			set => SetPropertyValue(ref _title, value);
		}

		public async Task Uninstall()
		{
			try
			{
				var confirmationResult = MessageDialogResult.Affirmative;
				if (!_configService.GetEffectiveConfiguration().SkipModalDialogConfirmation.GetValueOrDefault(false))
				{
					confirmationResult = await _dialogService.ShowConfirmationMessageAsync(
						L(nameof(Resources.Dialog_AreYouSureTitle)),
						L(nameof(Resources.Dialog_AreYouSureUninstallMessage), this.Id));
				}

				if (confirmationResult == MessageDialogResult.Affirmative)
				{
					using (await StartProgressDialog(L(nameof(Resources.PackageViewModel_UninstallingPackage)), L(nameof(Resources.PackageViewModel_UninstallingPackage)), this.Id))
					{
						var result = await _chocolateyService.UninstallPackage(this.Id, this.Version.ToNormalizedStringChecked(), true);

						if (!result.Successful)
						{
							var exceptionMessage = result.Exception == null
								? String.Empty
								: L(nameof(Resources.ChocolateyRemotePackageService_ExceptionFormat), result.Exception);

							var message = L(
								nameof(Resources.ChocolateyRemotePackageService_UninstallFailedMessage), this.Id, this.Version,
								String.Join("\n", result.Messages),
								exceptionMessage);

							await _dialogService.ShowMessageAsync(
								L(nameof(Resources.ChocolateyRemotePackageService_UninstallFailedTitle)),
								message);

							PackageViewModel.Logger.Warning(result.Exception, "Failed to uninstall {Package}, version {Version}. Errors: {Errors}", this.Id, this.Version, result.Messages);

							return;
						}

						this.IsInstalled = false;
						_eventAggregator.BeginPublishOnUIThread(new PackageChangedMessage(this.Id, PackageChangeType.Uninstalled, this.Version));
					}
				}
			}
			catch (Exception ex)
			{
				PackageViewModel.Logger.Error(ex, "Ran into an error while uninstalling {Id}, version {Version}.", this.Id, this.Version);

				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_FailedToUninstall)),
					L(nameof(Resources.PackageViewModel_RanIntoUninstallError), this.Id, ex.Message));
			}
		}

		public async Task Update()
		{
			try
			{
				using (await StartProgressDialog(L(nameof(Resources.PackageViewModel_UpdatingPackage)), L(nameof(Resources.PackageViewModel_UpdatingPackage)), this.Id))
				{
					var result = await _chocolateyService.UpdatePackage(this.Id, this.Source);

					if (!result.Successful)
					{
						var exceptionMessage = result.Exception == null
							? String.Empty
							: L(nameof(Resources.ChocolateyRemotePackageService_ExceptionFormat), result.Exception);

						var message = L(
							nameof(Resources.ChocolateyRemotePackageService_UpdateFailedMessage), this.Id,
							String.Join("\n", result.Messages),
							exceptionMessage);

						await _dialogService.ShowMessageAsync(
							L(nameof(Resources.ChocolateyRemotePackageService_UpdateFailedTitle)),
							message);

						PackageViewModel.Logger.Warning(result.Exception, "Failed to update {Package}. Errors: {Errors}", this.Id, result.Messages);

						return;
					}

					_chocolateyGuiCacheService.PurgeOutdatedPackages();
					_eventAggregator.BeginPublishOnUIThread(new PackageChangedMessage(this.Id, PackageChangeType.Updated));
				}
			}
			catch (Exception ex)
			{
				PackageViewModel.Logger.Error(ex, "Ran into an error while updating {Id}, version {Version}.", this.Id, this.Version);

				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_FailedToUpdate)),
					L(nameof(Resources.PackageViewModel_RanIntoUpdateError), this.Id, ex.Message));
			}
		}

		public NuGetVersion Version
		{
			get => _version;
			set => SetPropertyValue(ref _version, value);
		}

		public Int64 VersionDownloadCount
		{
			get => _versionDownloadCount;
			set => SetPropertyValue(ref _versionDownloadCount, value);
		}

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

		public async void ViewDetails()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
		{
			await _eventAggregator.PublishOnUIThreadAsync(new ShowPackageDetailsMessage(this)).ConfigureAwait(false);
		}

		#endregion

		#region Private implementation

		private async Task InstallPackage(String version, AdvancedInstall advancedOptions = null)
		{
			try
			{
				using (await StartProgressDialog(L(nameof(Resources.PackageViewModel_InstallingPackage)), L(nameof(Resources.PackageViewModel_InstallingPackage)), this.Id))
				{
					var packageInstallResult = await _chocolateyService.InstallPackage(this.Id,
					                                                                   version, this.Source,
					                                                                   false,
					                                                                   advancedOptions);

					if (!packageInstallResult.Successful)
					{
						var exceptionMessage = packageInstallResult.Exception == null
							? String.Empty
							: L(nameof(Resources.ChocolateyRemotePackageService_ExceptionFormat), packageInstallResult.Exception);

						var message = L(
							nameof(Resources.ChocolateyRemotePackageService_InstallFailedMessage), this.Id, this.Version,
							String.Join("\n", packageInstallResult.Messages),
							exceptionMessage);

						await _dialogService.ShowMessageAsync(
							L(nameof(Resources.ChocolateyRemotePackageService_InstallFailedTitle)),
							message);

						PackageViewModel.Logger.Warning(packageInstallResult.Exception, "Failed to install {Package}, version {Version}. Errors: {Errors}", this.Id, this.Version, packageInstallResult.Messages);

						return;
					}

					this.IsInstalled = true;

					_chocolateyGuiCacheService.PurgeOutdatedPackages();
					_eventAggregator.BeginPublishOnUIThread(new PackageChangedMessage(this.Id, PackageChangeType.Installed, this.Version));
				}
			}
			catch (Exception ex)
			{
				PackageViewModel.Logger.Error(ex, "Ran into an error while installing {Id}, version {Version}.", this.Id, this.Version);

				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackageViewModel_FailedToInstall)),
					L(nameof(Resources.PackageViewModel_RanIntoInstallError), this.Id, ex.Message));
			}
		}

		private async Task<IDisposable> StartProgressDialog(String commandString, String initialProgressText, String id = "")
		{
			await _progressService.StartLoading(L(nameof(Resources.PackageViewModel_StartLoadingFormat), commandString, id));
			_progressService.WriteMessage(initialProgressText);
			return new DisposableAction(() => _progressService.StopLoading());
		}

		#endregion

		#region DisposableAction Class

		private class DisposableAction : IDisposable
		{
			#region Declarations

			private readonly Action _disposeAction;

			#endregion

			#region Constructors

			public DisposableAction(Action disposeAction)
			{
				_disposeAction = disposeAction;
			}

			#endregion

			#region IDisposable implementation

			public void Dispose()
			{
				_disposeAction?.Invoke();
			}

			#endregion
		}

		#endregion
	}
}