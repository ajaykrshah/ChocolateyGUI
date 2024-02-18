// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Commands
{
	using System;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Utilities;
	using Serilog;

	public abstract class BaseCommand
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<BaseCommand>();
		private static readonly TranslationSource TranslationSource = TranslationSource.Instance;

		#endregion

		#region Protected interface

		protected static String L(String key)
		{
			return BaseCommand.TranslationSource[key];
		}

		protected static String L(String key, params Object[] parameters)
		{
			return BaseCommand.TranslationSource[key, parameters];
		}

		protected static void PrintExitCodeInformation()
		{
			BaseCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_ExitCodesTitle)));
			BaseCommand.Logger.Information(String.Empty);
			BaseCommand.Logger.Information(BaseCommand.L(nameof(Resources.Command_ExitCodesText)));
			BaseCommand.Logger.Information(String.Empty);
			BaseCommand.Logger.Warning(BaseCommand.L(nameof(Resources.Command_OptionsAndSwitches)));
		}

		#endregion
	}
}