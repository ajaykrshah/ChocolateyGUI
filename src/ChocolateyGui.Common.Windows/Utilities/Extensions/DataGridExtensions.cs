// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Extensions
{
	using System;
	using System.ComponentModel;
	using System.Windows.Controls;
	using System.Windows.Data;

	internal static class DataGridExtensions
	{
		#region Public interface

		public static Int32 FindSortDescription(this SortDescriptionCollection sortDescriptions, String sortPropertyName)
		{
			var index = -1;
			var i = 0;
			foreach (var sortDesc in sortDescriptions)
			{
				if (String.CompareOrdinal(sortDesc.PropertyName, sortPropertyName) == 0)
				{
					index = i;
					break;
				}

				i++;
			}

			return index;
		}

		public static String GetSortMemberPath(this DataGridColumn column)
		{
			// find the sortmemberpath
			String sortPropertyName;

			var boundColumn = column as DataGridBoundColumn;
			if (boundColumn == null)
			{
				return null;
			}

			var binding = boundColumn.Binding as Binding;
			if (binding == null)
			{
				return null;
			}

			if (!String.IsNullOrEmpty(binding.XPath))
			{
				sortPropertyName = binding.XPath;
			}
			else if (binding.Path != null)
			{
				sortPropertyName = binding.Path.Path;
			}
			else
			{
				sortPropertyName = null;
			}

			return sortPropertyName;
		}

		#endregion
	}
}