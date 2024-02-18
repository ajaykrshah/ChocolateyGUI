// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using chocolatey;
	using chocolatey.infrastructure.commandline;
	using ChocolateyGui.Common.Attributes;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using Serilog;

	[LocalizedCommandFor("config", nameof(Resources.ConfigCommand_Description))]
	public class ConfigCommand : BaseCommand, ICommand
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<ConfigCommand>();
		private readonly IConfigService _configService;

		#endregion

		#region Constructors

		public ConfigCommand(IConfigService configService)
		{
			_configService = configService;
		}

		#endregion

		#region ICommand implementation

		public virtual void ConfigureArgumentParser(OptionSet optionSet, ChocolateyGuiConfiguration configuration)
		{
			optionSet
				.Add(
					"name=",
					BaseCommand.L(nameof(Resources.ConfigCommand_NameOption)),
					option => configuration.ConfigCommand.Name = option.UnquoteSafe())
				.Add(
					"value=",
					BaseCommand.L(nameof(Resources.ConfigCommand_ValueOption)),
					option => configuration.ConfigCommand.ConfigValue = option.UnquoteSafe())
				.Add(
					"g|global",
					BaseCommand.L(nameof(Resources.GlobalOption)),
					option => configuration.Global = option != null);
		}

		public virtual void HandleAdditionalArgumentParsing(IList<String> unparsedArguments, ChocolateyGuiConfiguration configuration)
		{
			configuration.Input = String.Join(" ", unparsedArguments);

			var command = ConfigCommandType.Unknown;
			var unparsedCommand = unparsedArguments.DefaultIfEmpty(String.Empty).FirstOrDefault().ToStringSafe().Replace("-", String.Empty);
			Enum.TryParse(unparsedCommand, true, out command);
			if (command == ConfigCommandType.Unknown)
			{
				if (!String.IsNullOrWhiteSpace(unparsedCommand))
				{
					ConfigCommand.Logger.Warning(BaseCommand.L(nameof(Resources.ConfigCommand_UnknownCommandError), unparsedCommand, "list"));
				}

				command = ConfigCommandType.List;
			}

			configuration.ConfigCommand.Command = command;

			if ((configuration.ConfigCommand.Command == ConfigCommandType.List || !String.IsNullOrWhiteSpace(configuration.ConfigCommand.Name)) && unparsedArguments.Count > 1)
			{
				ConfigCommand.Logger.Error(BaseCommand.L(nameof(Resources.ConfigCommand_SingleConfigError)));
				Environment.Exit(-1);
			}

			if (String.IsNullOrWhiteSpace(configuration.ConfigCommand.Name) && unparsedArguments.Count >= 2)
			{
				configuration.ConfigCommand.Name = unparsedArguments[1];
			}

			if (String.IsNullOrWhiteSpace(configuration.ConfigCommand.ConfigValue) && unparsedArguments.Count >= 3)
			{
				configuration.ConfigCommand.ConfigValue = unparsedArguments[2];
			}
		}

		public virtual void HandleValidation(ChocolateyGuiConfiguration configuration)
		{
			if (configuration.ConfigCommand.Command != ConfigCommandType.List &&
			    String.IsNullOrWhiteSpace(configuration.ConfigCommand.Name))
			{
				ConfigCommand.Logger.Error(BaseCommand.L(nameof(Resources.ConfigCommand_MissingNameOptionError), configuration.ConfigCommand.Command.ToStringSafe(), "--name"));
				Environment.Exit(-1);
			}

			if (configuration.ConfigCommand.Command == ConfigCommandType.Set &&
			    String.IsNullOrWhiteSpace(configuration.ConfigCommand.ConfigValue))
			{
				ConfigCommand.Logger.Error(BaseCommand.L(nameof(Resources.ConfigCommand_MissingValueOptionError), configuration.ConfigCommand.Command.ToStringSafe(), "--value"));
				Environment.Exit(-1);
			}
		}

		public virtual void HelpMessage(ChocolateyGuiConfiguration configuration)
		{
			ConfigCommand.Logger.Warning(BaseCommand.L(nameof(Resources.ConfigCommand_Title)));
			ConfigCommand.Logger.Information(String.Empty);
			ConfigCommand.Logger.Information(BaseCommand.L(nameof(Resources.ConfigCommand_Help)));
			ConfigCommand.Logger.Information(String.Empty);
			ConfigCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_Usage)));
			ConfigCommand.Logger.Information(@"
    chocolateyguicli config [list]|get|set|unset [<options/switches>]
");

			ConfigCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_Examples)));
			ConfigCommand.Logger.Information(@"
    chocolateyguicli config
    chocolateyguicli config list
    chocolateyguicli config get outdatedPackagesCacheDurationInMinutes
    chocolateyguicli config get --name outdatedPackagesCacheDurationInMinutes
    chocolateyguicli config set outdatedPackagesCacheDurationInMinutes 60
    chocolateyguicli config set --name outdatedPackagesCacheDurationInMinutes --value 60
    chocolateyguicli config unset outdatedPackagesCacheDurationInMinutes
    chocolateyguicli config unset --name outdatedPackagesCacheDurationInMinutes
");
			BaseCommand.PrintExitCodeInformation();
		}

		public virtual void Run(ChocolateyGuiConfiguration config)
		{
			switch (config.ConfigCommand.Command)
			{
				case ConfigCommandType.List:
					_configService.ListSettings(config);
					break;
				case ConfigCommandType.Get:
					_configService.GetConfigValue(config);
					break;
				case ConfigCommandType.Set:
					_configService.SetConfigValue(config);
					break;
				case ConfigCommandType.Unset:
					_configService.UnsetConfigValue(config);
					break;
			}
		}

		#endregion
	}
}