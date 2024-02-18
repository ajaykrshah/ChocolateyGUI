// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Views
{
	using System;
	using System.Diagnostics;
	using System.Windows.Controls;
	using System.Windows.Input;

	/// <summary>
	///     Interaction logic for AboutView.xaml
	/// </summary>
	public partial class AboutView : UserControl
	{
		#region Constructors

		public AboutView()
		{
			InitializeComponent();
		}

		#endregion

		#region Private implementation

		private void HandleMarkdownLink(Object sender, ExecutedRoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Parameter.ToString()));
		}

		#endregion
	}
}