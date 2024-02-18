// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System.Windows;

	public class NullToVisibility : NullToValue
	{
		#region Constructors

		public NullToVisibility()
		{
			this.TrueValue = Visibility.Collapsed;
			this.FalseValue = Visibility.Visible;
		}

		#endregion
	}
}