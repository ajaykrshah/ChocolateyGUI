// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Windows;

	/// <summary>
	///     BindingProxy class with a DependencyProperty as the backing store for Data.
	///     This enables animation, styling, binding, etc...
	///     And prevents errors like:
	///     System.Windows.Data Error: 2 : Cannot find governing FrameworkElement or FrameworkContentElement for target
	///     element. BindingExpression:Path=ShowConsoleOutput; DataItem=null; target element is 'ChocolateyGUISetting'
	///     (HashCode=61659320); target property is 'Enabled' (type 'Boolean')
	/// </summary>
	public class BindingProxy : Freezable
	{
		#region Fields and constants

		public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
			"Data",
			typeof(Object),
			typeof(BindingProxy),
			new UIPropertyMetadata(null));

		#endregion

		#region Properties

		public Object Data
		{
			get => GetValue(BindingProxy.DataProperty);
			set => SetValue(BindingProxy.DataProperty, value);
		}

		#endregion

		#region Freezable overrides

		protected override Freezable CreateInstanceCore()
		{
			return new BindingProxy();
		}

		#endregion
	}
}