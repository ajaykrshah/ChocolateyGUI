// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System.Collections;
	using System.ComponentModel;

	public interface IDataGridColumnComparer : IComparer
	{
		ListSortDirection SortDirection { get; set; }
	}
}