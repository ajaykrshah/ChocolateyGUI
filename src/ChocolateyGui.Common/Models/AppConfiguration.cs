// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using chocolatey;
	using ChocolateyGui.Common.Attributes;
	using ChocolateyGui.Common.Properties;

	public class AppConfiguration
	{
		#region Properties

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleAllowNonAdminAccessToSettingsDescription))]
		[Feature]
		public Boolean? AllowNonAdminAccessToSettings { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ConfigDefaultSourceName))]
		[Config]
		public String DefaultSourceName { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleDefaultToDarkMode))]
		[Feature]
		public Boolean? DefaultToDarkMode { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleDefaultTileViewLocalDescription))]
		[Feature]
		public Boolean? DefaultToTileViewForLocalSource { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleDefaultTileViewRemoteDescription))]
		[Feature]
		public Boolean? DefaultToTileViewForRemoteSource { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleExcludeInstalledPackagesDescription))]
		[Feature]
		public Boolean? ExcludeInstalledPackages { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleHideAllRemoteChocolateySources))]
		[Feature]
		public Boolean? HideAllRemoteChocolateySources { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_TogglePackageDownloadCountDescription))]
		[Feature]
		public Boolean? HidePackageDownloadCount { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleHideThisPCSourceDescription))]
		[Feature]
		public Boolean? HideThisPCSource { get; set; }

		public String Id { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ConfigOutdatedPackagesCacheDurationInMinutesDescription))]
		[Config]
		public String OutdatedPackagesCacheDurationInMinutes { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_TogglePreventAllPackageIconDownloads))]
		[Feature]
		public Boolean? PreventAllPackageIconDownloads { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_TogglePreventAutomatedOutdatedPackagesCheckDescription))]
		[Feature]
		public Boolean? PreventAutomatedOutdatedPackagesCheck { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_TogglePreventPreloadDescription))]
		[Feature]
		public Boolean? PreventPreload { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_TogglePreventUsageOfUpdateAllButtonDescription))]
		[Feature]
		public Boolean? PreventUsageOfUpdateAllButton { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleShowAdditionalPackageInformationDescription))]
		[Feature]
		public Boolean? ShowAdditionalPackageInformation { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleShowAggregatedSourceViewDescription))]
		[Feature]
		public Boolean? ShowAggregatedSourceView { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleShowConsoleOutputDescription))]
		[Feature]
		public Boolean? ShowConsoleOutput { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleSkipModalDialogConfirmationDescription))]
		[Feature]
		public Boolean? SkipModalDialogConfirmation { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleUseDelayedSearchDescription))]
		[Feature]
		public Boolean? UseDelayedSearch { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_ToggleUseKeyboardBindings))]
		[Feature]
		public Boolean? UseKeyboardBindings { get; set; }

		[LocalizedDescription(nameof(Resources.SettingsView_LanguageDescription))]
		[Config]
		public String UseLanguage { get; set; }

		#endregion

		#region object overrides

		public override String ToString()
		{
			return @"
OutdatedPackagesCacheDurationInMinutes: {0}
DefaultSourceName: {1}
UseLanguage: {2}
ShowConsoleOutput: {3}
DefaultToTileViewForLocalSource: {4}
DefaultToTileViewForRemoteSource: {5}
UseDelayedSearch: {6}
PreventPreload: {7}
PreventAutomatedOutdatedPackagesCheck: {8}
ExcludeInstalledPackages: {9}
ShowAggregatedSourceView: {10}
ShowAdditionalPackageInformation: {11}
AllowNonAdminAccessToSettings: {12}
UseKeyboardBindings: {13}
HidePackageDownloadCount: {14}
PreventAllPackageIconDownloads: {15}
HideAllRemoteChocolateySources: {16}
DefaultToDarkMode: {17}
HideThisPCSource: {18}
PreventUsageOfUpdateAllButton: {19}
SkipModalDialogConfirmation: {20}
".FormatWith(this.OutdatedPackagesCacheDurationInMinutes, this.DefaultSourceName, this.UseLanguage, this.ShowConsoleOutput, this.DefaultToTileViewForLocalSource, this.DefaultToTileViewForRemoteSource, this.UseDelayedSearch, this.PreventPreload, this.PreventAutomatedOutdatedPackagesCheck, this.ExcludeInstalledPackages, this.ShowAggregatedSourceView, this.ShowAdditionalPackageInformation, this.AllowNonAdminAccessToSettings, this.UseKeyboardBindings, this.HidePackageDownloadCount, this.PreventAllPackageIconDownloads, this.HideAllRemoteChocolateySources, this.DefaultToDarkMode, this.HideThisPCSource, this.PreventUsageOfUpdateAllButton, this.SkipModalDialogConfirmation);
		}

		#endregion
	}
}