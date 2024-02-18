// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Reactive.Linq;
	using System.Threading.Tasks;
	using System.Windows.Data;
	using System.Windows.Input;
	using AutoMapper;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Base;
	using ChocolateyGui.Common.Enums;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.ViewModels;
	using ChocolateyGui.Common.ViewModels.Items;
	using ChocolateyGui.Common.Windows.Services;
	using ChocolateyGui.Common.Windows.Utilities.Extensions;
	using MahApps.Metro.Controls.Dialogs;
	using Serilog;

	public sealed class LocalSourceViewModel : ViewModelScreen, ISourceViewModelBase, IHandleWithTask<PackageChangedMessage>
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<LocalSourceViewModel>();
		private readonly IAllowedCommandsService _allowedCommandsService;
		private readonly IChocolateyGuiCacheService _chocolateyGuiCacheService;
		private readonly IChocolateyService _chocolateyService;
		private readonly IConfigService _configService;
		private readonly IDialogService _dialogService;
		private readonly IEventAggregator _eventAggregator;
		private readonly IMapper _mapper;
		private readonly List<IPackageViewModel> _packages;
		private readonly IPersistenceService _persistenceService;
		private readonly IProgressService _progressService;
		private Boolean _exportAll = true;
		private Boolean _firstLoadIncomplete = true;
		private Boolean _hasLoaded;
		private Boolean _isLoading;
		private Boolean _isShowOnlyPackagesWithUpdateEnabled;
		private ListViewMode _listViewMode;
		private Boolean _matchWord;
		private ObservableCollection<IPackageViewModel> _packageViewModels;
		private readonly String _resourceId;
		private String _searchQuery;
		private Boolean _showAdditionalPackageInformation;
		private Boolean _showOnlyPackagesWithUpdate;
		private String _sortColumn;
		private Boolean _sortDescending;

		#endregion

		#region Constructors

		public LocalSourceViewModel(
			IChocolateyService chocolateyService,
			IDialogService dialogService,
			IProgressService progressService,
			IPersistenceService persistenceService,
			IChocolateyGuiCacheService chocolateyGuiCacheService,
			IConfigService configService,
			IAllowedCommandsService allowedCommandsService,
			IEventAggregator eventAggregator,
			String displayName,
			IMapper mapper,
			TranslationSource translator)
			: base(translator)
		{
			_chocolateyService = chocolateyService;
			_dialogService = dialogService;
			_progressService = progressService;
			_persistenceService = persistenceService;
			_chocolateyGuiCacheService = chocolateyGuiCacheService;
			_configService = configService;
			_allowedCommandsService = allowedCommandsService;

			if (displayName[0] == '[' && displayName[displayName.Length - 1] == ']')
			{
				_resourceId = displayName.Trim('[', ']');
				this.DisplayName = translator[_resourceId];
				translator.PropertyChanged += (sender, e) => { this.DisplayName = translator[_resourceId]; };
			}
			else
			{
				this.DisplayName = displayName;
			}

			_packages = new List<IPackageViewModel>();
			this.Packages = new ObservableCollection<IPackageViewModel>();
			this.PackageSource = CollectionViewSource.GetDefaultView(this.Packages);
			this.PackageSource.Filter = FilterPackage;

			if (eventAggregator == null)
			{
				throw new ArgumentNullException(nameof(eventAggregator));
			}

			_eventAggregator = eventAggregator;
			_mapper = mapper;
			_eventAggregator.Subscribe(this);
		}

		#endregion

		#region Properties

		public Boolean FirstLoadIncomplete
		{
			get => _firstLoadIncomplete;
			set => this.SetPropertyValue(ref _firstLoadIncomplete, value);
		}

		public Boolean HasLoaded
		{
			get => _hasLoaded;
			set => this.SetPropertyValue(ref _hasLoaded, value);
		}

		public Boolean IsLoading
		{
			get => _isLoading;
			set => this.SetPropertyValue(ref _isLoading, value);
		}

		public Boolean IsShowOnlyPackagesWithUpdateEnabled
		{
			get => _isShowOnlyPackagesWithUpdateEnabled;
			set => this.SetPropertyValue(ref _isShowOnlyPackagesWithUpdateEnabled, value);
		}

		public Boolean IsUpgradeAllAllowed => _allowedCommandsService.IsUpgradeAllCommandAllowed;
		public Boolean IsUpgradeAllowed => _allowedCommandsService.IsUpgradeCommandAllowed;

		public ListViewMode ListViewMode
		{
			get => _listViewMode;
			set => this.SetPropertyValue(ref _listViewMode, value);
		}

		public Boolean MatchWord
		{
			get => _matchWord;
			set => this.SetPropertyValue(ref _matchWord, value);
		}

		public ObservableCollection<IPackageViewModel> Packages
		{
			get => _packageViewModels;
			set => this.SetPropertyValue(ref _packageViewModels, value);
		}

		public ICollectionView PackageSource { get; }

		public String SearchQuery
		{
			get => _searchQuery;
			set => this.SetPropertyValue(ref _searchQuery, value);
		}

		public Boolean ShowAdditionalPackageInformation
		{
			get => _showAdditionalPackageInformation;
			set => this.SetPropertyValue(ref _showAdditionalPackageInformation, value);
		}

		public Boolean ShowOnlyPackagesWithUpdate
		{
			get => _showOnlyPackagesWithUpdate;
			set => this.SetPropertyValue(ref _showOnlyPackagesWithUpdate, value);
		}

		public String SortColumn
		{
			get => _sortColumn;
			set => this.SetPropertyValue(ref _sortColumn, value);
		}

		public Boolean SortDescending
		{
			get => _sortDescending;
			set => this.SetPropertyValue(ref _sortDescending, value);
		}

		#endregion

		#region Public interface

		public Boolean CanCheckForOutdatedPackages()
		{
			return this.HasLoaded && !this.IsLoading;
		}

		public Boolean CanExportAll()
		{
			return _exportAll;
		}

		public Boolean CanRefreshPackages()
		{
			return this.HasLoaded && !this.IsLoading;
		}

		public Boolean CanUpdateAll()
		{
			return this.Packages.Any(p => p.CanUpdate) && _allowedCommandsService.IsUpgradeCommandAllowed && _allowedCommandsService.IsUpgradeAllCommandAllowed;
		}

		public async Task CheckForOutdatedPackages()
		{
			_chocolateyGuiCacheService.PurgeOutdatedPackages();
			await CheckOutdated(true);
		}

		public async void ExportAll()
		{
			_exportAll = false;

			try
			{
				var exportFilePath = _persistenceService.GetFilePath("*.config", L(nameof(Resources.LocalSourceViewModel_ConfigFiles), "(.config)|*.config"));

				if (String.IsNullOrEmpty(exportFilePath))
				{
					return;
				}

				await _chocolateyService.ExportPackages(exportFilePath, true);

				await _dialogService.ShowMessageAsync(
					                    L(nameof(Resources.LocalSourceView_ButtonExport)),
					                    L(nameof(Resources.LocalSourceViewModel_ExportComplete), exportFilePath))
				                    .ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				LocalSourceViewModel.Logger.Fatal("Export all has failed.", ex);
				throw;
			}
			finally
			{
				_exportAll = true;
			}
		}

		public async void RefreshPackages()
		{
			await LoadPackages();
		}

		public async void UpdateAll()
		{
			try
			{
				var result = MessageDialogResult.Affirmative;
				if (!_configService.GetEffectiveConfiguration().SkipModalDialogConfirmation.GetValueOrDefault(false))
				{
					result = await _dialogService.ShowConfirmationMessageAsync(
						L(nameof(Resources.Dialog_AreYouSureTitle)),
						L(nameof(Resources.Dialog_AreYouSureUpdateAllMessage)));
				}

				if (result == MessageDialogResult.Affirmative)
				{
					await _progressService.StartLoading(L(nameof(Resources.LocalSourceViewModel_Packages)), true);
					this.IsLoading = true;

					_progressService.WriteMessage(L(nameof(Resources.LocalSourceViewModel_FetchingPackages)));
					var token = _progressService.GetCancellationToken();
					var packages = this.Packages.Where(p => p.CanUpdate && !p.IsPinned).ToList();
					Double current = 0.0f;
					foreach (var package in packages)
					{
						if (token.IsCancellationRequested)
						{
							await _progressService.StopLoading();
							this.IsLoading = false;
							return;
						}

						_progressService.Report(Math.Min(current++ / packages.Count, 100));
						await package.Update();
					}

					await _progressService.StopLoading();
					this.IsLoading = false;
					this.ShowOnlyPackagesWithUpdate = false;
					RefreshPackages();
				}
			}
			catch (Exception ex)
			{
				LocalSourceViewModel.Logger.Fatal("Updated all has failed.", ex);
				throw;
			}
		}

		#endregion

		#region Screen overrides

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
		protected override async void OnInitialize()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
		{
			try
			{
				if (this.HasLoaded)
				{
					return;
				}

				this.ListViewMode = _configService.GetEffectiveConfiguration().DefaultToTileViewForLocalSource ?? true ? ListViewMode.Tile : ListViewMode.Standard;
				this.ShowAdditionalPackageInformation = _configService.GetEffectiveConfiguration().ShowAdditionalPackageInformation ?? false;

				Observable.FromEventPattern<EventArgs>(_configService, "SettingsChanged")
				          .ObserveOnDispatcher()
				          .Subscribe(eventPattern =>
				          {
					          var appConfig = (AppConfiguration)eventPattern.Sender;

					          this.ListViewMode = appConfig.DefaultToTileViewForLocalSource ?? false
						          ? ListViewMode.Tile
						          : ListViewMode.Standard;
					          this.ShowAdditionalPackageInformation = appConfig.ShowAdditionalPackageInformation ?? false;
				          });

				await LoadPackages();

				Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
				          .Where(
					          eventPattern =>
						          eventPattern.EventArgs.PropertyName == nameof(LocalSourceViewModel.MatchWord) ||
						          eventPattern.EventArgs.PropertyName == nameof(LocalSourceViewModel.SearchQuery) ||
						          eventPattern.EventArgs.PropertyName == nameof(LocalSourceViewModel.ShowOnlyPackagesWithUpdate))
				          .ObserveOnDispatcher()
				          .Subscribe(eventPattern => this.PackageSource.Refresh());

				Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
				          .Where(eventPattern => eventPattern.EventArgs.PropertyName == nameof(LocalSourceViewModel.ListViewMode))
				          .ObserveOnDispatcher()
				          .Subscribe(eventPattern =>
				          {
					          if (this.ListViewMode == ListViewMode.Tile)
					          {
						          // reset custom sorting for now
						          var listColView = this.PackageSource as ListCollectionView;
						          if (listColView != null)
						          {
							          listColView.CustomSort = null;
						          }
					          }
				          });

				this.HasLoaded = true;

				var chocoPackage = _packages.FirstOrDefault(p => p.Id.ToLower() == "chocolatey");
				if (chocoPackage != null && chocoPackage.CanUpdate)
				{
					await _dialogService.ShowMessageAsync(
						                    L(nameof(Resources.LocalSourceViewModel_Chocolatey)),
						                    L(nameof(Resources.LocalSourceViewModel_UpdateAvailableForChocolatey)))
					                    .ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				LocalSourceViewModel.Logger.Fatal("Local source control view model failed to load.", ex);
				throw;
			}
		}

		#endregion

		#region IHandleWithTask<PackageChangedMessage> implementation

		public async Task Handle(PackageChangedMessage message)
		{
			switch (message.ChangeType)
			{
				case PackageChangeType.Pinned:
					this.PackageSource.Refresh();
					break;
				case PackageChangeType.Unpinned:
					var package = this.Packages.First(p => p.Id == message.Id);
					if (package.LatestVersion != null)
					{
						this.PackageSource.Refresh();
					}
					else
					{
						var outOfDatePackages =
							await _chocolateyService.GetOutdatedPackages(package.IsPrerelease, package.Id);
						foreach (var update in outOfDatePackages)
						{
							await _eventAggregator.PublishOnUIThreadAsync(new PackageHasUpdateMessage(update.Id, update.Version));
						}

						this.PackageSource.Refresh();
					}

					break;

				case PackageChangeType.Uninstalled:
					this.Packages.Remove(this.Packages.First(p => p.Id == message.Id));
					break;

				default:
					await LoadPackages();
					break;
			}
		}

		#endregion

		#region Private implementation

		private async Task CheckOutdated(Boolean forceCheckForOutdated)
		{
			this.IsLoading = true;

			try
			{
				var updates = await _chocolateyService.GetOutdatedPackages(false, null, forceCheckForOutdated);

				// Use a list of task for correct async loop
				var listOfTasks = updates.Select(update => _eventAggregator.PublishOnUIThreadAsync(new PackageHasUpdateMessage(update.Id, update.Version))).ToList();
				await Task.WhenAll(listOfTasks);

				this.PackageSource.Refresh();
			}
			catch (ConnectionClosedException)
			{
				LocalSourceViewModel.Logger.Warning("Threw connection closed message while processing load packages.");
			}
			catch (Exception ex)
			{
				LocalSourceViewModel.Logger.Fatal("Packages failed to load", ex);
				throw;
			}
			finally
			{
				this.IsLoading = false;

				// Only enable the "Show only outdated packages" when it makes sense.
				// It does not make sense to enable the checkbox when we haven't checked for
				// outdated packages. We should only enable the checkbox here when: (or)
				// 1. the "Prevent Automated Outdated Packages Check" is disabled
				// 2. forced a check for outdated packages.
				this.IsShowOnlyPackagesWithUpdateEnabled = forceCheckForOutdated || !(_configService.GetEffectiveConfiguration().PreventAutomatedOutdatedPackagesCheck ?? false);

				// Force invalidating the command stuff.
				// This helps us to prevent disabled buttons after executing this routine.
				// But IMO it has something to do with Caliburn.
				CommandManager.InvalidateRequerySuggested();
			}
		}

		private Boolean FilterPackage(Object packageObject)
		{
			var package = (IPackageViewModel)packageObject;
			var include = true;
			if (!String.IsNullOrWhiteSpace(this.SearchQuery))
			{
				if (this.MatchWord)
				{
					include &= String.Compare(
						package.Title ?? package.Id, this.SearchQuery,
						StringComparison.OrdinalIgnoreCase) == 0;
				}
				else
				{
					include &= CultureInfo.CurrentCulture.CompareInfo.IndexOf(
						package.Title ?? package.Id, this.SearchQuery,
						CompareOptions.OrdinalIgnoreCase) >= 0;
				}
			}

			if (this.ShowOnlyPackagesWithUpdate)
			{
				include &= package.CanUpdate && !package.IsPinned;
			}

			return include;
		}

		private async Task LoadPackages()
		{
			if (this.IsLoading)
			{
				return;
			}

			this.IsLoading = true;
			this.IsShowOnlyPackagesWithUpdateEnabled = false;

			_packages.Clear();
			this.Packages.Clear();

			var packages = (await _chocolateyService.GetInstalledPackages())
			               .Select(Mapper.Map<IPackageViewModel>).ToList();

			var count = 1;
			foreach (var packageViewModel in packages)
			{
				packageViewModel.Index = count++;
				_packages.Add(packageViewModel);
				this.Packages.Add(packageViewModel);
			}

			this.FirstLoadIncomplete = false;

			await CheckOutdated(false);
		}

		#endregion
	}
}