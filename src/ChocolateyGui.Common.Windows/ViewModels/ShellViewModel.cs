// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using System.Linq;
	using System.Windows.Input;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Providers;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Windows.Commands;
	using ChocolateyGui.Common.Windows.Utilities;
	using ChocolateyGui.Common.Windows.ViewModels.Items;

	public class ShellViewModel : Conductor<Object>.Collection.OneActive,
	                              IHandle<ShowPackageDetailsMessage>,
	                              IHandle<ShowSourcesMessage>,
	                              IHandle<ShowSettingsMessage>,
	                              IHandle<ShowPackagePublishMessage>,
	                              IHandle<ShowAboutMessage>,
	                              IHandle<SettingsGoBackMessage>,
	                              IHandle<AboutGoBackMessage>,
	                              IHandle<PackagePublishGoBackMessage>,
	                              IHandle<FeatureModifiedMessage>
	{
		#region Declarations

		private readonly IChocolateyService _chocolateyPackageService;
		private readonly IConfigService _configService;
		private readonly IEventAggregator _eventAggregator;
		private readonly SourcesViewModel _sourcesViewModel;
		private readonly IVersionNumberProvider _versionNumberProvider;
		private Object _lastActiveItem;

		#endregion

		#region Constructors

		public ShellViewModel(
			IChocolateyService chocolateyPackageService,
			IVersionNumberProvider versionNumberProvider,
			IEventAggregator eventAggregator,
			SourcesViewModel sourcesViewModel,
			IConfigService configService)
		{
			_chocolateyPackageService = chocolateyPackageService;
			_versionNumberProvider = versionNumberProvider;
			_eventAggregator = eventAggregator;
			_sourcesViewModel = sourcesViewModel;
			_configService = configService;
			this.Sources = new BindableCollection<SourceViewModel>();
			this.ActiveItem = _sourcesViewModel;
			this.GoToSourceCommand = new RelayCommand(GoToSource, CanGoToSource);
		}

		#endregion

		#region Properties

		public String AboutInformation
			=> ResourceReader.GetFromResources(GetType().Assembly, "ChocolateyGui.Resources.ABOUT.md");

		public virtual Boolean CanShowSettings => true;

		public String Credits
			=> ResourceReader.GetFromResources(GetType().Assembly, "ChocolateyGui.Resources.CREDITS.md");

		public ICommand GoToSourceCommand { get; }

		public String ReleaseNotes
			=> ResourceReader.GetFromResources(GetType().Assembly, "ChocolateyGui.Resources.CHANGELOG.md");

		public BindableCollection<SourceViewModel> Sources { get; set; }
		public SourcesViewModel SourcesSelectorViewModel => _sourcesViewModel;
		public String VersionNumber => _versionNumberProvider.Version;

		#endregion

		#region Public interface

		public void ShowAbout()
		{
			if (this.ActiveItem is AboutViewModel)
			{
				return;
			}

			SetActiveItem(IoC.Get<AboutViewModel>());
		}

		public void ShowPublish()
		{
			if (this.ActiveItem is PackagePublishViewModel)
			{
				return;
			}

			SetActiveItem(IoC.Get<PackagePublishViewModel>());
		}

		public void ShowSettings()
		{
			if (this.ActiveItem is SettingsViewModel)
			{
				return;
			}

			SetActiveItem(IoC.Get<SettingsViewModel>());
		}

		#endregion

		#region Screen overrides

		protected override void OnInitialize()
		{
			_eventAggregator.Subscribe(this);
		}

		#endregion

		#region IHandle<AboutGoBackMessage> implementation

		public void Handle(AboutGoBackMessage message)
		{
			SetActiveItem(_sourcesViewModel);
		}

		#endregion

		#region IHandle<FeatureModifiedMessage> implementation

		public void Handle(FeatureModifiedMessage message)
		{
			NotifyOfPropertyChange(nameof(ShellViewModel.CanShowSettings));
		}

		#endregion

		#region IHandle<PackagePublishGoBackMessage> implementation

		public void Handle(PackagePublishGoBackMessage message)
		{
			SetActiveItem(_sourcesViewModel);
		}

		#endregion

		#region IHandle<SettingsGoBackMessage> implementation

		public void Handle(SettingsGoBackMessage message)
		{
			SetActiveItem(_sourcesViewModel);
		}

		#endregion

		#region IHandle<ShowAboutMessage> implementation

		public void Handle(ShowAboutMessage message)
		{
			ShowAbout();
		}

		#endregion

		#region IHandle<ShowPackageDetailsMessage> implementation

		public void Handle(ShowPackageDetailsMessage message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			if (message.Package == null)
			{
				throw new ArgumentNullException(nameof(message.Package));
			}

			var packageViewModel = this.ActiveItem as PackageViewModel;
			if (packageViewModel != null && packageViewModel.Package.Id == message.Package.Id)
			{
				return;
			}

			var packageVm = IoC.Get<PackageViewModel>();
			packageVm.Package = message.Package;
			SetActiveItem(packageVm);
		}

		#endregion

		#region IHandle<ShowPackagePublishMessage> implementation

		public void Handle(ShowPackagePublishMessage message)
		{
			ShowPublish();
		}

		#endregion

		#region IHandle<ShowSettingsMessage> implementation

		public void Handle(ShowSettingsMessage message)
		{
			ShowSettings();
		}

		#endregion

		#region IHandle<ShowSourcesMessage> implementation

		public void Handle(ShowSourcesMessage message)
		{
			SetActiveItem(_sourcesViewModel);
		}

		#endregion

		#region Private implementation

		private Boolean CanGoToSource(Object obj)
		{
			if (!_configService.GetEffectiveConfiguration().UseKeyboardBindings ?? true)
			{
				return false;
			}

			var sourceIndex = obj as Int32?;
			return sourceIndex.HasValue && sourceIndex > 0 && sourceIndex <= _sourcesViewModel.Items.Count(vm => !(vm is SourceSeparatorViewModel));
		}

		private async void GetSources()
		{
			var sources =
				(await _chocolateyPackageService.GetSources()).Select(
					source => new SourceViewModel { Name = source.Id, Url = source.Value });
			this.Sources.AddRange(sources);
		}

		private void GoToSource(Object obj)
		{
			var sourceIndex = obj as Int32?;
			if (sourceIndex.HasValue)
			{
				--sourceIndex;

				var items = _sourcesViewModel.Items.Where(vm => !(vm is SourceSeparatorViewModel)).ToList();
				if (sourceIndex < 0 || sourceIndex > items.Count)
				{
					return;
				}

				_sourcesViewModel.ActivateItem(items[sourceIndex.Value]);
			}
		}

		private void SetActiveItem<T>(T newItem)
		{
			if (_lastActiveItem != null && _lastActiveItem.Equals(newItem))
			{
				_lastActiveItem = null;
			}
			else
			{
				_lastActiveItem = this.ActiveItem;
				DeactivateItem(this.ActiveItem, false);
			}

			ActivateItem(newItem);
			if (_lastActiveItem is PackageViewModel)
			{
				this.CloseItem(_lastActiveItem);
			}
		}

		#endregion
	}
}