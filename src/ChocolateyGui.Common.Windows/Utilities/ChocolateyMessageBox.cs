// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Windows;
	using System.Windows.Media;

	public static class ChocolateyMessageBox
	{
		#region Public interface

		public static MessageBoxResult Show(String messageBoxText)
		{
			return ChocolateyMessageBox.Show(messageBoxText, String.Empty);
		}

		public static MessageBoxResult Show(String messageBoxText, String caption)
		{
			return ChocolateyMessageBox.Show(messageBoxText, caption, MessageBoxButton.OK);
		}

		public static MessageBoxResult Show(String messageBoxText, String caption, MessageBoxButton button)
		{
			return ChocolateyMessageBox.Show(messageBoxText, caption, button, MessageBoxImage.None);
		}

		public static MessageBoxResult Show(String messageBoxText, String caption, MessageBoxButton button, MessageBoxImage icon)
		{
			return ChocolateyMessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
		}

		public static MessageBoxResult Show(String messageBoxText, String caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
		{
			return ChocolateyMessageBox.Show(messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
		}

		public static MessageBoxResult Show(String messageBoxText, String caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
		{
			var dummyWindow = ChocolateyMessageBox.DummyWindow();
			dummyWindow.Show();
			var result = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
			dummyWindow.Show();
			return result;
		}

		public static MessageBoxResult Show(Window owner, String messageBoxText)
		{
			return ChocolateyMessageBox.Show(owner, messageBoxText, String.Empty);
		}

		public static MessageBoxResult Show(Window owner, String messageBoxText, String caption)
		{
			return ChocolateyMessageBox.Show(owner, messageBoxText, caption, MessageBoxButton.OK);
		}

		public static MessageBoxResult Show(Window owner, String messageBoxText, String caption, MessageBoxButton button)
		{
			return ChocolateyMessageBox.Show(owner, messageBoxText, caption, button, MessageBoxImage.None);
		}

		public static MessageBoxResult Show(Window owner, String messageBoxText, String caption, MessageBoxButton button, MessageBoxImage icon)
		{
			return ChocolateyMessageBox.Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.OK);
		}

		public static MessageBoxResult Show(Window owner, String messageBoxText, String caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
		{
			return ChocolateyMessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
		}

		public static MessageBoxResult Show(Window owner, String messageBoxText, String caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
		{
			var dummyWindow = ChocolateyMessageBox.DummyWindow();
			dummyWindow.Show();
			var result = MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, options);
			dummyWindow.Show();
			return result;
		}

		#endregion

		#region Private implementation

		private static Window DummyWindow()
		{
			return new Window
			       {
				       AllowsTransparency = true,
				       Background = Brushes.Transparent,
				       WindowStyle = WindowStyle.None,
				       Top = 0,
				       Left = 0,
				       Width = 1,
				       Height = 1,
				       ShowInTaskbar = false
			       };
		}

		#endregion
	}
}