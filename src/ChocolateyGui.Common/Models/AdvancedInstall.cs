// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	public class AdvancedInstall
	{
		#region Properties

		public Boolean AllowDowngrade { get; set; }
		public Boolean AllowEmptyChecksums { get; set; }
		public Boolean AllowEmptyChecksumsSecure { get; set; }
		public Boolean ApplyInstallArgumentsToDependencies { get; set; }
		public Boolean ApplyPackageParametersToDependencies { get; set; }
		public String CacheLocation { get; set; }
		public String DownloadChecksum { get; set; }
		public String DownloadChecksum64bit { get; set; }
		public String DownloadChecksumType { get; set; }
		public String DownloadChecksumType64bit { get; set; }
		public Int32 ExecutionTimeoutInSeconds { get; set; }
		public Boolean ForceDependencies { get; set; }
		public Boolean Forcex86 { get; set; }
		public Boolean IgnoreChecksums { get; set; }
		public Boolean IgnoreDependencies { get; set; }
		public Boolean IgnoreHttpCache { get; set; }
		public String InstallArguments { get; set; }
		public String LogFile { get; set; }
		public Boolean NotSilent { get; set; }
		public Boolean OverrideArguments { get; set; }
		public String PackageParameters { get; set; }
		public Boolean PreRelease { get; set; }
		public Boolean RequireChecksums { get; set; }
		public Boolean SkipPowerShell { get; set; }

		#endregion
	}
}