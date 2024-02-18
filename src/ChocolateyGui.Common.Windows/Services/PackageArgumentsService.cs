// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Collections.Generic;
	using chocolatey;
	using chocolatey.infrastructure.adapters;
	using chocolatey.infrastructure.app.utility;
	using chocolatey.infrastructure.filesystem;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Providers;
	using ChocolateyGui.Common.Utilities;
	using Serilog;

	public class PackageArgumentsService : IPackageArgumentsService
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<PackageArgumentsService>();
		private readonly IChocolateyConfigurationProvider _chocolateyConfigurationProvider;
		private readonly IDialogService _dialogService;
		private readonly IEncryptionUtility _encryptionUtility;
		private readonly IFileSystem _fileSystem;

		#endregion

		#region Constructors

		public PackageArgumentsService(
			IEncryptionUtility encryptionUtility,
			IFileSystem fileSystem,
			IChocolateyConfigurationProvider chocolateyConfigurationProvider,
			IDialogService dialogService)
		{
			_encryptionUtility = encryptionUtility;
			_fileSystem = fileSystem;
			_chocolateyConfigurationProvider = chocolateyConfigurationProvider;
			_dialogService = dialogService;
		}

		#endregion

		#region IPackageArgumentsService implementation

		public IEnumerable<String> DecryptPackageArgumentsFile(String id, String version)
		{
			var argumentsPath = _fileSystem.CombinePaths(_chocolateyConfigurationProvider.ChocolateyInstall, ".chocolatey", "{0}.{1}".FormatWith(id, version));
			var argumentsFile = _fileSystem.CombinePaths(argumentsPath, ".arguments");

			var arguments = String.Empty;

			// Get the arguments decrypted in here and return them
			try
			{
				if (_fileSystem.FileExists(argumentsFile))
				{
					arguments = _fileSystem.ReadFile(argumentsFile);
				}
			}
			catch (Exception ex)
			{
				var message = PackageArgumentsService.L(nameof(Resources.Application_PackageArgumentsError), version, id);
				PackageArgumentsService.Logger.Error(ex, message);
			}

			if (String.IsNullOrEmpty(arguments))
			{
				PackageArgumentsService.Logger.Debug(
					String.Empty,
					PackageArgumentsService.L(nameof(Resources.PackageView_UnableToFindArgumentsFile), version, id));
				yield break;
			}

			// The following code is borrowed from the Chocolatey codebase, should
			// be extracted to a separate location in choco executable so we can re-use it.
			var packageArgumentsUnencrypted = arguments.Contains(" --") && arguments.ToStringSafe().Length > 4
				? arguments
				: _encryptionUtility.DecryptString(arguments);

			// Lets do a global check first to see if there are any sensitive arguments
			// before we filter out the values used later.
			var sensitiveArgs = ArgumentsUtility.SensitiveArgumentsProvided(packageArgumentsUnencrypted);

			var packageArgumentsSplit =
				packageArgumentsUnencrypted.Split(new[] { " --" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var packageArgument in packageArgumentsSplit.OrEmpty())
			{
				var isSensitiveArgument = sensitiveArgs && ArgumentsUtility.SensitiveArgumentsProvided(String.Concat("--", packageArgument));

				var packageArgumentSplit =
					packageArgument.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);

				var optionName = packageArgumentSplit[0].ToStringSafe();
				var optionValue = String.Empty;

				if (packageArgumentSplit.Length == 2 && isSensitiveArgument)
				{
					optionValue = PackageArgumentsService.L(nameof(Resources.PackageArgumentService_RedactedArgument));
				}
				else if (packageArgumentSplit.Length == 2)
				{
					optionValue = packageArgumentSplit[1].ToStringSafe().UnquoteSafe();
					if (optionValue.StartsWith("'"))
					{
						optionValue.UnquoteSafe();
					}
				}

				yield return "--{0}{1}".FormatWith(
					optionName,
					String.IsNullOrWhiteSpace(optionValue) ? String.Empty : "=" + optionValue);
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

		#endregion
	}
}