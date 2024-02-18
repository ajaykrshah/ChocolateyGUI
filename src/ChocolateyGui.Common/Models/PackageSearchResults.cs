// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using System.Collections.Generic;
	using ChocolateyGui.Common.ViewModels.Items;

	public class PackageSearchResults
	{
		#region Properties

		public IEnumerable<IPackageViewModel> Packages { get; set; }
		public Int32 TotalCount { get; set; }

		#endregion
	}
}