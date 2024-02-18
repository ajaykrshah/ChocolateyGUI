// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	public class PackageResults
	{
		#region Properties

		public Package[] Packages { get; set; }
		public Int32 TotalCount { get; set; }

		#endregion
	}
}