// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common
{
	using System;
	using chocolatey;
	using chocolatey.infrastructure.app.configuration;

	public static class ChocolateyConfigurationExtensions
	{
		#region Public interface

		[Obsolete("This is used only for backwards compatibility, should be removed when updating Chocolatey CLI reference in the nuspec.")]
		public static Boolean HasCacheExpirationInMinutes()
		{
			var configType = typeof(ChocolateyConfiguration);

			return configType.GetProperty("CacheExpirationInMinutes") != null;
		}

		[Obsolete("This is used only for backwards compatibility, should be removed when updating Chocolatey CLI reference in the nuspec.")]
		public static void SetCacheExpirationInMinutes(this ChocolateyConfiguration config, Int32 cacheExpirationInMinutes)
		{
			var configType = config.GetType();

			var property = configType.GetProperty("CacheExpirationInMinutes");

			if (property != null)
			{
				property.SetValue(config, cacheExpirationInMinutes);
			}
			else
			{
				"chocolatey".Log().Warn("CacheExpirationInMinutes property is not available. Unable to ignore existing cached items!");
			}
		}

		#endregion
	}
}