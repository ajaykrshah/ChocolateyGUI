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
	using System.Reactive.Subjects;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using Caliburn.Micro;
	using chocolatey.infrastructure.filesystem;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.Windows.Services;
	using ChocolateyGui.Common.Windows.Startup;
	using ChocolateyGui.Common.Windows.Theming;
	using ChocolateyGui.Common.Windows.Utilities.Extensions;
	using MahApps.Metro.Controls.Dialogs;

	public sealed class SettingsViewModel : ViewModelScreen
	{
		#region Declarations

		private const String ChocolateyLicensedSourceId = "chocolatey.licensed";
		private readonly IChocolateyGuiCacheService _chocolateyGuiCacheService;
		private readonly IChocolateyService _chocolateyService;
		private readonly IConfigService _configService;
		private readonly IDialogService _dialogService;
		private readonly IEventAggregator _eventAggregator;
		private readonly IFileSystem _fileSystem;
		private readonly IProgressService _progressService;
		private readonly TranslationSource _translationSource;
		private Subject<ChocolateyFeature> _changedChocolateyFeature;
		private Subject<ChocolateyGuiFeature> _changedChocolateyGuiFeature;
		private Subject<ChocolateyGuiSetting> _changedChocolateyGuiSetting;
		private Subject<ChocolateySetting> _changedChocolateySetting;
		private String _chocolateyFeatureSearchQuery;
		private String _chocolateyGuiFeatureSearchQuery;
		private String _chocolateyGuiSettingSearchQuery;
		private String _chocolateySettingsSearchQuery;
		private AppConfiguration _config;
		private ChocolateySource _draftSource;
		private Boolean _isNewItem;
		private String _originalId;
		private ChocolateySource _selectedSource;

		#endregion

		#region Constructors

		public SettingsViewModel(
			IChocolateyService chocolateyService,
			IDialogService dialogService,
			IProgressService progressService,
			IConfigService configService,
			IEventAggregator eventAggregator,
			IChocolateyGuiCacheService chocolateyGuiCacheService,
			IFileSystem fileSystem,
			TranslationSource translationSource)
			: base(translationSource)
		{
			_chocolateyService = chocolateyService;
			_dialogService = dialogService;
			_progressService = progressService;
			_configService = configService;
			_eventAggregator = eventAggregator;
			_chocolateyGuiCacheService = chocolateyGuiCacheService;
			_fileSystem = fileSystem;
			_translationSource = translationSource;
			this.DisplayName = L(nameof(Resources.SettingsViewModel_DisplayName));
			Activated += OnActivated;
			Deactivated += OnDeactivated;

			this.ChocolateyGuiFeaturesView.Filter = o => FilterChocolateyGuiFeatures(o as ChocolateyGuiFeature);
			this.ChocolateyGuiSettingsView.Filter = o => FilterChocolateyGuiSettings(o as ChocolateyGuiSetting);
			this.ChocolateyFeaturesView.Filter = o => FilterChocolateyFeatures(o as ChocolateyFeature);
			this.ChocolateySettingsView.Filter = o => FilterChocolateySettings(o as ChocolateySetting);
		}

		#endregion

		#region Properties

		public ObservableCollection<CultureInfo> AllLanguages { get; private set; } =
			new ObservableCollection<CultureInfo>();

		public Boolean CanCancel => this.SelectedSource != null;
		public Boolean CanRemove => this.SelectedSource != null && !_isNewItem && this.SelectedSource.Id != SettingsViewModel.ChocolateyLicensedSourceId;
		public Boolean CanSave => this.SelectedSource != null;
		public ObservableCollection<ChocolateyFeature> ChocolateyFeatures { get; } = new ObservableCollection<ChocolateyFeature>();

		public String ChocolateyFeatureSearchQuery
		{
			get => _chocolateyFeatureSearchQuery;
			set
			{
				_chocolateyFeatureSearchQuery = value;
				NotifyOfPropertyChange();
				this.ChocolateyFeaturesView.Refresh();
			}
		}

		public ICollectionView ChocolateyFeaturesView => CollectionViewSource.GetDefaultView(this.ChocolateyFeatures);
		public ObservableCollection<ChocolateyGuiFeature> ChocolateyGuiFeatures { get; } = new ObservableCollection<ChocolateyGuiFeature>();

		public String ChocolateyGuiFeatureSearchQuery
		{
			get => _chocolateyGuiFeatureSearchQuery;
			set
			{
				_chocolateyGuiFeatureSearchQuery = value;
				NotifyOfPropertyChange();
				this.ChocolateyGuiFeaturesView.Refresh();
			}
		}

		public ICollectionView ChocolateyGuiFeaturesView => CollectionViewSource.GetDefaultView(this.ChocolateyGuiFeatures);
		public ObservableCollection<ChocolateyGuiSetting> ChocolateyGuiSettings { get; } = new ObservableCollection<ChocolateyGuiSetting>();

		public String ChocolateyGuiSettingSearchQuery
		{
			get => _chocolateyGuiSettingSearchQuery;
			set
			{
				_chocolateyGuiSettingSearchQuery = value;
				NotifyOfPropertyChange();
				this.ChocolateyGuiSettingsView.Refresh();
			}
		}

		public ICollectionView ChocolateyGuiSettingsView => CollectionViewSource.GetDefaultView(this.ChocolateyGuiSettings);
		public ObservableCollection<ChocolateySetting> ChocolateySettings { get; } = new ObservableCollection<ChocolateySetting>();

		public String ChocolateySettingSearchQuery
		{
			get => _chocolateySettingsSearchQuery;
			set
			{
				_chocolateySettingsSearchQuery = value;
				NotifyOfPropertyChange();
				this.ChocolateySettingsView.Refresh();
			}
		}

		public ICollectionView ChocolateySettingsView => CollectionViewSource.GetDefaultView(this.ChocolateySettings);

		public ChocolateySource DraftSource
		{
			get => _draftSource;
			set => this.SetPropertyValue(ref _draftSource, value);
		}

		public Boolean IsChocolateyLicensedSource => this.DraftSource != null && this.DraftSource.Id == SettingsViewModel.ChocolateyLicensedSourceId;
		public Boolean IsSourceEditable => this.DraftSource != null && this.DraftSource.Id != SettingsViewModel.ChocolateyLicensedSourceId;

		public ChocolateySource SelectedSource
		{
			get => _selectedSource;
			set
			{
				this.SetPropertyValue(ref _selectedSource, value);
				if (value != null && value.Id == null)
				{
					_isNewItem = true;
				}
				else
				{
					_isNewItem = false;
					_originalId = value?.Id;
				}

				this.DraftSource = value == null ? null : new ChocolateySource(value);
				NotifyOfPropertyChange(nameof(SettingsViewModel.CanSave));
				NotifyOfPropertyChange(nameof(SettingsViewModel.CanRemove));
				NotifyOfPropertyChange(nameof(SettingsViewModel.CanCancel));
				NotifyOfPropertyChange(nameof(SettingsViewModel.IsSourceEditable));
				NotifyOfPropertyChange(nameof(SettingsViewModel.IsChocolateyLicensedSource));
			}
		}

		public ObservableCollection<ChocolateySource> Sources { get; } = new ObservableCollection<ChocolateySource>();

		public CultureInfo UseLanguage
		{
			get
			{
				if (String.IsNullOrEmpty(_config.UseLanguage))
				{
					return Internationalization.GetFallbackCulture();
				}

				return new CultureInfo(_config.UseLanguage);
			}
			set
			{
				_config.UseLanguage = value.Name;
				NotifyOfPropertyChange();
				Internationalization.UpdateLanguage(value.Name);

				// We explicitly update settings when the language changes
				_configService.SetConfigValue(nameof(SettingsViewModel.UseLanguage), value.Name);
				this.ChocolateyGuiFeaturesView.Refresh();
				this.ChocolateyGuiSettingsView.Refresh();
			}
		}

		#endregion

		#region Public interface

		public void Back()
		{
			_eventAggregator.PublishOnUIThread(new SettingsGoBackMessage());
		}

		public void Cancel()
		{
			this.DraftSource = new ChocolateySource(this.SelectedSource);
		}

		public void ChocolateyFeatureToggled(ChocolateyFeature feature)
		{
			_changedChocolateyFeature.OnNext(feature);
		}

		public void ChocolateyGuiFeatureToggled(ChocolateyGuiFeature feature)
		{
			_changedChocolateyGuiFeature.OnNext(feature);
		}

		public async void ChocolateyGuiSettingsRowEditEnding(DataGridRowEditEndingEventArgs eventArgs)
		{
			await Task.Delay(100);
			_changedChocolateyGuiSetting.OnNext((ChocolateyGuiSetting)eventArgs.Row.Item);
		}

		public async void ChocolateySettingsRowEditEnding(DataGridRowEditEndingEventArgs eventArgs)
		{
			await Task.Delay(100);
			_changedChocolateySetting.OnNext((ChocolateySetting)eventArgs.Row.Item);
		}

		public void New()
		{
			this.SelectedSource = new ChocolateySource();
		}

		public async Task PurgeIconCache()
		{
			var result = MessageDialogResult.Affirmative;
			if (!_config.SkipModalDialogConfirmation.GetValueOrDefault(false))
			{
				result = await _dialogService.ShowConfirmationMessageAsync(
					L(nameof(Resources.Dialog_AreYouSureTitle)),
					L(nameof(Resources.Dialog_AreYouSureIconsMessage)));
			}

			if (result == MessageDialogResult.Affirmative)
			{
				_chocolateyGuiCacheService.PurgeIcons();
			}
		}

		public async Task PurgeOutdatedPackagesCache()
		{
			var result = MessageDialogResult.Affirmative;
			if (!_config.SkipModalDialogConfirmation.GetValueOrDefault(false))
			{
				result = await _dialogService.ShowConfirmationMessageAsync(
					L(nameof(Resources.Dialog_AreYouSureTitle)),
					L(nameof(Resources.Dialog_AreYouSureOutdatedPackagesMessage)));
			}

			if (result == MessageDialogResult.Affirmative)
			{
				_chocolateyGuiCacheService.PurgeOutdatedPackages();
			}
		}

		public async Task Remove()
		{
			var result = MessageDialogResult.Affirmative;
			if (!_config.SkipModalDialogConfirmation.GetValueOrDefault(false))
			{
				result = await _dialogService.ShowConfirmationMessageAsync(
					L(nameof(Resources.Dialog_AreYouSureTitle)),
					L(nameof(Resources.Dialog_AreYourSureRemoveSourceMessage), _originalId));
			}

			if (result == MessageDialogResult.Affirmative)
			{
				await _progressService.StartLoading(L(nameof(Resources.SettingsViewModel_RemovingSource)));
				try
				{
					await _chocolateyService.RemoveSource(_originalId);
					this.Sources.Remove(this.SelectedSource);
					this.SelectedSource = null;
					await _eventAggregator.PublishOnUIThreadAsync(new SourcesUpdatedMessage());
				}
				catch (UnauthorizedAccessException)
				{
					await _dialogService.ShowMessageAsync(
						L(nameof(Resources.General_UnauthorisedException_Title)),
						L(nameof(Resources.General_UnauthorisedException_Description)));
				}
				finally
				{
					await _progressService.StopLoading();
				}
			}
		}

		public async Task Save()
		{
			if (String.IsNullOrWhiteSpace(this.DraftSource.Id))
			{
				await _dialogService.ShowMessageAsync(L(nameof(Resources.SettingsViewModel_SavingSource)), L(nameof(Resources.SettingsViewModel_SourceMissingId)));
				return;
			}

			if (String.IsNullOrWhiteSpace(this.DraftSource.Value))
			{
				await _dialogService.ShowMessageAsync(L(nameof(Resources.SettingsViewModel_SavingSource)), L(nameof(Resources.SettingsViewModel_SourceMissingValue)));
				return;
			}

			await _progressService.StartLoading(L(nameof(Resources.SettingsViewModel_SavingSourceLoading)));
			try
			{
				if (_isNewItem)
				{
					if (this.DraftSource.Id == SettingsViewModel.ChocolateyLicensedSourceId)
					{
						await _progressService.StopLoading();
						await _dialogService.ShowMessageAsync(L(nameof(Resources.SettingsViewModel_SavingSource)), L(nameof(Resources.SettingsViewModel_InvalidSourceId)));
						return;
					}

					await _chocolateyService.AddSource(this.DraftSource);
					_isNewItem = false;
					this.Sources.Add(this.DraftSource);
					NotifyOfPropertyChange(nameof(SettingsViewModel.CanRemove));
				}
				else
				{
					if (this.DraftSource.Id == SettingsViewModel.ChocolateyLicensedSourceId)
					{
						if (this.DraftSource.Disabled)
						{
							await _chocolateyService.DisableSource(this.DraftSource.Id);
						}
						else
						{
							await _chocolateyService.EnableSource(this.DraftSource.Id);
						}
					}
					else
					{
						await _chocolateyService.UpdateSource(_originalId, this.DraftSource);
					}

					this.Sources[this.Sources.IndexOf(this.SelectedSource)] = this.DraftSource;
				}

				_originalId = this.DraftSource?.Id;
				await _eventAggregator.PublishOnUIThreadAsync(new SourcesUpdatedMessage());
			}
			catch (UnauthorizedAccessException)
			{
				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.General_UnauthorisedException_Title)),
					L(nameof(Resources.General_UnauthorisedException_Description)));
			}
			finally
			{
				this.SelectedSource = null;
				await _progressService.StopLoading();
			}
		}

		public async void SetCertificateAndPassword()
		{
			var loginDialogSettings = new LoginDialogSettings
			                          {
				                          AffirmativeButtonText = L(nameof(Resources.SettingsView_ButtonSave)),
				                          NegativeButtonText = L(nameof(Resources.SettingsView_ButtonCancel)),
				                          NegativeButtonVisibility = Visibility.Visible,
				                          InitialUsername = this.DraftSource.Certificate,
				                          InitialPassword = this.DraftSource.CertificatePassword,
				                          UsernameWatermark = L(nameof(Resources.SettingsViewModel_SetSourceCertificateAndPasswordUsernameWatermark)),
				                          PasswordWatermark = L(nameof(Resources.SettingsViewModel_SetSourceCertificateAndPasswordPasswordWatermark))
			                          };

			// Only allow the previewing of a password when creating a new source
			// not when modifying an existing source
			if (_isNewItem)
			{
				loginDialogSettings.EnablePasswordPreview = true;
			}

			var result = await _dialogService.ShowLoginAsync(L(nameof(Resources.SettingsViewModel_SetSourceCertificateAndPasswordTitle)), L(nameof(Resources.SettingsViewModel_SetSourceCertificateAndPasswordMessage)), loginDialogSettings);

			if (result != null)
			{
				this.DraftSource.Certificate = result.Username;
				this.DraftSource.CertificatePassword = result.Password;
				NotifyOfPropertyChange(nameof(SettingsViewModel.DraftSource));
			}
		}

		public async void SetUserAndPassword()
		{
			var loginDialogSettings = new LoginDialogSettings
			                          {
				                          AffirmativeButtonText = L(nameof(Resources.SettingsView_ButtonSave)),
				                          NegativeButtonText = L(nameof(Resources.SettingsView_ButtonCancel)),
				                          NegativeButtonVisibility = Visibility.Visible,
				                          InitialUsername = this.DraftSource.UserName,
				                          InitialPassword = this.DraftSource.Password
			                          };

			// Only allow the previewing of a password when creating a new source
			// not when modifying an existing source
			if (_isNewItem)
			{
				loginDialogSettings.EnablePasswordPreview = true;
			}

			var result = await _dialogService.ShowLoginAsync(L(nameof(Resources.SettingsViewModel_SetSourceUsernameAndPasswordTitle)), L(nameof(Resources.SettingsViewModel_SetSourceUsernameAndPasswordMessage)), loginDialogSettings);

			if (result != null)
			{
				this.DraftSource.UserName = result.Username;
				this.DraftSource.Password = result.Password;
				NotifyOfPropertyChange(nameof(SettingsViewModel.DraftSource));
			}
		}

		public void SourceSelectionChanged(Object source)
		{
			var sourceItem = (ChocolateySource)source;
			this.SelectedSource = sourceItem;
		}

		public async Task UpdateChocolateyFeature(ChocolateyFeature feature)
		{
			try
			{
				await _chocolateyService.SetFeature(feature);
			}
			catch (UnauthorizedAccessException)
			{
				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.General_UnauthorisedException_Title)),
					L(nameof(Resources.General_UnauthorisedException_Description)));
			}
		}

		public async Task UpdateChocolateyGuiFeature(ChocolateyGuiFeature feature)
		{
			// When the flow direction gets changed, this results in the feature
			// being null some times immediately. As such, if the feature is null
			// then just return so we don't encounter an exception.
			if (feature == null)
			{
				return;
			}

			var configuration = new ChocolateyGuiConfiguration();
			configuration.CommandName = "feature";
			configuration.FeatureCommand.Name = feature.Title;

			if (feature.Enabled)
			{
				configuration.FeatureCommand.Command = FeatureCommandType.Enable;
				await Task.Run(() => _configService.ToggleFeature(configuration, true));
			}
			else
			{
				configuration.FeatureCommand.Command = FeatureCommandType.Disable;
				await Task.Run(() => _configService.ToggleFeature(configuration, false));
			}

			_eventAggregator.PublishOnUIThread(new FeatureModifiedMessage());

			if (feature.Title == "ShowAggregatedSourceView")
			{
				await _eventAggregator.PublishOnUIThreadAsync(new SourcesUpdatedMessage());
			}

			if (feature.Title == "DefaultToDarkMode")
			{
				ThemeAssist.BundledTheme.IsLightTheme = !feature.Enabled;
				ThemeAssist.BundledTheme.ToggleTheme.Execute(null);
			}
		}

		public async Task UpdateChocolateyGuiSetting(ChocolateyGuiSetting setting)
		{
			var configuration = new ChocolateyGuiConfiguration();
			configuration.CommandName = "config";
			configuration.ConfigCommand.Name = setting.Key;
			configuration.ConfigCommand.ConfigValue = setting.Value;

			await Task.Run(() => _configService.SetConfigValue(configuration));
		}

		public async Task UpdateChocolateySetting(ChocolateySetting setting)
		{
			await _chocolateyService.SetSetting(setting);
		}

		#endregion

		#region ViewModelScreen overrides

		protected override void OnLanguageChanged()
		{
			this.DisplayName = L(nameof(Resources.SettingsViewModel_DisplayName));
		}

		#endregion

		#region Private implementation

		private Boolean FilterChocolateyFeatures(ChocolateyFeature chocolateyFeature)
		{
			return this.ChocolateyFeatureSearchQuery == null
			       || chocolateyFeature.Name.IndexOf(this.ChocolateyFeatureSearchQuery, StringComparison.OrdinalIgnoreCase) != -1;
		}

		private Boolean FilterChocolateyGuiFeatures(ChocolateyGuiFeature chocolateyGuiFeature)
		{
			return this.ChocolateyGuiFeatureSearchQuery == null
			       || chocolateyGuiFeature.Title.IndexOf(this.ChocolateyGuiFeatureSearchQuery, StringComparison.OrdinalIgnoreCase) != -1
			       || chocolateyGuiFeature.DisplayTitle.IndexOf(this.ChocolateyGuiFeatureSearchQuery, StringComparison.OrdinalIgnoreCase) != -1;
		}

		private Boolean FilterChocolateyGuiSettings(ChocolateyGuiSetting chocolateyGuiSetting)
		{
			return this.ChocolateyGuiSettingSearchQuery == null
			       || chocolateyGuiSetting.Key.IndexOf(this.ChocolateyGuiSettingSearchQuery, StringComparison.OrdinalIgnoreCase) != -1
			       || chocolateyGuiSetting.DisplayName.IndexOf(this.ChocolateyGuiSettingSearchQuery, StringComparison.OrdinalIgnoreCase) != -1;
		}

		private Boolean FilterChocolateySettings(ChocolateySetting chocolateySetting)
		{
			return this.ChocolateySettingSearchQuery == null
			       || chocolateySetting.Key.IndexOf(this.ChocolateySettingSearchQuery, StringComparison.OrdinalIgnoreCase) != -1;
		}

		private async void OnActivated(Object sender, ActivationEventArgs activationEventArgs)
		{
			_config = _configService.GetEffectiveConfiguration();

			var chocolateyFeatures = await _chocolateyService.GetFeatures();
			foreach (var chocolateyFeature in chocolateyFeatures)
			{
#if !DEBUG // We hide this during DEBUG as it is a dark feature
				var descriptionKey = "Chocolatey_" + chocolateyFeature.Name + "Description";

				var newDescription = _translationSource[descriptionKey];

				if (String.IsNullOrEmpty(newDescription))
				{
					descriptionKey = chocolateyFeature.Description;
					newDescription = _translationSource[descriptionKey];
				}

				if (!String.IsNullOrEmpty(newDescription))
				{
					chocolateyFeature.Description = newDescription;
					_translationSource.PropertyChanged += (s, e) => { chocolateyFeature.Description = _translationSource[descriptionKey]; };
				}
#endif
				this.ChocolateyFeatures.Add(chocolateyFeature);
			}

			_changedChocolateyFeature = new Subject<ChocolateyFeature>();
			_changedChocolateyFeature
				.Select(f => Observable.FromAsync(() => UpdateChocolateyFeature(f)))
				.Concat()
				.Subscribe();

			var chocolateySettings = await _chocolateyService.GetSettings();
			foreach (var chocolateySetting in chocolateySettings)
			{
#if !DEBUG // We hide this during DEBUG as it is a dark feature
				var descriptionKey = "Chocolatey_" + chocolateySetting.Key + "Description";

				var newDescription = _translationSource[descriptionKey];

				if (String.IsNullOrEmpty(newDescription))
				{
					descriptionKey = chocolateySetting.Description;
					newDescription = _translationSource[descriptionKey];
				}

				if (!String.IsNullOrEmpty(newDescription))
				{
					chocolateySetting.Description = newDescription;
					_translationSource.PropertyChanged += (s, e) => { chocolateySetting.Description = _translationSource[descriptionKey]; };
				}
#endif
				this.ChocolateySettings.Add(chocolateySetting);
			}

			_changedChocolateySetting = new Subject<ChocolateySetting>();
			_changedChocolateySetting
				.Select(s => Observable.FromAsync(() => UpdateChocolateySetting(s)))
				.Concat()
				.Subscribe();

			var chocolateyGuiFeatures = _configService.GetFeatures(global: false, useResourceKeys: true);
			foreach (var chocolateyGuiFeature in chocolateyGuiFeatures)
			{
				chocolateyGuiFeature.DisplayTitle = _translationSource["ChocolateyGUI_" + chocolateyGuiFeature.Title + "Title"];
#if DEBUG
				var descriptionKey = String.Empty;
#else
				var descriptionKey = "ChocolateyGUI_" + chocolateyGuiFeature.Title + "Description";
#endif

				var newDescription = _translationSource[descriptionKey];

				if (String.IsNullOrEmpty(newDescription))
				{
					descriptionKey = chocolateyGuiFeature.Description;
					newDescription = _translationSource[descriptionKey];
				}

				if (!String.IsNullOrEmpty(newDescription))
				{
					chocolateyGuiFeature.Description = newDescription;
					_translationSource.PropertyChanged += (s, e) =>
					{
						chocolateyGuiFeature.DisplayTitle = _translationSource["ChocolateyGUI_" + chocolateyGuiFeature.Title + "Title"];
						chocolateyGuiFeature.Description = _translationSource[descriptionKey];
					};
				}

				this.ChocolateyGuiFeatures.Add(chocolateyGuiFeature);
			}

			_changedChocolateyGuiFeature = new Subject<ChocolateyGuiFeature>();
			_changedChocolateyGuiFeature
				.Select(s => Observable.FromAsync(() => UpdateChocolateyGuiFeature(s)))
				.Concat()
				.Subscribe();

			var chocolateyGuiSettings = _configService.GetSettings(global: false, useResourceKeys: true);
			foreach (var chocolateyGuiSetting in chocolateyGuiSettings.Where(c => !String.Equals(c.Key, nameof(SettingsViewModel.UseLanguage), StringComparison.OrdinalIgnoreCase)))
			{
				chocolateyGuiSetting.DisplayName = _translationSource["ChocolateyGUI_" + chocolateyGuiSetting.Key + "Title"];
#if DEBUG
				var descriptionKey = String.Empty;
#else
				var descriptionKey = "ChocolateyGUI_" + chocolateyGuiSetting.Key + "Description";
#endif

				var newDescription = _translationSource[descriptionKey];

				if (String.IsNullOrEmpty(newDescription))
				{
					descriptionKey = chocolateyGuiSetting.Description;
					newDescription = _translationSource[descriptionKey];
				}

				if (!String.IsNullOrEmpty(newDescription))
				{
					chocolateyGuiSetting.Description = newDescription;
					_translationSource.PropertyChanged += (s, e) =>
					{
						chocolateyGuiSetting.DisplayName =
							_translationSource["ChocolateyGUI_" + chocolateyGuiSetting.Key + "Title"];
						chocolateyGuiSetting.Description = _translationSource[descriptionKey];
					};
				}

				this.ChocolateyGuiSettings.Add(chocolateyGuiSetting);
			}

			_changedChocolateyGuiSetting = new Subject<ChocolateyGuiSetting>();
			_changedChocolateyGuiSetting
				.Select(s => Observable.FromAsync(() => UpdateChocolateyGuiSetting(s)))
				.Concat()
				.Subscribe();

			var sources = await _chocolateyService.GetSources();
			foreach (var source in sources)
			{
				this.Sources.Add(source);
			}

			this.AllLanguages.Clear();

			foreach (var language in Internationalization.GetAllSupportedCultures().OrderBy(c => c.NativeName))
			{
				this.AllLanguages.Add(language);
			}

			var selectedLanguage = _config.UseLanguage;

			// We set it to the configuration itself, instead of the property
			// as we do not want to save the configuration file when it is not needed.
			_config.UseLanguage = Internationalization.GetSupportedCultureInfo(selectedLanguage).Name;
			NotifyOfPropertyChange(nameof(SettingsViewModel.UseLanguage));
		}

		private void OnDeactivated(Object sender, DeactivationEventArgs deactivationEventArgs)
		{
			_changedChocolateyFeature.OnCompleted();
			_changedChocolateySetting.OnCompleted();
			_changedChocolateyGuiFeature.OnCompleted();
			_changedChocolateyGuiSetting.OnCompleted();
		}

		#endregion
	}
}