// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.ViewModels.Items;

	/// <summary>
	///     Interaction logic for RemoteSourceControl.xaml
	/// </summary>
	public partial class RemoteSourceView : IHandle<ResetScrollPositionMessage>
	{
		#region Constructors

		public RemoteSourceView(IEventAggregator eventAggregator)
		{
			if (eventAggregator == null)
			{
				throw new ArgumentNullException(nameof(eventAggregator));
			}

			InitializeComponent();

			eventAggregator.Subscribe(this);

			Loaded += RemoteSourceViewOnLoaded;
		}

		#endregion

		#region IHandle<ResetScrollPositionMessage> implementation

		public void Handle(ResetScrollPositionMessage message)
		{
			if (Packages.Items.Count > 0)
			{
				Packages.ScrollIntoView(Packages.Items[0]);
			}
		}

		#endregion

		#region Private implementation

		private void Packages_OnMouseDoubleClick(Object sender, MouseButtonEventArgs e)
		{
			var obj = (DependencyObject)e.OriginalSource;

			while (obj != null && obj != Packages)
			{
				var listBoxItem = obj as ListBoxItem;
				if (listBoxItem != null)
				{
					var context = (IPackageViewModel)listBoxItem.DataContext;
					context.ViewDetails();
				}

				obj = VisualTreeHelper.GetParent(obj);
			}
		}

		private void RemoteSourceViewOnLoaded(Object sender, RoutedEventArgs e)
		{
			SearchTextBox.Focus();
		}

		#endregion
	}
}