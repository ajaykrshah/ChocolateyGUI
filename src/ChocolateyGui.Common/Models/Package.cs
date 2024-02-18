// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using NuGet.Versioning;

	public class Package
	{
		#region Properties

		public String[] Authors { get; set; }
		public String Copyright { get; set; }
		public String Dependencies { get; set; }
		public String Description { get; set; }
		public Int64 DownloadCount { get; set; }
		public String GalleryDetailsUrl { get; set; }
		public String IconUrl { get; set; }
		public String Id { get; set; }
		public Int32 Index { get; set; }
		public Boolean IsInstalled { get; set; }
		public Boolean IsPinned { get; set; }
		public Boolean IsPrerelease { get; set; }
		public String Language { get; set; }
		public String LatestVersion { get; set; }
		public String LicenseUrl { get; set; }
		public String[] Owners { get; set; }
		public String PackageHash { get; set; }
		public String PackageHashAlgorithm { get; set; }
		public Int64 PackageSize { get; set; }
		public String ProjectUrl { get; set; }
		public DateTimeOffset Published { get; set; }
		public String ReleaseNotes { get; set; }
		public String ReportAbuseUrl { get; set; }
		public String RequireLicenseAcceptance { get; set; }
		public Uri Source { get; set; }
		public String Summary { get; set; }
		public String Tags { get; set; }
		public String Title { get; set; }
		public NuGetVersion Version { get; set; }
		public Int64 VersionDownloadCount { get; set; }

		#endregion
	}
}