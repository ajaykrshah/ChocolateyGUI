// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Input;
	using System.Xml.Linq;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Enums;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Windows.Commands;
	using ChocolateyGui.Common.Windows.Services;
	using ChocolateyGui.Common.Windows.Utilities.Extensions;

	public sealed class PackagePublishViewModel : ViewModelScreen
	{
		#region Declarations

		private const String ChocolateyLicensedSourceId = "chocolatey.licensed";
		private const String resourceName = "chocolateyinstall.ps1";
		private readonly IChocolateyService _chocolateyService;

		//private ChocolateySource _draftPackage;
		private readonly IDialogService _dialogService;
		private readonly IEventAggregator _eventAggregator;
		private readonly IPersistenceService _persistenceService;
		private readonly IProgressService _progressService;
		private readonly IVersionService _versionService;
		private String _packageFileLocation;
		private String _packagePublishName;
		private String _packagePublishVersion;

		#endregion

		#region Constructors

		public PackagePublishViewModel(IEventAggregator eventAggregator, IVersionService versionService,
		                               IDialogService dialogService,
		                               IChocolateyService chocolateyService,
		                               IProgressService progressService,
		                               IPersistenceService persistenceService)
		{
			_eventAggregator = eventAggregator;
			_versionService = versionService;
			_dialogService = dialogService;
			_progressService = progressService;
			_chocolateyService = chocolateyService;
			_persistenceService = persistenceService;
			this.BrowsePackageFileLocationCommand = new RelayCommand(BrowsePackageFileLocation);
		}

		#endregion

		#region Properties

		public ICommand BrowsePackageFileLocationCommand { get; }
		public String ChocolateyGuiInformationalVersion => _versionService.InformationalVersion;
		public String ChocolateyGuiVersion => _versionService.Version;
		public Boolean IsPackagePushable => PackagePublishViewModel.ChocolateyLicensedSourceId != this.ChocolateyGuiVersion;

		public String PackageFileLocation
		{
			get => _packageFileLocation;
			set => this.SetPropertyValue(ref _packageFileLocation, value);
		}

		public String PackagePublishName
		{
			get => _packagePublishName;
			set => this.SetPropertyValue(ref _packagePublishName, value);
		}

		public String PackagePublishVersion
		{
			get => _packagePublishVersion;
			set => this.SetPropertyValue(ref _packagePublishVersion, value);
		}

		#endregion

		#region Public interface

		public void Back()
		{
			_eventAggregator.PublishOnUIThread(new PackagePublishGoBackMessage());
		}

		public void ClearPublishedPackageProperties()
		{
			this.PackagePublishName = null;
			this.PackagePublishVersion = null;
			this.PackageFileLocation = null;
		}

		public async Task PublishPackage()
		{
			var errorMessages = new List<String>();

			if (String.IsNullOrWhiteSpace(this.PackagePublishName))
			{
				errorMessages.Add(L(nameof(Resources.PackagePublishViewModel_PackageMissingName)));
			}

			if (String.IsNullOrWhiteSpace(this.PackagePublishVersion))
			{
				errorMessages.Add(L(nameof(Resources.PackagePublishViewModel_PackageMissingVersion)));
			}

			if (String.IsNullOrWhiteSpace(this.PackageFileLocation))
			{
				errorMessages.Add(L(nameof(Resources.PackagePublishViewModel_PackageMissingFile)));
			}

			if (errorMessages.Any())
			{
				var errorMessage = String.Join("\n", errorMessages);
				await _dialogService.ShowMessageAsync(L(nameof(Resources.PackagePublishViewModel_PublishingPackage)), errorMessage);
				return;
			}

			await _progressService.StartLoading(L(nameof(Resources.PackagePublishViewModel_PublishingPackage)));

			try
			{
				var tempFolderPackagePath = Path.Combine(Path.GetTempPath(), this.PackagePublishName);

				if (Directory.Exists(tempFolderPackagePath))
				{
					Directory.Delete(tempFolderPackagePath, true);
				}

				Directory.CreateDirectory(Path.Combine(tempFolderPackagePath, "tools"));
				var destinationFilePath = Path.Combine(tempFolderPackagePath, "tools", Path.GetFileName(this.PackageFileLocation));
				await Task.Run(() => File.Copy(this.PackageFileLocation, destinationFilePath, true));

				var nuspecFilePath = PackagePublishViewModel.CreateNuSpecFile(tempFolderPackagePath, this.PackagePublishName, this.PackagePublishVersion);

				var executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var sourceFilePath = Path.Combine(executablePath, PackagePublishViewModel.resourceName);
				var destinationScriptPath = Path.Combine(tempFolderPackagePath, "tools", PackagePublishViewModel.resourceName);
				File.Copy(sourceFilePath, destinationScriptPath, true);

				if (nuspecFilePath != null)
				{
					var packingOperationResult = await _chocolateyService.PackPackage(GenericCommandType.Pack, tempFolderPackagePath);
					if (packingOperationResult.Successful)
					{
						var pushOperationResult = await _chocolateyService.PublishPackage(GenericCommandType.Push, tempFolderPackagePath);
						if (pushOperationResult.Successful)
						{
							await _dialogService.ShowMessageAsync(
								L(nameof(Resources.PackagePublishViewModel_PublishingPackage)),
								L(nameof(Resources.PackagePublishViewModel_PackagePublishedSuccessful)));
							await _eventAggregator.PublishOnUIThreadAsync(new SourcesUpdatedMessage());
							Back();
							ClearPublishedPackageProperties();
						}
						else
						{
							await _dialogService.ShowMessageAsync(
								L(nameof(Resources.PackagePublishViewModel_PublishingPackage)),
								L(nameof(Resources.PackagePublishViewModel_PackagePublishedFailed)));
						}
					}
				}
			}
			catch (Exception ex)
			{
				await _dialogService.ShowMessageAsync(
					L(nameof(Resources.PackagePublishViewModel_PublishingPackage)),
					ex.Message);
			}
			finally
			{
				await _progressService.StopLoading();
			}
		}

		#endregion

		#region Private implementation

		private void BrowsePackageFileLocation(Object value)
		{
			var filter = String.Format("{0}|*.exe|{1}|*.msi|{2}|*.*",
			                           L(nameof(Resources.FilePicker_ExeFiles)),
			                           L(nameof(Resources.FilePicker_MsiFiles)),
			                           L(nameof(Resources.FilePicker_AllFiles)));

			var packageFile = _persistenceService.SelectFile(2, filter);

			if (!String.IsNullOrEmpty(packageFile))
			{
				this.PackageFileLocation = packageFile;
			}
		}

		private static String CreateNuSpecFile(String folderPath, String packageName, String packageVersion)
		{
			XNamespace ns = "http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd";

			var nuspecDocument = new XDocument(
				new XDeclaration("1.0", "utf-8", null),
				new XElement(ns + "package",
				             new XElement(ns + "metadata",
				                          new XElement(ns + "id", packageName),
				                          new XElement(ns + "version", packageVersion),
				                          new XElement(ns + "title", $"{packageName} (Install)"),
				                          new XElement(ns + "authors", "Content Tooling"),
				                          new XElement(ns + "tags", packageName),
				                          new XElement(ns + "summary", $"{packageName} (Install)"),
				                          new XElement(ns + "description", $"{packageName} (Install)")
				             ),
				             new XElement(ns + "files",
				                          new XElement(ns + "file",
				                                       new XAttribute("src", "tools\\**"),
				                                       new XAttribute("target", "tools")
				                          )
				             )
				)
			);

			var nuspecFilePath = Path.Combine(folderPath, $"{packageName}.nuspec");
			nuspecDocument.Save(nuspecFilePath);
			return nuspecFilePath;
		}

		#endregion
	}
}