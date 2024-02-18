// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Providers.PlatformProvider
{
	using System;
	using System.Drawing;
	using System.Windows;
	using System.Windows.Interop;

	public class Windows7PlatformProvider : IPlatformProvider
	{
		#region IPlatformProvider implementation

		public Tuple<Single, Single> GetDpiScaleFactor()
		{
			Single dpiX, dpiY;
			var hwnd = IntPtr.Zero;
			if (Application.Current?.MainWindow != null)
			{
				hwnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
			}

			using (var graphics = Graphics.FromHwnd(hwnd))
			{
				dpiX = graphics.DpiX;
				dpiY = graphics.DpiY;
			}

			return Tuple.Create(dpiX / 96.0f, dpiY / 96.0f);
		}

		#endregion
	}
}