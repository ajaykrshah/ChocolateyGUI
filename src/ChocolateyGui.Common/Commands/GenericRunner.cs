// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Autofac;
	using chocolatey;
	using ChocolateyGui.Common.Attributes;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Utilities;
	using Serilog;

	public sealed class GenericRunner
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<GenericRunner>();

		#endregion

		#region Public interface

		public void Run(ChocolateyGuiConfiguration configuration, IContainer container, Action<ICommand> parseArgs)
		{
			var command = FindCommand(configuration, container, parseArgs);

			if (configuration.HelpRequested || configuration.UnsuccessfulParsing)
			{
				Environment.Exit(configuration.UnsuccessfulParsing ? 1 : 0);
			}

			if (command != null)
			{
				GenericRunner.Logger.Debug("_ {0}:{1} - Normal Run Mode _".FormatWith("Chocolatey GUI", command.GetType().Name));
				command.Run(configuration);
			}
		}

		#endregion

		#region Private implementation

		private ICommand FindCommand(ChocolateyGuiConfiguration configuration, IContainer container, Action<ICommand> parseArgs)
		{
			var commands = container.Resolve<IEnumerable<ICommand>>();
			var command = commands.Where(c =>
			{
				var attributes = c.GetType().GetCustomAttributes(typeof(LocalizedCommandForAttribute), false);
				return attributes.Cast<LocalizedCommandForAttribute>().Any(attribute => attribute.CommandName.IsEqualTo(configuration.CommandName));
			}).FirstOrDefault();

			if (command == null)
			{
				if (!String.IsNullOrWhiteSpace(configuration.CommandName))
				{
					GenericRunner.Logger.Error(TranslationSource.Instance[nameof(Resources.Command_NotFoundError), configuration.CommandName, "chocolateyguicli"]);
					Environment.Exit(-1);
				}
			}
			else
			{
				if (parseArgs != null)
				{
					parseArgs.Invoke(command);
				}
			}

			return command;
		}

		#endregion
	}
}