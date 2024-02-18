// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using AutoMapper;
	using chocolatey;
	using chocolatey.infrastructure.app;
	using chocolatey.infrastructure.app.configuration;
	using chocolatey.infrastructure.app.domain;
	using chocolatey.infrastructure.app.nuget;
	using chocolatey.infrastructure.app.services;
	using chocolatey.infrastructure.filesystem;
	using chocolatey.infrastructure.results;
	using chocolatey.infrastructure.services;
	using ChocolateyGui.Common.Enums;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Utilities;
	using Microsoft.VisualStudio.Threading;
	using NuGet.Protocol.Core.Types;
	using NuGet.Versioning;
	using Serilog;
	using ChocolateySource = ChocolateyGui.Common.Models.ChocolateySource;
	using ConfigCommandType = chocolatey.infrastructure.app.domain.ConfigCommandType;
	using FeatureCommandType = chocolatey.infrastructure.app.domain.FeatureCommandType;

	public class ChocolateyService : IChocolateyService
	{
		#region Declarations

		private static readonly AsyncReaderWriterLock Lock = new AsyncReaderWriterLock();
		private static readonly ILogger Logger = Log.ForContext<ChocolateyService>();
		private readonly GetChocolatey _choco;
		private readonly IConfigService _configService;
		private readonly IChocolateyConfigSettingsService _configSettingsService;
		private readonly IFileSystem _fileSystem;
		private readonly String _localAppDataPath = String.Empty;
		private readonly IMapper _mapper;
		private readonly IProgressService _progressService;
		private readonly IXmlService _xmlService;

		#endregion

		#region Constructors

		public ChocolateyService(IMapper mapper, IProgressService progressService, IChocolateyConfigSettingsService configSettingsService, IXmlService xmlService, IFileSystem fileSystem, IConfigService configService)
		{
			_mapper = mapper;
			_progressService = progressService;
			_configSettingsService = configSettingsService;
			_xmlService = xmlService;
			_fileSystem = fileSystem;
			_configService = configService;
			_choco = Lets.GetChocolatey(initializeLogging: false).SetCustomLogging(new SerilogLogger(ChocolateyService.Logger, _progressService), logExistingMessages: false, addToExistingLoggers: true);

			_localAppDataPath = _fileSystem.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify), "Chocolatey GUI");
		}

		#endregion

		#region IChocolateyService implementation

		public async Task AddSource(ChocolateySource source)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				_choco.Set(
					config =>
					{
						config.CommandName = "source";
						config.SourceCommand.Command = SourceCommandType.Add;
						config.SourceCommand.Name = source.Id;
						config.Sources = source.Value;
						config.SourceCommand.Username = source.UserName;
						config.SourceCommand.Password = source.Password;
						config.SourceCommand.Certificate = source.Certificate;
						config.SourceCommand.CertificatePassword = source.CertificatePassword;
						config.SourceCommand.Priority = source.Priority;
						config.SourceCommand.BypassProxy = source.BypassProxy;
						config.SourceCommand.AllowSelfService = source.AllowSelfService;
						config.SourceCommand.VisibleToAdminsOnly = source.VisibleToAdminsOnly;
					});

				await _choco.RunAsync();

				if (source.Disabled)
				{
					await DisableSource(source.Id);
				}
				else
				{
					await EnableSource(source.Id);
				}
			}
		}

		public async Task DisableSource(String id)
		{
			_choco.Set(
				config =>
				{
					config.CommandName = "source";
					config.SourceCommand.Command = SourceCommandType.Disable;
					config.SourceCommand.Name = id;
				});

			await _choco.RunAsync();
		}

		public async Task EnableSource(String id)
		{
			_choco.Set(
				config =>
				{
					config.CommandName = "source";
					config.SourceCommand.Command = SourceCommandType.Enable;
					config.SourceCommand.Name = id;
				});

			await _choco.RunAsync();
		}

		public async Task ExportPackages(String exportFilePath, Boolean includeVersionNumbers)
		{
			_choco.Set(
				config =>
				{
					config.CommandName = "export";
					config.ExportCommand.OutputFilePath = exportFilePath;
					config.ExportCommand.IncludeVersionNumbers = includeVersionNumbers;
				});

			await _choco.RunAsync();
		}

		public async Task<List<NuGetVersion>> GetAvailableVersionsForPackageIdAsync(String id, Int32 page, Int32 pageSize, Boolean includePreRelease)
		{
			_choco.Set(
				config =>
				{
					config.CommandName = "list";
					config.Input = id;
					config.ListCommand.Exact = true;
					config.ListCommand.Page = page;
					config.ListCommand.PageSize = pageSize;
					config.Prerelease = includePreRelease;
					config.AllVersions = true;
					config.QuietOutput = true;
					config.RegularOutput = false;
#if !DEBUG
                                config.Verbose = false;
#endif // DEBUG
				});
			var chocoConfig = _choco.GetConfiguration();
			var packages = await _choco.ListAsync<PackageResult>();
			return packages.Select(p => NuGetVersion.Parse(p.Version)).OrderByDescending(p => p.Version).ToList();
		}

		public async Task<Package> GetByVersionAndIdAsync(String id, String version, Boolean isPrerelease)
		{
			_choco.Set(
				config =>
				{
					config.CommandName = "list";
					config.Input = id;
					config.ListCommand.Exact = true;
					config.Version = version;
					config.QuietOutput = true;
					config.RegularOutput = false;
#if !DEBUG
                    config.Verbose = false;
#endif // DEBUG
				});
			var chocoConfig = _choco.GetConfiguration();

			var nugetLogger = _choco.Container().GetInstance<NuGet.Common.ILogger>();
			var origVer = chocoConfig.Version;
			chocoConfig.Version = version;
			var nugetPackage = await Task.Run(() => (NugetList.GetPackages(chocoConfig, nugetLogger, _fileSystem) as IQueryable<IPackageSearchMetadata>).FirstOrDefault());
			chocoConfig.Version = origVer;
			if (nugetPackage == null)
			{
				throw new Exception("No Package Found");
			}

			return ChocolateyService.GetMappedPackage(_choco, new PackageResult(nugetPackage, null, chocoConfig.Sources), _mapper);
		}

		public async Task<ChocolateyFeature[]> GetFeatures()
		{
			var config = await GetConfigFile();
			var features = config.Features.Select(_mapper.Map<ChocolateyFeature>);
			return features.OrderBy(f => f.Name).ToArray();
		}

		public async Task<IEnumerable<Package>> GetInstalledPackages()
		{
			_choco.Set(
				config => { config.CommandName = CommandNameType.List.ToString(); });

			var chocoConfig = _choco.GetConfiguration();

			// Not entirely sure what is going on here.  When there are no sources defined, for example, when they
			// are all disabled, the ListAsync command isn't returning any packages installed locally.  When in this
			// situation, use the nugetService directly to get the list of installed packages.
			if (chocoConfig.Sources != null)
			{
				var packages = await _choco.ListAsync<PackageResult>();
				return packages
				       .Select(package => ChocolateyService.GetMappedPackage(_choco, package, _mapper, true))
				       .ToArray();
			}
			else
			{
				var nugetService = _choco.Container().GetInstance<INugetService>();
				var packages = await Task.Run(() => nugetService.List(chocoConfig));
				return packages
				       .Select(package => ChocolateyService.GetMappedPackage(_choco, package, _mapper, true))
				       .ToArray();
			}
		}

		public async Task<IReadOnlyList<OutdatedPackage>> GetOutdatedPackages(Boolean includePrerelease = false, String packageName = null, Boolean forceCheckForOutdatedPackages = false)
		{
			var preventAutomatedOutdatedPackagesCheck = _configService.GetEffectiveConfiguration().PreventAutomatedOutdatedPackagesCheck ?? false;

			if (preventAutomatedOutdatedPackagesCheck && !forceCheckForOutdatedPackages)
			{
				return new List<OutdatedPackage>();
			}

			var outdatedPackagesFile = _fileSystem.CombinePaths(_localAppDataPath, "outdatedPackages.xml");

			var outdatedPackagesCacheDurationInMinutesSetting = _configService.GetEffectiveConfiguration().OutdatedPackagesCacheDurationInMinutes;
			var outdatedPackagesCacheDurationInMinutes = 0;
			if (!String.IsNullOrWhiteSpace(outdatedPackagesCacheDurationInMinutesSetting))
			{
				Int32.TryParse(outdatedPackagesCacheDurationInMinutesSetting, out outdatedPackagesCacheDurationInMinutes);
			}

			if (_fileSystem.FileExists(outdatedPackagesFile) && (DateTime.Now - _fileSystem.GetFileModifiedDate(outdatedPackagesFile)).TotalMinutes < outdatedPackagesCacheDurationInMinutes)
			{
				return _xmlService.Deserialize<List<OutdatedPackage>>(outdatedPackagesFile);
			}

			var choco = Lets.GetChocolatey(initializeLogging: false);
			choco.Set(
				config =>
				{
					config.CommandName = "outdated";
					config.PackageNames = packageName ?? ApplicationParameters.AllPackages;
					config.UpgradeCommand.NotifyOnlyAvailableUpgrades = true;
					config.RegularOutput = false;
					config.QuietOutput = true;
					config.Prerelease = false;

					if (forceCheckForOutdatedPackages)
					{
						config.SetCacheExpirationInMinutes(0);
					}
				});
			var chocoConfig = choco.GetConfiguration();

			// If there are no Sources configured, for example, if they are all disabled, then figuring out
			// which packages are outdated can't be completed.
			if (chocoConfig.Sources != null)
			{
				var nugetService = choco.Container().GetInstance<INugetService>();
				var packages = await Task.Run(() => nugetService.UpgradeDryRun(chocoConfig, null));
				var results = packages
				              .Where(p => !p.Value.Inconclusive)
				              .Select(p => new OutdatedPackage
				                           { Id = p.Value.Name, VersionString = p.Value.Version })
				              .ToArray();

				try
				{
					// The XmlService won't create a new file, if the file already exists with the same hash,
					// i.e. the list of outdated packages hasn't changed. Currently, we check for new outdated
					// packages, when the serialized file has become old/stale, so we NEED the file to be re-written
					// when this check is done, so that it isn't always doing the check. Therefore, when we are
					// getting ready to serialize the list of outdated packages, if the file already exists, delete it.
					if (_fileSystem.FileExists(outdatedPackagesFile))
					{
						_fileSystem.DeleteFile(outdatedPackagesFile);
					}

					_xmlService.Serialize(results, outdatedPackagesFile);
				}
				catch (Exception ex)
				{
					ChocolateyService.Logger.Error(ex, ChocolateyService.L(nameof(Resources.Application_OutdatedPackagesError)));
				}

				return results.ToList();
			}

			return new List<OutdatedPackage>();
		}

		public async Task<ChocolateySetting[]> GetSettings()
		{
			var config = await GetConfigFile();
			var settings = config.ConfigSettings.Select(_mapper.Map<ChocolateySetting>);
			return settings.OrderBy(s => s.Key).ToArray();
		}

		public async Task<ChocolateySource[]> GetSources()
		{
			// We only want to provide the sources returned by calling choco.exe, which will exclude those
			// as required, based on configuration.  However, in order to be able to set all properties of the source
			// we need to read all information from the config file, i.e. the username and password
			var config = await GetConfigFile();
			var allSources = config.Sources.Select(_mapper.Map<ChocolateySource>).ToArray();

			var filteredSourceIds = _configSettingsService.ListSources(_choco.GetConfiguration()).Select(s => s.Id).ToArray();

			var mappedSources = allSources.Where(s => filteredSourceIds.Contains(s.Id)).ToArray();
			return mappedSources;
		}

		public async Task<PackageOperationResult> InstallPackage(
			String id,
			String version = null,
			Uri source = null,
			Boolean force = false,
			AdvancedInstall advancedInstallOptions = null)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				var logger = new SerilogLogger(ChocolateyService.Logger, _progressService);
				var choco = Lets.GetChocolatey(initializeLogging: false).SetCustomLogging(logger, logExistingMessages: false, addToExistingLoggers: true);
				choco.Set(
					config =>
					{
						config.CommandName = CommandNameType.Install.ToString();
						config.PackageNames = id;
						config.Features.UsePackageExitCodes = false;

						if (version != null)
						{
							config.Version = version;
						}

						if (source != null)
						{
							config.Sources = source.ToString();
						}

						if (force)
						{
							config.Force = true;
						}

						if (advancedInstallOptions != null)
						{
							config.InstallArguments = advancedInstallOptions.InstallArguments;
							config.PackageParameters = advancedInstallOptions.PackageParameters;
							config.CommandExecutionTimeoutSeconds = advancedInstallOptions.ExecutionTimeoutInSeconds;

							if (!String.IsNullOrEmpty(advancedInstallOptions.LogFile))
							{
								config.AdditionalLogFileLocation = advancedInstallOptions.LogFile;
							}

							config.Prerelease = advancedInstallOptions.PreRelease;
							config.ForceX86 = advancedInstallOptions.Forcex86;
							config.OverrideArguments = advancedInstallOptions.OverrideArguments;
							config.NotSilent = advancedInstallOptions.NotSilent;
							config.ApplyInstallArgumentsToDependencies = advancedInstallOptions.ApplyInstallArgumentsToDependencies;
							config.ApplyPackageParametersToDependencies = advancedInstallOptions.ApplyPackageParametersToDependencies;
							config.AllowDowngrade = advancedInstallOptions.AllowDowngrade;
							config.IgnoreDependencies = advancedInstallOptions.IgnoreDependencies;
							config.ForceDependencies = advancedInstallOptions.ForceDependencies;
							config.SkipPackageInstallProvider = advancedInstallOptions.SkipPowerShell;
							config.Features.ChecksumFiles = !advancedInstallOptions.IgnoreChecksums;
							config.Features.AllowEmptyChecksums = advancedInstallOptions.AllowEmptyChecksums;
							config.Features.AllowEmptyChecksumsSecure = advancedInstallOptions.AllowEmptyChecksumsSecure;

							if (advancedInstallOptions.RequireChecksums)
							{
								config.Features.AllowEmptyChecksums = false;
								config.Features.AllowEmptyChecksumsSecure = false;
							}

							if (!String.IsNullOrEmpty(advancedInstallOptions.CacheLocation))
							{
								config.CacheLocation = advancedInstallOptions.CacheLocation;
							}

							config.DownloadChecksum = advancedInstallOptions.DownloadChecksum;
							config.DownloadChecksum64 = advancedInstallOptions.DownloadChecksum64bit;
							config.DownloadChecksumType = advancedInstallOptions.DownloadChecksumType;
							config.DownloadChecksumType64 = advancedInstallOptions.DownloadChecksumType64bit;

							if (advancedInstallOptions.IgnoreHttpCache)
							{
								config.SetCacheExpirationInMinutes(0);
							}
						}
					});

				Action<LogMessage> grabErrors;
				var errors = ChocolateyService.GetErrors(out grabErrors);

				using (logger.Intercept(grabErrors))
				{
					await choco.RunAsync();

					if (Environment.ExitCode != 0)
					{
						Environment.ExitCode = 0;
						return new PackageOperationResult { Successful = false, Messages = errors.ToArray() };
					}

					return PackageOperationResult.SuccessfulCached;
				}
			}
		}

		public Task<Boolean> IsElevated()
		{
			return Task.FromResult(Elevation.Instance.IsElevated);
		}

		public async Task<PackageOperationResult> PackPackage(GenericCommandType commandType, String workingDirectoryPath = null)
		{
			// Configure Chocolatey based on the command type
			_choco.Set(config => { config.CommandName = commandType.ToString().ToLower(); });
			return await PerformGenericTaskAsync(_choco, workingDirectoryPath);
		}

		public async Task<PackageOperationResult> PinPackage(String id, String version)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				_choco.Set(
					config =>
					{
						config.CommandName = "pin";
						config.PinCommand.Command = PinCommandType.Add;
						config.PinCommand.Name = id;
						config.Version = version;
						config.Sources = ApplicationParameters.PackagesLocation;
					});

				try
				{
					await _choco.RunAsync();
				}
				catch (Exception ex)
				{
					return new PackageOperationResult { Successful = false, Exception = ex };
				}

				return PackageOperationResult.SuccessfulCached;
			}
		}

		public async Task<PackageOperationResult> PublishPackage(GenericCommandType commandType, String workingDirectoryPath = null)
		{
			// Configure Chocolatey based on the command type
			_choco.Set(config =>
			{
				config.CommandName = commandType.ToString().ToLower();
				config.PushCommand.DefaultSource = ApplicationParameters.ChocolateyCommunityFeedPushSource;
			});
			return await PerformGenericTaskAsync(_choco, workingDirectoryPath);
		}

		public async Task<Boolean> RemoveSource(String id)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				var chocoConfig = await GetConfigFile();
				var sources = chocoConfig.Sources.Select(_mapper.Map<ChocolateySource>).ToList();

				if (sources.All(source => source.Id != id))
				{
					return false;
				}

				_choco.Set(
					config =>
					{
						config.CommandName = "source";
						config.SourceCommand.Command = SourceCommandType.Remove;
						config.SourceCommand.Name = id;
					});

				await _choco.RunAsync();
				return true;
			}
		}

		public async Task<PackageResults> Search(String query, PackageSearchOptions options)
		{
			_choco.Set(
				config =>
				{
					config.CommandName = "search";
					config.Input = query;
					config.AllVersions = options.IncludeAllVersions;
					config.ListCommand.Page = options.CurrentPage;
					config.ListCommand.PageSize = options.PageSize;
					config.Prerelease = options.IncludePrerelease;
					config.ListCommand.LocalOnly = false;
					if (String.IsNullOrWhiteSpace(query) || !String.IsNullOrWhiteSpace(options.SortColumn))
					{
						config.ListCommand.OrderByPopularity = String.IsNullOrWhiteSpace(options.SortColumn)
						                                       || options.SortColumn == "DownloadCount";
					}

					config.ListCommand.Exact = options.MatchQuery;
					if (!String.IsNullOrWhiteSpace(options.Source))
					{
						config.Sources = options.Source;
					}
#if !DEBUG
                        config.Verbose = false;
#endif // DEBUG
				});

			var packages =
				(await _choco.ListAsync<PackageResult>()).Select(
					pckge => ChocolateyService.GetMappedPackage(_choco, pckge, _mapper));

			return new PackageResults
			       {
				       Packages = packages.ToArray(),
				       TotalCount = await Task.Run(() => _choco.ListCount())
			       };
		}

		public async Task SetFeature(ChocolateyFeature feature)
		{
			if (feature == null)
			{
				return;
			}

			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				_choco.Set(
					config =>
					{
						config.CommandName = "feature";
						config.FeatureCommand.Command = feature.Enabled ? FeatureCommandType.Enable : FeatureCommandType.Disable;
						config.FeatureCommand.Name = feature.Name;
					});

				await _choco.RunAsync();
			}
		}

		public async Task SetSetting(ChocolateySetting setting)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				_choco.Set(
					config =>
					{
						config.CommandName = "config";
						config.ConfigCommand.Command = ConfigCommandType.Set;
						config.ConfigCommand.Name = setting.Key;
						config.ConfigCommand.ConfigValue = setting.Value;
					});

				await _choco.RunAsync();
			}
		}

		public async Task<PackageOperationResult> UninstallPackage(String id, String version, Boolean force = false)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				var logger = new SerilogLogger(ChocolateyService.Logger, _progressService);
				var choco = Lets.GetChocolatey(initializeLogging: false).SetCustomLogging(logger, logExistingMessages: false, addToExistingLoggers: true);
				choco.Set(
					config =>
					{
						config.CommandName = CommandNameType.Uninstall.ToString();
						config.PackageNames = id;
						config.Features.UsePackageExitCodes = false;

						if (version != null)
						{
							config.Version = version;
						}
					});

				return await RunCommand(choco, logger);
			}
		}

		public async Task<PackageOperationResult> UnpinPackage(String id, String version)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				_choco.Set(
					config =>
					{
						config.CommandName = "pin";
						config.PinCommand.Command = PinCommandType.Remove;
						config.PinCommand.Name = id;
						config.Version = version;
						config.Sources = ApplicationParameters.PackagesLocation;
					});
				try
				{
					await _choco.RunAsync();
				}
				catch (Exception ex)
				{
					return new PackageOperationResult { Successful = false, Exception = ex };
				}

				return PackageOperationResult.SuccessfulCached;
			}
		}

		public async Task<PackageOperationResult> UpdatePackage(String id, Uri source = null)
		{
			using (await ChocolateyService.Lock.WriteLockAsync())
			{
				var logger = new SerilogLogger(ChocolateyService.Logger, _progressService);
				var choco = Lets.GetChocolatey(initializeLogging: false).SetCustomLogging(logger, logExistingMessages: false, addToExistingLoggers: true);
				choco.Set(
					config =>
					{
						config.CommandName = CommandNameType.Upgrade.ToString();
						config.PackageNames = id;
						config.Features.UsePackageExitCodes = false;
					});

				return await RunCommand(choco, logger);
			}
		}

		public async Task UpdateSource(String id, ChocolateySource source)
		{
			// NOTE:  The strategy of first removing, and then re-adding the source
			// is due to the fact that there is no "edit source" command that can
			// be used.  This has the side effect of "having" to decrypt the password
			// for an authenticated source, otherwise, when re-adding the source,
			// the encrypted password, is re-encrypted, making it no longer work.
			if (id != source.Id)
			{
				await RemoveSource(id);
			}

			await AddSource(source);
		}

		#endregion

		#region Private implementation

		private async Task<ConfigFileSettings> GetConfigFile()
		{
			var xmlService = _choco.Container().GetInstance<IXmlService>();
			var config =
				await Task.Run(
					() => xmlService.Deserialize<ConfigFileSettings>(ApplicationParameters.GlobalConfigFileLocation));
			return config;
		}

		private static List<String> GetErrors(out Action<LogMessage> grabErrors)
		{
			var errors = new List<String>();
			grabErrors = m =>
			{
				switch (m.LogLevel)
				{
					case LogLevel.Warn:
					case LogLevel.Error:
					case LogLevel.Fatal:
						errors.Add(m.Message);
						break;
				}
			};
			return errors;
		}

		private static Package GetMappedPackage(GetChocolatey choco, PackageResult package, IMapper mapper, Boolean forceInstalled = false)
		{
			var mappedPackage = package == null ? null : mapper.Map<Package>(package.SearchMetadata);
			if (mappedPackage != null)
			{
				if (package.PackageMetadata != null)
				{
					mappedPackage.ReleaseNotes = package.PackageMetadata.ReleaseNotes;
					mappedPackage.Language = package.PackageMetadata.Language;
					mappedPackage.Copyright = package.PackageMetadata.Copyright;
				}

				var packageInfoService = choco.Container().GetInstance<IChocolateyPackageInformationService>();
				var packageInfo = packageInfoService.Get(package.PackageMetadata);
				mappedPackage.IsPinned = packageInfo.IsPinned;
				mappedPackage.IsInstalled = !String.IsNullOrWhiteSpace(package.InstallLocation) || forceInstalled;

				mappedPackage.IsPrerelease = mappedPackage.Version.IsPrerelease;
			}

			return mappedPackage;
		}

		private static String L(String key)
		{
			return TranslationSource.Instance[key];
		}

		private static String L(String key, params Object[] parameters)
		{
			return TranslationSource.Instance[key, parameters];
		}

		private async Task<PackageOperationResult> PerformGenericTaskAsync(GetChocolatey choco, String workingDirectoryPath = null)
		{
			var originalDirectory = Directory.GetCurrentDirectory();
			if (workingDirectoryPath != null)
			{
				Directory.SetCurrentDirectory(workingDirectoryPath);
			}

			try
			{
				Action<LogMessage> grabErrors;
				var errors = ChocolateyService.GetErrors(out grabErrors);
				var logger = new SerilogLogger(ChocolateyService.Logger, _progressService);

				using (logger.Intercept(grabErrors))
				{
					await choco.RunAsync();

					if (Environment.ExitCode != 0)
					{
						Environment.ExitCode = 0;
						return new PackageOperationResult { Successful = false, Messages = errors.ToArray() };
					}

					return PackageOperationResult.SuccessfulCached;
				}
			}
			catch (Exception ex)
			{
				return new PackageOperationResult { Successful = false, Messages = new[] { ex.Message } };
			}
			finally
			{
				Directory.SetCurrentDirectory(originalDirectory);
			}
		}

		private async Task<PackageOperationResult> RunCommand(GetChocolatey choco, SerilogLogger logger)
		{
			Action<LogMessage> grabErrors;
			var errors = ChocolateyService.GetErrors(out grabErrors);

			using (logger.Intercept(grabErrors))
			{
				try
				{
					await choco.RunAsync();
				}
				catch (Exception ex)
				{
					return new PackageOperationResult { Successful = false, Messages = errors.ToArray(), Exception = ex };
				}

				if (Environment.ExitCode != 0)
				{
					Environment.ExitCode = 0;
					return new PackageOperationResult { Successful = false, Messages = errors.ToArray() };
				}

				return PackageOperationResult.SuccessfulCached;
			}
		}

		#endregion

#pragma warning disable SA1401 // Fields must be private
#pragma warning restore SA1401 // Fields must be private
	}
}