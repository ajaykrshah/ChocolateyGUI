// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;
	using System.Collections.Generic;
	using ChocolateyGui.Common.Models;

	public interface IConfigService
	{
		void GetConfigValue(ChocolateyGuiConfiguration configuration);
		AppConfiguration GetEffectiveConfiguration();
		IEnumerable<ChocolateyGuiFeature> GetFeatures(Boolean global);
		IEnumerable<ChocolateyGuiFeature> GetFeatures(Boolean global, Boolean useResourceKeys);
		AppConfiguration GetGlobalConfiguration();
		IEnumerable<ChocolateyGuiSetting> GetSettings(Boolean global);
		IEnumerable<ChocolateyGuiSetting> GetSettings(Boolean global, Boolean useResourceKeys);
		void ListFeatures(ChocolateyGuiConfiguration configuration);
		void ListSettings(ChocolateyGuiConfiguration configuration);
		void SetConfigValue(String key, String value);
		void SetConfigValue(ChocolateyGuiConfiguration configuration);
		void ToggleFeature(ChocolateyGuiConfiguration configuration, Boolean requiredValue);
		void UnsetConfigValue(ChocolateyGuiConfiguration configuration);
		void UpdateSettings(AppConfiguration settings, Boolean global);
	}
}