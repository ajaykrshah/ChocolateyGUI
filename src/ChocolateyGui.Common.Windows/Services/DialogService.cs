// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.Windows.Controls.Dialogs;
	using ChocolateyGui.Common.Windows.Utilities;
	using ChocolateyGui.Common.Windows.Views;
	using ControlzEx.Theming;
	using MahApps.Metro.Controls.Dialogs;
	using MahApps.Metro.SimpleChildWindow;
	using Microsoft.VisualStudio.Threading;

	public class DialogService : IDialogService
	{
		#region Declarations

		private readonly AsyncSemaphore _lock;
		private RoutedEventHandler _childWindowLoadedHandler;
		private EventHandler<ThemeChangedEventArgs> _themeChangedHandler;

		#endregion

		#region Constructors

		public DialogService()
		{
			_lock = new AsyncSemaphore(1);
		}

		#endregion

		#region IDialogService implementation

		public event EventHandler<Object> ChildWindowClosed;
		public event EventHandler<Object> ChildWindowOpened;
		public ShellView ShellView { get; set; }

		/// <inheritdoc />
		public async Task<TResult> ShowChildWindowAsync<TDialogContext, TResult>(
			String title,
			Object dialogContent,
			TDialogContext dialogContext)
			where TDialogContext : IClosableChildWindow<TResult>
		{
			using (await _lock.EnterAsync())
			{
				if (this.ShellView != null)
				{
					var overlayBrush = new SolidColorBrush(((SolidColorBrush)this.ShellView.OverlayBrush).Color)
					                   {
						                   Opacity = this.ShellView.OverlayOpacity
					                   };
					overlayBrush.Freeze();

					var childWindow = new ChildWindow
					                  {
						                  Title = title,
						                  Content = dialogContent,
						                  DataContext = dialogContext,
						                  IsModal = true,
						                  AllowMove = true,
						                  ShowCloseButton = true,
						                  BorderThickness = new Thickness(1),
						                  OverlayBrush = overlayBrush
					                  };

					childWindow.SetResourceReference(Control.BorderBrushProperty, "MahApps.Brushes.Highlight");

					_childWindowLoadedHandler = (sender, e) =>
					{
						ChildWindowOpened?.Invoke(sender, e);

						var cw = (ChildWindow)sender;
						cw.ClosingFinished += (senderObj, r) => ChildWindowClosed?.Invoke(senderObj, r);

						if (cw.DataContext is IClosableChildWindow<TResult> vm)
						{
							vm.Close += r => { cw.Close(r); };
						}
						else
						{
							cw.Close();
						}
					};

					_themeChangedHandler = (s, e) =>
					{
						if (this.ShellView.OverlayBrush is SolidColorBrush brush)
						{
							overlayBrush = new SolidColorBrush(brush.Color)
							               {
								               Opacity = this.ShellView.OverlayOpacity
							               };
							overlayBrush.Freeze();

							childWindow.OverlayBrush = overlayBrush;
						}
					};

					childWindow.Loaded += _childWindowLoadedHandler;
					ThemeManager.Current.ThemeChanged += _themeChangedHandler;

					var result = await this.ShellView.ShowChildWindowAsync<TResult>(childWindow);

					childWindow.Loaded -= _childWindowLoadedHandler;
					ThemeManager.Current.ThemeChanged -= _themeChangedHandler;

					return result;
				}

				return default;
			}
		}

		/// <inheritdoc />
		public async Task<MessageDialogResult> ShowConfirmationMessageAsync(String title, String message)
		{
			using (await _lock.EnterAsync())
			{
				if (this.ShellView != null)
				{
					var dialogSettings = new MetroDialogSettings
					                     {
						                     AffirmativeButtonText = DialogService.L(nameof(Resources.Dialog_Yes)),
						                     NegativeButtonText = DialogService.L(nameof(Resources.Dialog_No))
					                     };

					return await this.ShellView.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, dialogSettings);
				}

				return ChocolateyMessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes
					? MessageDialogResult.Affirmative
					: MessageDialogResult.Negative;
			}
		}

		/// <inheritdoc />
		public async Task<TResult> ShowDialogAsync<TDialogContext, TResult>(
			String title,
			Object dialogContent,
			TDialogContext dialogContext,
			MetroDialogSettings settings = null)
			where TDialogContext : IClosableDialog<TResult>
		{
			using (await _lock.EnterAsync())
			{
				if (this.ShellView != null)
				{
					var customDialog = new CustomDialog
					                   {
						                   Title = title,
						                   Content = dialogContent,
						                   DialogContentMargin = new GridLength(1, GridUnitType.Star),
						                   DialogContentWidth = GridLength.Auto
					                   };

					await this.ShellView.ShowMetroDialogAsync(customDialog, settings);

					var result = await dialogContext.WaitForClosingAsync();

					await this.ShellView.HideMetroDialogAsync(customDialog, settings);

					return result;
				}

				return default;
			}
		}

		/// <inheritdoc />
		public async Task<LoginDialogData> ShowLoginAsync(String title, String message, LoginDialogSettings settings = null)
		{
			using (await _lock.EnterAsync())
			{
				if (this.ShellView != null)
				{
					return await this.ShellView.ShowLoginAsync(
						DialogService.L(nameof(Resources.SettingsViewModel_SetSourceUsernameAndPasswordTitle)),
						DialogService.L(nameof(Resources.SettingsViewModel_SetSourceUsernameAndPasswordMessage)),
						settings);
				}

				return null;
			}
		}

		/// <inheritdoc />
		public async Task<MessageDialogResult> ShowMessageAsync(String title, String message)
		{
			using (await _lock.EnterAsync())
			{
				if (this.ShellView != null)
				{
					var dialogSettings = new MetroDialogSettings
					                     {
						                     AffirmativeButtonText = DialogService.L(nameof(Resources.ChocolateyDialog_OK))
					                     };

					return await this.ShellView.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, dialogSettings);
				}

				return ChocolateyMessageBox.Show(message, title) == MessageBoxResult.OK
					? MessageDialogResult.Affirmative
					: MessageDialogResult.Negative;
			}
		}

		#endregion

		#region Private implementation

		private static String L(String key)
		{
			return TranslationSource.Instance[key];
		}

		private static String L(String key, params Object[] parameters)
		{
			return TranslationSource.Instance[key, parameters];
		}

		#endregion
	}
}