// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	public class PackageOperationResult
	{
		#region Fields and constants

		public static readonly PackageOperationResult SuccessfulCached = new PackageOperationResult
		                                                                 {
			                                                                 Successful = true
		                                                                 };

		#endregion

		#region Properties

		public Exception Exception { get; set; }
		public String[] Messages { get; set; } = new String[0];
		public Boolean Successful { get; set; }

		#endregion
	}
}