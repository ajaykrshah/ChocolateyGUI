// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using ChocolateyGui.Common.ViewModels;

	public class SourcesListBoxItemStyleSelector : StyleSelector
	{
		#region Properties

		public Style ListBoxItemContainerStyleKey { get; set; }
		public Style SeparatorContainerStyleKey { get; set; }

		#endregion

		#region StyleSelector overrides

		public override Style SelectStyle(Object item, DependencyObject container)
		{
			if (item is SourceSeparatorViewModel && this.SeparatorContainerStyleKey != null)
			{
				return this.SeparatorContainerStyleKey;
			}

			if (item is ISourceViewModelBase && this.ListBoxItemContainerStyleKey != null)
			{
				return this.ListBoxItemContainerStyleKey;
			}

			return base.SelectStyle(item, container);
		}

		#endregion
	}
}