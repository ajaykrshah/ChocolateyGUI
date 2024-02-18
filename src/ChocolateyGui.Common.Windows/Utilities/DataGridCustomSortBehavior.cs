// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using Microsoft.Xaml.Behaviors;

	public class DataGridCustomSortBehavior : Behavior<DataGrid>
	{
		#region Fields and constants

		public static readonly DependencyProperty CustomComparerProperty
			= DependencyProperty.RegisterAttached("CustomComparer", typeof(IDataGridColumnComparer), typeof(DataGridCustomSortBehavior));

		#endregion

		#region Public interface

		public static IDataGridColumnComparer GetCustomComparer(DataGridColumn gridColumn)
		{
			return (IDataGridColumnComparer)gridColumn.GetValue(DataGridCustomSortBehavior.CustomComparerProperty);
		}

		public static void SetCustomComparer(DataGridColumn gridColumn, IDataGridColumnComparer value)
		{
			gridColumn.SetValue(DataGridCustomSortBehavior.CustomComparerProperty, value);
		}

		#endregion

		#region Behavior overrides

		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.Sorting += HandleCustomSorting;
		}

		protected override void OnDetaching()
		{
			this.AssociatedObject.Sorting -= HandleCustomSorting;

			base.OnDetaching();
		}

		#endregion

		#region Private implementation

		private void HandleCustomSorting(Object sender, DataGridSortingEventArgs e)
		{
			var dataGrid = sender as DataGrid;
			if (dataGrid == null)
			{
				return;
			}

			var listColView = dataGrid.ItemsSource as ListCollectionView;
			if (listColView == null)
			{
				// The DataGrid's ItemsSource property must be of type, ListCollectionView
				return;
			}

			var direction = (e.Column.SortDirection != ListSortDirection.Ascending)
				? ListSortDirection.Ascending
				: ListSortDirection.Descending;

			// Get the custom sorter for this column
			var sorter = DataGridCustomSortBehavior.GetCustomComparer(e.Column);
			if (sorter == null)
			{
				if (listColView.CustomSort != null)
				{
					e.Handled = true;
					e.Column.SortDirection = direction;
					listColView.CustomSort = null;
				}

				return;
			}

			// Yes, we handle it
			e.Handled = true;
			e.Column.SortDirection = sorter.SortDirection = direction;
			listColView.CustomSort = sorter;
		}

		#endregion
	}
}