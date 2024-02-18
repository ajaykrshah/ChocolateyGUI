// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.ViewModels.Items
{
	using System;
	using System.Threading.Tasks;
	using NuGet.Versioning;

	public interface IPackageViewModel
	{
		String[] Authors { get; set; }
		Boolean CanUpdate { get; }
		String Copyright { get; set; }
		String Dependencies { get; set; }
		String Description { get; set; }
		Int64 DownloadCount { get; set; }
		String GalleryDetailsUrl { get; set; }
		String IconUrl { get; set; }
		String Id { get; set; }
		Int32 Index { get; set; }
		Boolean IsInstalled { get; set; }
		Boolean IsOutdated { get; set; }
		Boolean IsPinned { get; set; }
		Boolean IsPrerelease { get; set; }
		String Language { get; set; }
		NuGetVersion LatestVersion { get; }
		String LicenseUrl { get; set; }
		String[] Owners { get; set; }
		String PackageHash { get; set; }
		String PackageHashAlgorithm { get; set; }
		Int64 PackageSize { get; set; }
		String ProjectUrl { get; set; }
		DateTimeOffset Published { get; set; }
		String ReleaseNotes { get; set; }
		String ReportAbuseUrl { get; set; }
		String RequireLicenseAcceptance { get; set; }
		Uri Source { get; set; }
		String Summary { get; set; }
		String Tags { get; set; }
		String Title { get; set; }
		NuGetVersion Version { get; set; }
		Int64 VersionDownloadCount { get; set; }
		Task Install();
		Task InstallAdvanced();
		Task Uninstall();
		Task Update();
		void ViewDetails();
	}
}