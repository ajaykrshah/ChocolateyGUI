// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using ChocolateyGui.Common.Enums;
	using ChocolateyGui.Common.Models;
	using NuGet.Versioning;

	public interface IChocolateyService
	{
		Task AddSource(ChocolateySource source);
		Task DisableSource(String id);
		Task EnableSource(String id);
		Task ExportPackages(String exportFilePath, Boolean includeVersionNumbers);
		Task<List<NuGetVersion>> GetAvailableVersionsForPackageIdAsync(String id, Int32 page, Int32 pageSize, Boolean includePreRelease);
		Task<Package> GetByVersionAndIdAsync(String id, String version, Boolean isPrerelease);
		Task<ChocolateyFeature[]> GetFeatures();
		Task<IEnumerable<Package>> GetInstalledPackages();
		Task<IReadOnlyList<OutdatedPackage>> GetOutdatedPackages(Boolean includePrerelease = false, String packageName = null, Boolean forceCheckForOutdatedPackages = false);
		Task<ChocolateySetting[]> GetSettings();
		Task<ChocolateySource[]> GetSources();

		Task<PackageOperationResult> InstallPackage(
			String id,
			String version = null,
			Uri source = null,
			Boolean force = false,
			AdvancedInstall advancedInstallOptions = null);

		Task<Boolean> IsElevated();
		Task<PackageOperationResult> PackPackage(GenericCommandType commandName, String workingDirectoryPath = null);
		Task<PackageOperationResult> PinPackage(String id, String version);
		Task<PackageOperationResult> PublishPackage(GenericCommandType commandName, String workingDirectoryPath = null);
		Task<Boolean> RemoveSource(String id);
		Task<PackageResults> Search(String query, PackageSearchOptions options);
		Task SetFeature(ChocolateyFeature feature);
		Task SetSetting(ChocolateySetting setting);
		Task<PackageOperationResult> UninstallPackage(String id, String version, Boolean force = false);
		Task<PackageOperationResult> UnpinPackage(String id, String version);
		Task<PackageOperationResult> UpdatePackage(String id, Uri source = null);
		Task UpdateSource(String id, ChocolateySource source);
	}
}