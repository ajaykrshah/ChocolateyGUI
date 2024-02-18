// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Views
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Globalization;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Markup;
	using System.Windows.Media.Imaging;
	using Caliburn.Micro;
	using chocolatey.infrastructure.filesystem;
	using ChocolateyGui.Common.Providers;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.Windows.Controls.Dialogs;
	using ChocolateyGui.Common.Windows.Services;
	using ChocolateyGui.Common.Windows.Utilities;
	using MahApps.Metro.Controls.Dialogs;

	/// <summary>
	///     Interaction logic for ShellView.xaml
	/// </summary>
	public partial class ShellView
	{
		#region Declarations

		private readonly IChocolateyConfigurationProvider _chocolateyConfigurationProvider;
		private readonly IConfigService _configService;
		private readonly IFileSystem _fileSystem;
		private readonly IImageService _imageService;
		private readonly IProgressService _progressService;
		private Boolean _closeInitiated;

		#endregion

		#region Constructors

		public ShellView(
			IDialogService dialogService,
			IProgressService progressService,
			IChocolateyConfigurationProvider chocolateyConfigurationProvider,
			IConfigService configService,
			IFileSystem fileSystem,
			IImageService imageService)
		{
			InitializeComponent();

			dialogService.ShellView = this;
			progressService.ShellView = this;

			_progressService = progressService;
			_chocolateyConfigurationProvider = chocolateyConfigurationProvider;
			_configService = configService;
			_fileSystem = fileSystem;
			_imageService = imageService;

			this.Icon = BitmapFrame.Create(_imageService.ToolbarIconUri);

			CheckOperatingSystemCompatibility();

			// Certain things like Cef (our markdown browser engine) get unhappy when GUI is started from a different cwd.
			// If we're in a different one, reset it to our app files directory.
			if (_fileSystem.GetDirectoryName(Environment.CurrentDirectory) != Bootstrapper.ApplicationFilesPath)
			{
				Environment.CurrentDirectory = Bootstrapper.ApplicationFilesPath;
			}

			dialogService.ChildWindowOpened += (sender, o) => this.IsAnyDialogOpen = true;
			dialogService.ChildWindowClosed += (sender, o) => this.IsAnyDialogOpen = false;

			SetLanguage(TranslationSource.Instance.CurrentCulture);

			TranslationSource.Instance.PropertyChanged += TranslationLanguageChanged;
		}

		#endregion

		#region Public interface

		public void CheckOperatingSystemCompatibility()
		{
			var operatingSystemVersion = Environment.OSVersion;

			if (operatingSystemVersion.Version.Major == 10 &&
			    !_chocolateyConfigurationProvider.IsChocolateyExecutableBeingUsed)
			{
				// TODO: Possibly make these values translatable, do not use Resources directly, instead Use TranslationSource.Instance["KEY_NAME"];
				ChocolateyMessageBox.Show(
					"Usage of the PowerShell Version of Chocolatey (i.e. <= 0.9.8.33) has been detected.  Chocolatey GUI does not support using this version of Chocolatey on Windows 10.  Please update Chocolatey to the new C# Version (i.e. > 0.9.9.0) and restart Chocolatey GUI.  This application will now close.",
					"Incompatible Operating System Version",
					MessageBoxButton.OK,
					MessageBoxImage.Error,
					MessageBoxResult.OK,
					MessageBoxOptions.ServiceNotification);

				Application.Current.Shutdown();
			}
		}

		public Task<ChocolateyDialogController> ShowChocolateyDialogAsync(
			String title,
			Boolean isCancelable = false,
			MetroDialogSettings settings = null)
		{
			return this.Dispatcher.Invoke(async () =>
			{
				// create the dialog control
				var dialog = new ChocolateyDialog(this, _configService.GetEffectiveConfiguration().ShowConsoleOutput ?? false)
				             {
					             Title = title,
					             IsCancelable = isCancelable,
					             OutputBufferCollection = _progressService.Output
				             };

				if (settings == null)
				{
					settings = this.MetroDialogOptions;
					settings.NegativeButtonText = ShellView.L(nameof(Properties.Resources.ChocolateyDialog_Cancel));
					settings.AffirmativeButtonText = ShellView.L(nameof(Properties.Resources.ChocolateyDialog_OK));
				}

				dialog.NegativeButtonText = settings.NegativeButtonText;

				await this.ShowMetroDialogAsync(dialog);
				return new ChocolateyDialogController(dialog, () => this.HideMetroDialogAsync(dialog));
			});
		}

		#endregion

		#region MetroWindow overrides

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!_closeInitiated)
			{
				e.Cancel = true;
				_closeInitiated = true;
				var bootstrapper = (Bootstrapper)Application.Current.FindResource("Bootstrapper");
#pragma warning disable 4014

				// ReSharper disable once PossibleNullReferenceException
				bootstrapper.OnExitAsync().ContinueWith(t => Execute.OnUIThread(Close));
#pragma warning restore 4014

				// fire other Closing events too
				base.OnClosing(e);
			}
		}

		#endregion

		#region Private implementation

		private void CanGoToPage(Object sender, CanExecuteRoutedEventArgs e)
		{
			// GEP: I can't think of any reason that we would want to prevent going to the linked
			// page, so just going to default this to returning true
			e.CanExecute = true;
		}

		private static String L(String key)
		{
			return TranslationSource.Instance[key];
		}

		private void PerformGoToPage(Object sender, ExecutedRoutedEventArgs e)
		{
			// https://github.com/theunrepentantgeek/Markdown.XAML/issues/5
			Process.Start(new ProcessStartInfo(e.Parameter.ToString()));
			e.Handled = true;
		}

		private void SetLanguage(CultureInfo culture)
		{
			// This was introduced after testing Chocolatey GUI with Chocolatey GUI Licensed Extension.
			// When installed, and run for the first time, the Culture is null, which we believe is due
			// to the way that the overriding of the ShellViewModel is done within the Caliburn.Micro
			// view locator.  Since this only happens on initial loading of Chocolatey GUI, the thought
			// is to simply return, as this doesn't cause any other known issues.
			if (culture == null)
			{
				return;
			}

			this.Language = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
			this.FlowDirection = culture.TextInfo.IsRightToLeft
				? FlowDirection.RightToLeft
				: FlowDirection.LeftToRight;
		}

		private void TranslationLanguageChanged(Object sender, PropertyChangedEventArgs e)
		{
			SetLanguage(TranslationSource.Instance.CurrentCulture);
		}

		#endregion
	}
}