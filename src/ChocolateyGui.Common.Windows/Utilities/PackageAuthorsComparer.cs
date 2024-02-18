// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using ChocolateyGui.Common.ViewModels.Items;

	public class PackageAuthorsComparer : IDataGridColumnComparer, IComparer<IPackageViewModel>
	{
		#region IComparer implementation

		public Int32 Compare(Object x, Object y)
		{
			return Compare(x as IPackageViewModel, y as IPackageViewModel);
		}

		#endregion

		#region IComparer<IPackageViewModel> implementation

		public Int32 Compare(IPackageViewModel x, IPackageViewModel y)
		{
			if (x?.Authors == null)
			{
				return -1;
			}

			if (y?.Authors == null)
			{
				return 1;
			}

			var a = String.Join(", ", x.Authors.Select(item => item.Trim()).ToList());
			var b = String.Join(", ", y.Authors.Select(item => item.Trim()).ToList());
			var result = String.Compare(a, b, StringComparison.OrdinalIgnoreCase);
			return this.SortDirection == ListSortDirection.Ascending ? result : -result;
		}

		#endregion

		#region IDataGridColumnComparer implementation

		public ListSortDirection SortDirection { get; set; }

		#endregion
	}
}