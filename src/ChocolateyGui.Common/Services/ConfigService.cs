// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using chocolatey;
	using ChocolateyGui.Common.Attributes;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Utilities;
	using LiteDB;
	using Serilog;

	public class ConfigService : IConfigService
	{
		#region Declarations

		private static readonly TranslationSource TranslationSource = TranslationSource.Instance;

		#endregion

		#region Constructors

		public ConfigService(LiteDatabase globalDatabase, LiteDatabase userDatabase)
		{
			var defaultGlobalSettings = new AppConfiguration
			                            {
				                            Id = "v0.18.0",
				                            OutdatedPackagesCacheDurationInMinutes = "60",
				                            UseKeyboardBindings = true,
				                            DefaultToTileViewForLocalSource = true,
				                            DefaultToTileViewForRemoteSource = true
			                            };

			var defaultUserSettings = new AppConfiguration
			                          {
				                          Id = "v0.18.0"
			                          };

			// If the global database is null, the assumption has to be that we are running as a non-administrator
			// user, as such, we should proceed with default settings
			if (globalDatabase == null)
			{
				this.GlobalCollection = null;
				this.GlobalAppConfiguration = defaultGlobalSettings;
			}
			else
			{
				this.GlobalCollection = globalDatabase.GetCollection<AppConfiguration>(nameof(AppConfiguration));
				this.GlobalAppConfiguration = this.GlobalCollection.FindById("v0.18.0") ?? defaultGlobalSettings;
			}

			this.UserCollection = userDatabase.GetCollection<AppConfiguration>(nameof(AppConfiguration));
			this.UserAppConfiguration = this.UserCollection.FindById("v0.18.0") ?? defaultUserSettings;
		}

		#endregion

		#region Events

		public event EventHandler SettingsChanged;

		#endregion

		#region Properties

		public AppConfiguration EffectiveAppConfiguration { get; set; }
		public AppConfiguration GlobalAppConfiguration { get; set; }
		public ILiteCollection<AppConfiguration> GlobalCollection { get; set; }
		public static ILogger Logger => Log.ForContext<ConfigService>();
		public AppConfiguration UserAppConfiguration { get; set; }
		public ILiteCollection<AppConfiguration> UserCollection { get; set; }

		#endregion

		#region Public interface

		public virtual void SetEffectiveConfiguration()
		{
			this.EffectiveAppConfiguration = new AppConfiguration();

			var properties = typeof(AppConfiguration).GetProperties();
			foreach (var property in properties)
			{
				if (property.Name == "Id")
				{
					continue;
				}

				var featureAttributes = property.GetCustomAttributes(typeof(FeatureAttribute), true);

				Object globalPropertyValue;
				Object userPropertyValue;

				if (featureAttributes.Length > 0)
				{
					globalPropertyValue = ConfigService.GetPropertyValue<Boolean?>(this.GlobalAppConfiguration, property);
					userPropertyValue = ConfigService.GetPropertyValue<Boolean?>(this.UserAppConfiguration, property);
				}
				else
				{
					globalPropertyValue = (String)property.GetValue(this.GlobalAppConfiguration);
					userPropertyValue = (String)property.GetValue(this.UserAppConfiguration);
				}

				// Neither the user or global values have been set, so do nothing
				if (userPropertyValue == null && globalPropertyValue == null)
				{
					continue;
				}

				// If the user hasn't explicitly set a value, take the global value
				if (userPropertyValue == null)
				{
					property.SetValue(this.EffectiveAppConfiguration, globalPropertyValue);
					continue;
				}

				// If we get here, we know userPropertyValue is not null, so if globalPropertyValue
				// hasn't been set, take the userPropertyValue
				if (globalPropertyValue == null)
				{
					property.SetValue(this.EffectiveAppConfiguration, userPropertyValue);
					continue;
				}

				// At this point, both aren't null, so if they are the same, use global
				if (globalPropertyValue.Equals(userPropertyValue))
				{
					property.SetValue(this.EffectiveAppConfiguration, globalPropertyValue);
					continue;
				}

				// If they aren't the same, use user, since user can override
				property.SetValue(this.EffectiveAppConfiguration, userPropertyValue);
			}

			ConfigService.Logger.Debug("GlobalAppConfiguration Settings");
			ConfigService.Logger.Debug(this.GlobalAppConfiguration.ToString());
			ConfigService.Logger.Debug("EffectiveAppConfiguration Settings");
			ConfigService.Logger.Debug(this.EffectiveAppConfiguration.ToString());
			ConfigService.Logger.Debug("UserAppConfiguration settings");
			ConfigService.Logger.Debug(this.UserAppConfiguration.ToString());
		}

		#endregion

		#region IConfigService implementation

		public void GetConfigValue(ChocolateyGuiConfiguration configuration)
		{
			var chosenAppConfiguration = GetChosenAppConfiguration(configuration.Global);
			var configProperty = ConfigService.GetProperty(configuration.ConfigCommand.Name, false);
			var configValue = (String)configProperty.GetValue(chosenAppConfiguration);

			ConfigService.Logger.Information("{0}".FormatWith(configValue ?? String.Empty));
		}

		public AppConfiguration GetEffectiveConfiguration()
		{
			if (this.EffectiveAppConfiguration == null)
			{
				ConfigService.Logger.Information("Calling SettingEffectiveConfiguration from OSS");
				SetEffectiveConfiguration();
			}

			return this.EffectiveAppConfiguration;
		}

		public IEnumerable<ChocolateyGuiFeature> GetFeatures(Boolean global)
		{
			return GetFeatures(global, useResourceKeys: false);
		}

		public IEnumerable<ChocolateyGuiFeature> GetFeatures(Boolean global, Boolean useResourceKeys)
		{
			var features = new List<ChocolateyGuiFeature>();

			var properties = typeof(AppConfiguration).GetProperties();
			foreach (var property in properties)
			{
				var propertyName = property.Name;

				var featureAttributes = property.GetCustomAttributes(typeof(FeatureAttribute), true);
				if (property.Name != "Id" && featureAttributes.Length > 0)
				{
					var propertyValue = (Boolean?)property.GetValue(global ? this.GlobalAppConfiguration : this.EffectiveAppConfiguration);

					features.Add(new ChocolateyGuiFeature { Description = GetDescriptionFromProperty(property, useResourceKeys), Enabled = propertyValue ?? false, Title = propertyName });
				}
			}

			return features.OrderBy(f => f.Title);
		}

		public AppConfiguration GetGlobalConfiguration()
		{
			return this.GlobalAppConfiguration;
		}

		public IEnumerable<ChocolateyGuiSetting> GetSettings(Boolean global)
		{
			return GetSettings(global, useResourceKeys: false);
		}

		public IEnumerable<ChocolateyGuiSetting> GetSettings(Boolean global, Boolean useResourceKeys)
		{
			var settings = new List<ChocolateyGuiSetting>();

			var properties = typeof(AppConfiguration).GetProperties();
			foreach (var property in properties)
			{
				var propertyName = property.Name;

				var configAttributes = property.GetCustomAttributes(typeof(ConfigAttribute), true);
				if (property.Name != "Id" && configAttributes.Length > 0)
				{
					var propertyValue = (String)property.GetValue(global ? this.GlobalAppConfiguration : this.EffectiveAppConfiguration);

					settings.Add(new ChocolateyGuiSetting { Description = GetDescriptionFromProperty(property, useResourceKeys), Value = propertyValue, Key = propertyName });
				}
			}

			return settings.OrderBy(s => s.Key);
		}

		public void ListFeatures(ChocolateyGuiConfiguration configuration)
		{
			foreach (var feature in GetFeatures(configuration.Global))
			{
				if (configuration.RegularOutput)
				{
					ConfigService.Logger.Information("{0} {1} - {2}".FormatWith(feature.Enabled ? "[x]" : "[ ]", feature.Title, feature.Description));
				}
				else
				{
					ConfigService.Logger.Information("{0}|{1}|{2}".FormatWith(feature.Title, ConfigService.L(!feature.Enabled ? nameof(Resources.FeatureCommand_Disabled) : nameof(Resources.FeatureCommand_Enabled)), feature.Description));
				}
			}
		}

		public void ListSettings(ChocolateyGuiConfiguration configuration)
		{
			foreach (var setting in GetSettings(configuration.Global))
			{
				if (configuration.RegularOutput)
				{
					ConfigService.Logger.Information("{0} = {1} - {2}".FormatWith(setting.Key, setting.Value, setting.Description));
				}
				else
				{
					ConfigService.Logger.Information("{0}|{1}|{2}".FormatWith(setting.Key, setting.Value, setting.Description));
				}
			}
		}

		public void SetConfigValue(String key, String value)
		{
			var configuration = new ChocolateyGuiConfiguration
			                    {
				                    CommandName = "config",
				                    ConfigCommand =
				                    {
					                    Name = key,
					                    ConfigValue = value
				                    }
			                    };

			SetConfigValue(configuration);
		}

		public void SetConfigValue(ChocolateyGuiConfiguration configuration)
		{
			if (configuration.Global && !Hacks.IsElevated)
			{
				// This is not allowed!
				ConfigService.Logger.Error(ConfigService.L(nameof(Resources.ConfigCommand_ElevatedPermissionsError)));
				return;
			}

			var chosenAppConfiguration = GetChosenAppConfiguration(configuration.Global);
			var configProperty = ConfigService.GetProperty(configuration.ConfigCommand.Name, false);
			configProperty.SetValue(chosenAppConfiguration, configuration.ConfigCommand.ConfigValue);
			UpdateSettings(chosenAppConfiguration, configuration.Global);

			// since the update happened successfully, update the effective configuration
			configProperty.SetValue(this.EffectiveAppConfiguration, configuration.ConfigCommand.ConfigValue);

			ConfigService.Logger.Warning(ConfigService.L(nameof(Resources.ConfigCommand_Updated), configuration.ConfigCommand.Name, configuration.ConfigCommand.ConfigValue));
		}

		public void ToggleFeature(ChocolateyGuiConfiguration configuration, Boolean requiredValue)
		{
			if (configuration.Global && !Hacks.IsElevated)
			{
				// This is not allowed!
				ConfigService.Logger.Error(ConfigService.L(nameof(Resources.FeatureCommand_ElevatedPermissionsError)));
				return;
			}

			var chosenAppConfiguration = GetChosenAppConfiguration(configuration.Global);
			var featureProperty = ConfigService.GetProperty(configuration.FeatureCommand.Name, true);
			var featureValue = ConfigService.GetPropertyValue<Boolean?>(chosenAppConfiguration, featureProperty);

			if (featureValue == null || (featureValue.HasValue && requiredValue != featureValue))
			{
				featureProperty.SetValue(chosenAppConfiguration, requiredValue);
				UpdateSettings(chosenAppConfiguration, configuration.Global);

				// since the update happened successfully, update the effective configuration
				featureProperty.SetValue(this.EffectiveAppConfiguration, requiredValue);

				ConfigService.Logger.Warning(ConfigService.L(
					                             requiredValue
						                             ? nameof(Resources.FeatureCommand_EnabledWarning)
						                             : nameof(Resources.FeatureCommand_DisabledWarning),
					                             configuration.FeatureCommand.Name));
			}
			else
			{
				ConfigService.Logger.Warning(ConfigService.L(nameof(Resources.FeatureCommand_NoChangeMessage)));
			}
		}

		public void UnsetConfigValue(ChocolateyGuiConfiguration configuration)
		{
			if (configuration.Global && !Hacks.IsElevated)
			{
				// This is not allowed!
				ConfigService.Logger.Error(ConfigService.L(nameof(Resources.ConfigCommand_Unset_ElevatedPermissionsError)));
				return;
			}

			var chosenAppConfiguration = GetChosenAppConfiguration(configuration.Global);
			var configProperty = ConfigService.GetProperty(configuration.ConfigCommand.Name, false);
			configProperty.SetValue(chosenAppConfiguration, String.Empty);
			UpdateSettings(chosenAppConfiguration, configuration.Global);

			// since the update happened successfully, update the effective configuration
			configProperty.SetValue(this.EffectiveAppConfiguration, String.Empty);

			ConfigService.Logger.Warning(ConfigService.L(nameof(Resources.ConfigCommand_Unset), configuration.ConfigCommand.Name));
		}

		public void UpdateSettings(AppConfiguration settings, Boolean global)
		{
			if (global && this.GlobalCollection == null)
			{
				// This is very much an edge case, and we shouldn't ever get to here, but it does need to be handled
				ConfigService.Logger.Warning("An attempt has been made to save a configuration change globally, when the global configuration database hasn't been created.");
				ConfigService.Logger.Warning("No action will be taken, please check with your System Administrator.");
				return;
			}

			var settingsCollection = global ? this.GlobalCollection : this.UserCollection;

			if (settingsCollection.Exists(Query.EQ("_id", "v0.18.0")))
			{
				settingsCollection.Update("v0.18.0", settings);
			}
			else
			{
				try
				{
					settingsCollection.Insert(settings);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}

			SettingsChanged?.Invoke(GetEffectiveConfiguration(), EventArgs.Empty);
		}

		#endregion

		#region Private implementation

		private AppConfiguration GetChosenAppConfiguration(Boolean global)
		{
			return global ? this.GlobalAppConfiguration : this.UserAppConfiguration;
		}

		private String GetDescriptionFromProperty(PropertyInfo property, Boolean useResourceKey = false)
		{
			var attributes = property.GetCustomAttributes(typeof(LocalizedDescriptionAttribute), true);
			var attribute = attributes.Length > 0 ? (LocalizedDescriptionAttribute)attributes[0] : null;
			return useResourceKey ? attribute?.Key : attribute?.Description;
		}

		private static PropertyInfo GetProperty(String propertyName, Boolean isFeature)
		{
			var featureProperty = typeof(AppConfiguration).GetProperties().FirstOrDefault(f => f.Name.ToLowerInvariant() == propertyName.ToLowerInvariant());

			if (featureProperty == null)
			{
				var key = isFeature
					? nameof(Resources.FeatureCommand_FeatureNotFoundError)
					: nameof(Resources.ConfigCommand_ConfigNotFoundError);

				ConfigService.Logger.Error(ConfigService.L(key, propertyName));
				Environment.Exit(-1);
			}

			return featureProperty;
		}

		private static T GetPropertyValue<T>(AppConfiguration configuration, PropertyInfo property)
		{
			var propertyValue = (T)property.GetValue(configuration);
			return propertyValue;
		}

		private static String L(String key)
		{
			return ConfigService.TranslationSource[key];
		}

		private static String L(String key, params Object[] parameters)
		{
			return ConfigService.TranslationSource[key, parameters];
		}

		#endregion
	}
}