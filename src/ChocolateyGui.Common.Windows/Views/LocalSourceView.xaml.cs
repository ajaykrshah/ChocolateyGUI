// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Views
{
	using System;
	using System.Windows;
	using System.Windows.Input;
	using ChocolateyGui.Common.ViewModels.Items;

	/// <summary>
	///     Interaction logic for LocalSourceView.xaml
	/// </summary>
	public partial class LocalSourceView
	{
		#region Constructors

		public LocalSourceView()
		{
			InitializeComponent();

			PART_Loading.Margin = new Thickness(0, 0, 13, 0);

			Loaded += LocalSourceViewOnLoaded;
		}

		#endregion

		#region Private implementation

		private void LocalSourceViewOnLoaded(Object sender, RoutedEventArgs e)
		{
			SearchTextBox.Focus();
		}

		private void PackageDoubleClick(Object sender, MouseButtonEventArgs e)
		{
			dynamic source = e.OriginalSource;
			var item = source.DataContext as IPackageViewModel;
			item?.ViewDetails();
		}

		#endregion
	}
}