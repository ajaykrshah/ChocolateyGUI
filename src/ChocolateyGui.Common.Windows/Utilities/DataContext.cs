// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Windows;

	public static class DataContext
	{
		#region Public interface

		public static Object GetDataContext(Object element)
		{
			var fe = element as FrameworkElement;
			if (fe != null)
			{
				return fe.DataContext;
			}

			var fce = element as FrameworkContentElement;
			return fce == null ? null : fce.DataContext;
		}

		#endregion
	}
}