// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Views
{
	using System;
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Documents;
	using System.Windows.Input;

	/// <summary>
	///     Interaction logic for PackageView.xaml
	/// </summary>
	public partial class PackageView
	{
		#region Constructors

		public PackageView()
		{
			InitializeComponent();
		}

		#endregion

		#region Private implementation

		private void HandleLinkClick(Object sender, RoutedEventArgs e)
		{
			var hl = (Hyperlink)sender;
			var navigateUri = hl.NavigateUri.ToString();
			Process.Start(new ProcessStartInfo(navigateUri));
			e.Handled = true;
		}

		private void HandleMarkdownLink(Object sender, ExecutedRoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Parameter.ToString()));
		}

		#endregion
	}
}