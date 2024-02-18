// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using System.Reactive.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using AutoMapper;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Enums;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.ViewModels;
	using ChocolateyGui.Common.ViewModels.Items;
	using ChocolateyGui.Common.Windows.Services;
	using ChocolateyGui.Common.Windows.Utilities;
	using ChocolateyGui.Common.Windows.Utilities.Extensions;
	using NuGet.Packaging;
	using Serilog;

	public sealed class RemoteSourceViewModel : ViewModelScreen, ISourceViewModelBase
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<RemoteSourceViewModel>();
		private readonly IChocolateyGuiCacheService _chocolateyGuiCacheService;
		private readonly IChocolateyService _chocolateyPackageService;
		private readonly IConfigService _configService;
		private readonly IDialogService _dialogService;
		private readonly IEventAggregator _eventAggregator;
		private readonly IMapper _mapper;
		private readonly IProgressService _progressService;
		private Int32 _currentPage = 1;
		private Boolean _hasLoaded;
		private Boolean _includeAllVersions;
		private Boolean _includePrerelease;
		private ListViewMode _listViewMode;
		private Boolean _matchWord;
		private ObservableCollection<IPackageViewModel> _packageViewModels;
		private Int32 _pageCount = 1;
		private Int32 _pageSize = 50;
		private readonly String _resourceId;
		private String _searchQuery;
		private IDisposable _searchQuerySubscription;
		private Boolean _shouldShowPreventPreloadMessage;
		private Boolean _showAdditionalPackageInformation;
		private String _sortSelection;
		private String _sortSelectionName;

		#endregion

		#region Constructors

		public RemoteSourceViewModel(
			IChocolateyService chocolateyPackageService,
			IDialogService dialogService,
			IProgressService progressService,
			IChocolateyGuiCacheService chocolateyGuiCacheService,
			IConfigService configService,
			IEventAggregator eventAggregator,
			ChocolateySource source,
			IMapper mapper,
			TranslationSource translator)
			: base(translator)
		{
			this.Source = source;
			_chocolateyPackageService = chocolateyPackageService;
			_dialogService = dialogService;
			_progressService = progressService;
			_chocolateyGuiCacheService = chocolateyGuiCacheService;
			_configService = configService;
			_eventAggregator = eventAggregator;
			_mapper = mapper;

			this.Packages = new ObservableCollection<IPackageViewModel>();

			if (source.Id[0] == '[' && source.Id[source.Id.Length - 1] == ']')
			{
				_resourceId = source.Id.Trim('[', ']');
				this.DisplayName = translator[_resourceId];
				translator.PropertyChanged += (sender, e) => { this.DisplayName = translator[_resourceId]; };
			}
			else
			{
				this.DisplayName = source.Id;
			}

			if (eventAggregator == null)
			{
				throw new ArgumentNullException(nameof(eventAggregator));
			}

			_eventAggregator.Subscribe(this);

			AddSortOptions();

			this.SortSelection = L(nameof(Resources.RemoteSourceViewModel_SortSelectionPopularity));
		}

		#endregion

		#region Properties

		public Int32 CurrentPage
		{
			get => _currentPage;
			set => this.SetPropertyValue(ref _currentPage, value);
		}

		public Boolean HasLoaded
		{
			get => _hasLoaded;
			set => this.SetPropertyValue(ref _hasLoaded, value);
		}

		public Boolean IncludeAllVersions
		{
			get => _includeAllVersions;
			set => this.SetPropertyValue(ref _includeAllVersions, value);
		}

		public Boolean IncludePrerelease
		{
			get => _includePrerelease;
			set => this.SetPropertyValue(ref _includePrerelease, value);
		}

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

		public Int32 PageCount
		{
			get => _pageCount;
			set => this.SetPropertyValue(ref _pageCount, value);
		}

		public Int32 PageSize
		{
			get => _pageSize;
			set => this.SetPropertyValue(ref _pageSize, value);
		}

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

		public Boolean ShowShouldPreventPreloadMessage
		{
			get => _shouldShowPreventPreloadMessage;
			set => this.SetPropertyValue(ref _shouldShowPreventPreloadMessage, value);
		}

		public ObservableCollection<String> SortOptions { get; } = new ObservableCollection<String>();

		public String SortSelection
		{
			get => _sortSelection;
			set
			{
				_sortSelectionName = value == L(nameof(Resources.RemoteSourceViewModel_SortSelectionPopularity))
					? "DownloadCount"
					: "Title";
				this.SetPropertyValue(ref _sortSelection, value);
			}
		}

		public ChocolateySource Source { get; }

		#endregion

		#region Public interface

		public Boolean CanCheckForOutdatedPackages()
		{
			return this.HasLoaded;
		}

		public Boolean CanGoToFirst()
		{
			return this.CurrentPage > 1;
		}

		public Boolean CanGoToLast()
		{
			return this.CurrentPage < this.PageCount;
		}

		public Boolean CanGoToNext()
		{
			return this.CurrentPage < this.PageCount;
		}

		public Boolean CanGoToPrevious()
		{
			return this.CurrentPage > 1;
		}

		public Boolean CanLoadRemotePackages()
		{
			return this.HasLoaded;
		}

		public Boolean CanSearchForPackages()
		{
			return this.HasLoaded;
		}

		public async void CheckForOutdatedPackages()
		{
			_chocolateyGuiCacheService.PurgeOutdatedPackages();
			await LoadPackages(true);
		}

		public void GoToFirst()
		{
			this.CurrentPage = 1;
		}

		public void GoToLast()
		{
			this.CurrentPage = this.PageCount;
		}

		public void GoToNext()
		{
			if (this.CurrentPage < this.PageCount)
			{
				this.CurrentPage++;
			}
		}

		public void GoToPrevious()
		{
			if (this.CurrentPage > 1)
			{
				this.CurrentPage--;
			}
		}

		public async Task LoadPackages(Boolean forceCheckForOutdatedPackages)
		{
			try
			{
				if (!this.IsActive || (!CanLoadRemotePackages() && this.Packages.Any()))
				{
					return;
				}

				if (!this.HasLoaded && (_configService.GetEffectiveConfiguration().PreventPreload ?? false))
				{
					this.ShowShouldPreventPreloadMessage = true;
					this.HasLoaded = true;
					return;
				}

				this.HasLoaded = false;
				this.ShowShouldPreventPreloadMessage = false;

				var sort = _sortSelectionName;

				await _progressService.StartLoading(L(nameof(Resources.RemoteSourceViewModel_LoadingPage), this.CurrentPage));

				_progressService.WriteMessage(L(nameof(Resources.RemoteSourceViewModel_FetchingPackages)));

				try
				{
					var result =
						await
							_chocolateyPackageService.Search(this.SearchQuery,
							                                 new PackageSearchOptions(this.PageSize, this.CurrentPage - 1,
							                                                          sort, this.IncludePrerelease, this.IncludeAllVersions, this.MatchWord, this.Source.Value));
					var installed = await _chocolateyPackageService.GetInstalledPackages();
					var outdated = await _chocolateyPackageService.GetOutdatedPackages(false, null, forceCheckForOutdatedPackages);

					this.PageCount = (Int32)Math.Ceiling(result.TotalCount / (Double)this.PageSize);
					this.Packages.Clear();
					var count = 1;
					result.Packages.ToList().ForEach(p =>
					{
						if (installed.Any(package => String.Equals(package.Id, p.Id, StringComparison.OrdinalIgnoreCase)))
						{
							p.IsInstalled = true;
						}
						p.Index = count++;
						this.Packages.Add(Mapper.Map<IPackageViewModel>(p));
					});

					if (_configService.GetEffectiveConfiguration().ExcludeInstalledPackages ?? false)
					{
						this.Packages.RemoveAll(x => x.IsInstalled);
					}

					if (this.PageCount < this.CurrentPage)
					{
						this.CurrentPage = this.PageCount == 0 ? 1 : this.PageCount;
					}
				}
				finally
				{
					await _progressService.StopLoading();
					this.HasLoaded = true;
				}

				await _eventAggregator.PublishOnUIThreadAsync(new ResetScrollPositionMessage());
			}
			catch (Exception ex)
			{
				RemoteSourceViewModel.Logger.Error(ex, "Failed to load new packages.");
				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.RemoteSourceViewModel_FailedToLoad)),
					L(nameof(Resources.RemoteSourceViewModel_FailedToLoadRemotePackages), ex.Message));
				throw;
			}
		}

		public void RefreshRemotePackages()
		{
#pragma warning disable 4014
			LoadPackages(false);
#pragma warning restore 4014
		}

		public void SearchForPackages()
		{
#pragma warning disable 4014
			LoadPackages(false);
#pragma warning restore 4014
		}

		#endregion

		#region Screen overrides

		protected override async void OnActivate()
		{
			if (!this.HasLoaded)
			{
				await LoadPackages(false);
			}
		}

		protected override void OnInitialize()
		{
			try
			{
				this.ListViewMode = _configService.GetEffectiveConfiguration().DefaultToTileViewForRemoteSource ?? true ? ListViewMode.Tile : ListViewMode.Standard;
				this.ShowAdditionalPackageInformation = _configService.GetEffectiveConfiguration().ShowAdditionalPackageInformation ?? false;

				Observable.FromEventPattern<EventArgs>(_configService, "SettingsChanged")
				          .ObserveOnDispatcher()
				          .Subscribe(eventPattern =>
				          {
					          var appConfig = (AppConfiguration)eventPattern.Sender;

					          _searchQuerySubscription?.Dispose();
					          if (appConfig.UseDelayedSearch ?? false)
					          {
						          SubscribeToLoadPackagesOnSearchQueryChange();
					          }

					          this.ListViewMode = appConfig.DefaultToTileViewForRemoteSource ?? false ? ListViewMode.Tile : ListViewMode.Standard;
					          this.ShowAdditionalPackageInformation = appConfig.ShowAdditionalPackageInformation ?? false;
				          });

				var immediateProperties = new[]
				                          {
					                          "IncludeAllVersions", "IncludePrerelease", "MatchWord", "SortSelection"
				                          };

				if (_configService.GetEffectiveConfiguration().UseDelayedSearch ?? false)
				{
					SubscribeToLoadPackagesOnSearchQueryChange();
				}

				Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
				          .Where(e => immediateProperties.Contains(e.EventArgs.PropertyName))
				          .ObserveOnDispatcher()
#pragma warning disable 4014
				          .Subscribe(e => LoadPackages(false));
#pragma warning restore 4014

				Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
				          .Where(e => e.EventArgs.PropertyName == "CurrentPage")
				          .Throttle(TimeSpan.FromMilliseconds(300))
				          .DistinctUntilChanged()
				          .ObserveOnDispatcher()
#pragma warning disable 4014
				          .Subscribe(e => LoadPackages(false));
#pragma warning restore 4014
			}
			catch (InvalidOperationException ex)
			{
				RemoteSourceViewModel.Logger.Error(ex, "Failed to initialize remote source view model.");
				var message = L(nameof(Resources.RemoteSourceViewModel_UnableToConnectToFeed));
				var caption = L(nameof(Resources.RemoteSourceViewModel_FeedSearchError));
				ChocolateyMessageBox.Show(
					String.Format(
						CultureInfo.InvariantCulture,
						message, this.Source.Value),
					caption,
					MessageBoxButton.OK,
					MessageBoxImage.Error,
					MessageBoxResult.OK,
					MessageBoxOptions.ServiceNotification);
			}
		}

		#endregion

		#region ViewAware overrides

		protected override void OnViewAttached(Object view, Object context)
		{
			_eventAggregator.Subscribe(view);
		}

		#endregion

		#region ViewModelScreen overrides

		protected override void OnLanguageChanged()
		{
			AddSortOptions();

			this.SortSelection = _sortSelectionName == "DownloadCount"
				? L(nameof(Resources.RemoteSourceViewModel_SortSelectionPopularity))
				: L(nameof(Resources.RemoteSourceViewModel_SortSelectionAtoZ));

			RemoveOldSortOptions();
		}

		#endregion

		#region Private implementation

		private void AddSortOptions()
		{
			var downloadCount = L(nameof(Resources.RemoteSourceViewModel_SortSelectionPopularity));
			var title = L(nameof(Resources.RemoteSourceViewModel_SortSelectionAtoZ));

			var index = this.SortOptions.IndexOf(downloadCount);

			if (index == -1)
			{
				this.SortOptions.Insert(0, downloadCount);
			}

			index = this.SortOptions.IndexOf(title);

			if (index == -1)
			{
				this.SortOptions.Insert(1, title);
			}
		}

		private void RemoveOldSortOptions()
		{
			var downloadCount = L(nameof(Resources.RemoteSourceViewModel_SortSelectionPopularity));
			var title = L(nameof(Resources.RemoteSourceViewModel_SortSelectionAtoZ));

			this.SortOptions.RemoveAll(so => so != downloadCount && so != title);
		}

		private void SubscribeToLoadPackagesOnSearchQueryChange()
		{
			_searchQuerySubscription = Observable.FromEventPattern<PropertyChangedEventArgs>(this, "PropertyChanged")
			                                     .Where(e => e.EventArgs.PropertyName == "SearchQuery")
			                                     .Throttle(TimeSpan.FromMilliseconds(500))
			                                     .DistinctUntilChanged()
			                                     .ObserveOnDispatcher()
#pragma warning disable 4014
			                                     .Subscribe(e => LoadPackages(false));
#pragma warning restore 4014
		}

		#endregion
	}
}