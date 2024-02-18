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

	[LocalizedCommandFor("feature", nameof(Resources.FeatureCommand_Description))]
	public class FeatureCommand : BaseCommand, ICommand
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<FeatureCommand>();
		private readonly IConfigService _configService;

		#endregion

		#region Constructors

		public FeatureCommand(IConfigService configService)
		{
			_configService = configService;
		}

		#endregion

		#region ICommand implementation

		public virtual void ConfigureArgumentParser(OptionSet optionSet, ChocolateyGuiConfiguration configuration)
		{
			optionSet
				.Add(
					"n=|name=",
					BaseCommand.L(nameof(Resources.FeatureCommand_NameOption)),
					option => configuration.FeatureCommand.Name = option.UnquoteSafe())
				.Add(
					"g|global",
					BaseCommand.L(nameof(Resources.GlobalOption)),
					option => configuration.Global = option != null);
		}

		public virtual void HandleAdditionalArgumentParsing(IList<String> unparsedArguments, ChocolateyGuiConfiguration configuration)
		{
			configuration.Input = String.Join(" ", unparsedArguments);

			if (unparsedArguments.Count > 1)
			{
				FeatureCommand.Logger.Error(BaseCommand.L(nameof(Resources.FeatureCommand_SingleFeatureError)));
				Environment.Exit(-1);
			}

			var command = FeatureCommandType.Unknown;
			var unparsedCommand = unparsedArguments.DefaultIfEmpty(String.Empty).FirstOrDefault();
			Enum.TryParse(unparsedCommand, true, out command);
			if (command == FeatureCommandType.Unknown)
			{
				if (!String.IsNullOrWhiteSpace(unparsedCommand))
				{
					FeatureCommand.Logger.Warning(BaseCommand.L(nameof(Resources.FeatureCommand_UnknownCommandError), unparsedCommand, "list"));
				}

				command = FeatureCommandType.List;
			}

			configuration.FeatureCommand.Command = command;
		}

		public virtual void HandleValidation(ChocolateyGuiConfiguration configuration)
		{
			if (configuration.FeatureCommand.Command != FeatureCommandType.List && String.IsNullOrWhiteSpace(configuration.FeatureCommand.Name))
			{
				FeatureCommand.Logger.Error(BaseCommand.L(nameof(Resources.FeatureCommand_MissingNameOptionError), configuration.FeatureCommand.Command.ToStringSafe(), "--name"));
				Environment.Exit(-1);
			}
		}

		public virtual void HelpMessage(ChocolateyGuiConfiguration configuration)
		{
			FeatureCommand.Logger.Warning(BaseCommand.L(nameof(Resources.FeatureCommand_Title)));
			FeatureCommand.Logger.Information(String.Empty);
			FeatureCommand.Logger.Information(BaseCommand.L(nameof(Resources.FeatureCommand_Help)));
			FeatureCommand.Logger.Information(String.Empty);
			FeatureCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_Usage)));
			FeatureCommand.Logger.Information(@"
    chocolateyguicli feature [list]|disable|enable [<options/switches>]
");
			FeatureCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_Examples)));
			FeatureCommand.Logger.Information(@"
    chocolateyguicli feature
    chocolateyguicli feature list
    chocolateyguicli feature disable -n=ShowConsoleOutput
    chocolateyguicli feature enable -n=ShowConsoleOutput
");

			BaseCommand.PrintExitCodeInformation();
		}

		public virtual void Run(ChocolateyGuiConfiguration config)
		{
			switch (config.FeatureCommand.Command)
			{
				case FeatureCommandType.List:
					_configService.ListFeatures(config);
					break;
				case FeatureCommandType.Disable:
					_configService.ToggleFeature(config, false);
					break;
				case FeatureCommandType.Enable:
					_configService.ToggleFeature(config, true);
					break;
			}
		}

		#endregion
	}
}