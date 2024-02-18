// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using chocolatey.infrastructure.commandline;
	using ChocolateyGui.Common.Attributes;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using Serilog;

	[LocalizedCommandFor("purge", nameof(Resources.PurgeCommand_Description))]
	public class PurgeCommand : BaseCommand, ICommand
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<PurgeCommand>();
		private readonly IChocolateyGuiCacheService _chocolateyGuiCacheService;

		#endregion

		#region Constructors

		public PurgeCommand(IChocolateyGuiCacheService chocolateyGuiCacheService)
		{
			_chocolateyGuiCacheService = chocolateyGuiCacheService;
		}

		#endregion

		#region ICommand implementation

		public virtual void ConfigureArgumentParser(OptionSet optionSet, ChocolateyGuiConfiguration configuration)
		{
			// There are no additional options for this command currently
		}

		public virtual void HandleAdditionalArgumentParsing(IList<String> unparsedArguments, ChocolateyGuiConfiguration configuration)
		{
			configuration.Input = String.Join(" ", unparsedArguments);

			if (unparsedArguments.Count > 1)
			{
				Environment.Exit(-1);
			}

			var command = PurgeCommandType.Unknown;
			var unparsedCommand = unparsedArguments.DefaultIfEmpty(String.Empty).FirstOrDefault();
			Enum.TryParse(unparsedCommand, true, out command);
			configuration.PurgeCommand.Command = command;
		}

		public virtual void HandleValidation(ChocolateyGuiConfiguration configuration)
		{
			if (configuration.PurgeCommand.Command == PurgeCommandType.Unknown)
			{
				PurgeCommand.Logger.Error(BaseCommand.L(nameof(Resources.PurgeCommand_UnknownCommandError), configuration.Input, "icons", "outdated"));
				Environment.Exit(-1);
			}
		}

		public virtual void HelpMessage(ChocolateyGuiConfiguration configuration)
		{
			PurgeCommand.Logger.Warning(BaseCommand.L(nameof(Resources.PurgeCommand_Title)));
			PurgeCommand.Logger.Information(String.Empty);
			PurgeCommand.Logger.Information(BaseCommand.L(nameof(Resources.PurgeCommand_Help)));
			PurgeCommand.Logger.Information(String.Empty);
			PurgeCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_Usage)));
			PurgeCommand.Logger.Information(@"
    chocolateyguicli purge icons|outdated [<options/switches>]
");
			PurgeCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_Examples)));
			PurgeCommand.Logger.Information(@"
    chocolateyguicli purge icons
    chocolateyguicli purge outdated
");

			BaseCommand.PrintExitCodeInformation();
		}

		public virtual void Run(ChocolateyGuiConfiguration config)
		{
			switch (config.PurgeCommand.Command)
			{
				case PurgeCommandType.Icons:
					_chocolateyGuiCacheService.PurgeIcons();
					break;
				case PurgeCommandType.Outdated:
					_chocolateyGuiCacheService.PurgeOutdatedPackages();
					break;
			}
		}

		#endregion
	}
}