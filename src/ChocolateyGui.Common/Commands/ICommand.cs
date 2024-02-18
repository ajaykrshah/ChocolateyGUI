// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Commands
{
	using System;
	using System.Collections.Generic;
	using chocolatey.infrastructure.commandline;
	using ChocolateyGui.Common.Models;

	/// <summary>
	///     Commands that can be configured and run
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		///     Configure the argument parser.
		/// </summary>
		/// <param name="optionSet">The option set.</param>
		/// <param name="configuration">The configuration.</param>
		void ConfigureArgumentParser(OptionSet optionSet, ChocolateyGuiConfiguration configuration);

		/// <summary>
		///     Handle the arguments that were not parsed by the argument parser and/or do additional parsing work
		/// </summary>
		/// <param name="unparsedArguments">The unparsed arguments.</param>
		/// <param name="configuration">The configuration.</param>
		void HandleAdditionalArgumentParsing(IList<String> unparsedArguments, ChocolateyGuiConfiguration configuration);

		void HandleValidation(ChocolateyGuiConfiguration configuration);

		/// <summary>
		///     The specific help message for a particular command.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		void HelpMessage(ChocolateyGuiConfiguration configuration);

		/// <summary>
		///     Runs the command.
		/// </summary>
		/// <param name="config">The configuration.</param>
		void Run(ChocolateyGuiConfiguration config);
	}
}