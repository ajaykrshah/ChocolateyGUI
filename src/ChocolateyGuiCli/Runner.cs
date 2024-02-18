// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGuiCli
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using Autofac;
	using chocolatey;
	using chocolatey.infrastructure.commandline;
	using ChocolateyGui.Common.Attributes;
	using ChocolateyGui.Common.Commands;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.Windows.Startup;
	using LiteDB;
	using Serilog;

	public class Runner
	{
		#region Declarations

		private static readonly OptionSet _optionSet = new OptionSet();

		#endregion

		#region Properties

		public static OptionSet OptionSet => Runner._optionSet;

		#endregion

		#region Public interface

		public static void Run(String[] args)
		{
			try
			{
				Bootstrapper.Configure();

				var commandName = String.Empty;
				IList<String> commandArgs = new List<String>();

				// shift the first arg off
				var count = 0;
				foreach (var arg in args)
				{
					if (count == 0)
					{
						count += 1;
						continue;
					}

					commandArgs.Add(arg);
				}

				// We need to initialize the current culture before we
				// use any translatable strings.
				Runner.SetUpPreferredLanguage(Bootstrapper.Container.Resolve<IConfigService>());

				var configuration = new ChocolateyGuiConfiguration();
				Runner.SetUpGlobalOptions(args, configuration, Bootstrapper.Container);
				Runner.SetEnvironmentOptions(configuration);

				if (configuration.RegularOutput)
				{
#if DEBUG
					Bootstrapper.Logger.Warning(" (DEBUG BUILD)".FormatWith("Chocolatey GUI", configuration.Information.DisplayVersion));
#else
					Bootstrapper.Logger.Warning("{0}".format_with(configuration.Information.DisplayVersion));
#endif

					if (args.Length == 0)
					{
						Bootstrapper.Logger.Information(Runner.L(nameof(Resources.Command_CommandsText), "chocolateyguicli"));
					}
				}

				var runner = new GenericRunner();
				runner.Run(configuration, Bootstrapper.Container, command =>
				{
					Runner.ParseArgumentsAndUpdateConfiguration(
						commandArgs,
						configuration,
						optionSet => command.ConfigureArgumentParser(optionSet, configuration),
						unparsedArgs => { command.HandleAdditionalArgumentParsing(unparsedArgs, configuration); },
						() =>
						{
							Bootstrapper.Logger.Debug("Performing validation checks...");
							command.HandleValidation(configuration);
						},
						() => command.HelpMessage(configuration));
				});
			}
			catch (Exception ex)
			{
				Bootstrapper.Logger.Error(ex.Message);
			}
			finally
			{
				Log.CloseAndFlush();

				if (Bootstrapper.Container != null)
				{
					if (Bootstrapper.Container.IsRegisteredWithName<LiteDatabase>(Bootstrapper.GlobalConfigurationDatabaseName))
					{
						var globalDatabase = Bootstrapper.Container.ResolveNamed<LiteDatabase>(Bootstrapper.GlobalConfigurationDatabaseName);
						globalDatabase.Dispose();
					}

					if (Bootstrapper.Container.IsRegisteredWithName<LiteDatabase>(Bootstrapper.UserConfigurationDatabaseName))
					{
						var userDatabase = Bootstrapper.Container.ResolveNamed<LiteDatabase>(Bootstrapper.UserConfigurationDatabaseName);
						userDatabase.Dispose();
					}

					Bootstrapper.Container.Dispose();
				}
			}
		}

		#endregion

		#region Private implementation

		private static String L(String key)
		{
			return TranslationSource.Instance[key];
		}

		private static String L(String key, params Object[] parameters)
		{
			return TranslationSource.Instance[key, parameters];
		}

		private static void ParseArgumentsAndUpdateConfiguration(
			ICollection<String> args,
			ChocolateyGuiConfiguration configuration,
			Action<OptionSet> setOptions,
			Action<IList<String>> afterParse,
			Action validateConfiguration,
			Action helpMessage)
		{
			IList<String> unparsedArguments = new List<String>();

			// add help only once
			if (Runner._optionSet.Count == 0)
			{
				Runner._optionSet
				      .Add(
					      "?|help|h",
					      Runner.L(nameof(Resources.Command_HelpOption)),
					      option => configuration.HelpRequested = option != null);
			}

			if (setOptions != null)
			{
				setOptions(Runner._optionSet);
			}

			try
			{
				unparsedArguments = Runner._optionSet.Parse(args);
			}
			catch (OptionException)
			{
				Runner.ShowHelp(Runner._optionSet, helpMessage);
				configuration.UnsuccessfulParsing = true;
			}

			// the command argument
			if (String.IsNullOrWhiteSpace(configuration.CommandName) &&
			    unparsedArguments.Contains(args.FirstOrDefault()))
			{
				var commandName = args.FirstOrDefault();
				if (!Regex.IsMatch(commandName, @"^[-\/+]"))
				{
					configuration.CommandName = commandName;
				}
				else if (commandName.IsEqualTo("-v") || commandName.IsEqualTo("--version"))
				{
					// skip help menu
				}
				else
				{
					configuration.HelpRequested = true;
					configuration.UnsuccessfulParsing = true;
				}
			}

			if (afterParse != null)
			{
				afterParse(unparsedArguments);
			}

			if (configuration.HelpRequested)
			{
				Runner.ShowHelp(Runner._optionSet, helpMessage);
			}
			else
			{
				if (validateConfiguration != null)
				{
					validateConfiguration();
				}
			}
		}

		private static void SetEnvironmentOptions(ChocolateyGuiConfiguration config)
		{
			var versionService = Bootstrapper.Container.Resolve<IVersionService>();
			config.Information.ChocolateyGuiVersion = versionService.Version;
			config.Information.ChocolateyGuiProductVersion = versionService.InformationalVersion;
			config.Information.DisplayVersion = versionService.DisplayVersion;
			config.Information.FullName = Assembly.GetExecutingAssembly().FullName;
		}

		private static void SetUpGlobalOptions(IList<String> args, ChocolateyGuiConfiguration configuration, IContainer container)
		{
			Runner.ParseArgumentsAndUpdateConfiguration(
				args,
				configuration,
				option_set =>
				{
					option_set
						.Add(
							"r|limitoutput|limit-output",
							Runner.L(nameof(Resources.Command_LimitOutputOption)),
							option => configuration.RegularOutput = option == null);
				},
				unparsedArgs =>
				{
					if (!String.IsNullOrWhiteSpace(configuration.CommandName))
					{
						// save help for next menu
						configuration.HelpRequested = false;
						configuration.UnsuccessfulParsing = false;
					}
				},
				() => { },
				() =>
				{
					var commandsLog = new StringBuilder();
					var commands = container.Resolve<IEnumerable<ICommand>>();
					foreach (var command in commands.OrEmpty())
					{
						var attributes = command.GetType().GetCustomAttributes(typeof(LocalizedCommandForAttribute), false).Cast<LocalizedCommandForAttribute>();
						foreach (var attribute in attributes.OrEmpty())
						{
							commandsLog.AppendFormat(" * {0} - {1}\n", attribute.CommandName, attribute.Description);
						}
					}

					Bootstrapper.Logger.Information(Runner.L(nameof(Resources.Command_CommandsListText), "chocolateyguicli"));
					Bootstrapper.Logger.Information(String.Empty);
					Bootstrapper.Logger.Warning(Runner.L(nameof(Resources.Command_CommandsTitle)));
					Bootstrapper.Logger.Information(String.Empty);
					Bootstrapper.Logger.Information("{0}".FormatWith(commandsLog.ToString()));
					Bootstrapper.Logger.Information(Runner.L(nameof(Resources.Command_CommandsText), "chocolateyguicli"));
					Bootstrapper.Logger.Information(String.Empty);
					Bootstrapper.Logger.Warning(Runner.L(nameof(Resources.Command_DefaultOptionsAndSwitches)));
				});
		}

		private static void SetUpPreferredLanguage(IConfigService configService)
		{
			var effectiveService = configService.GetEffectiveConfiguration();
			Internationalization.UpdateLanguage(effectiveService.UseLanguage);
		}

		private static void ShowHelp(OptionSet optionSet, Action helpMessage)
		{
			if (helpMessage != null)
			{
				helpMessage.Invoke();
			}

			optionSet.WriteOptionDescriptions(Console.Out);
		}

		#endregion
	}
}