// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Windows;
	using Autofac;
	using ChocolateyGui.Common.Enums;
	using ChocolateyGui.Common.Services;
	using ChocolateyGui.Common.Startup;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.Windows;
	using ChocolateyGui.Common.Windows.Startup;
	using ChocolateyGui.Common.Windows.Theming;
	using ChocolateyGui.Common.Windows.Utilities;

	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		#region Declarations

		private static readonly App _application = new App();
		private static readonly TranslationSource _translationSource = TranslationSource.Instance;

		#endregion

		#region Constructors

		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				var requestedAssembly = new AssemblyName(args.Name);

				try
				{
					if (String.Equals(requestedAssembly.Name, "chocolatey", StringComparison.OrdinalIgnoreCase))
					{
						var installDir = Environment.GetEnvironmentVariable("ChocolateyInstall");
						if (String.IsNullOrEmpty(installDir))
						{
							var rootDrive = Path.GetPathRoot(Assembly.GetExecutingAssembly().Location);
							if (String.IsNullOrEmpty(rootDrive))
							{
								return null; // TODO: Maybe return the chocolatey.dll file instead?
							}

							installDir = Path.Combine(rootDrive, "ProgramData", "chocolatey");
						}

						var assemblyLocation = Path.Combine(installDir, "choco.exe");

						return AssemblyResolver.ResolveOrLoadAssembly("choco", String.Empty, assemblyLocation);
					}

#if FORCE_CHOCOLATEY_OFFICIAL_KEY
                    var chocolateyGuiPublicKey = Bootstrapper.OfficialChocolateyGuiPublicKey;
#else
					var chocolateyGuiPublicKey = Bootstrapper.UnofficialChocolateyGuiPublicKey;
#endif

					if (AssemblyResolver.DoesPublicKeyTokenMatch(requestedAssembly, chocolateyGuiPublicKey)
					    && String.Equals(requestedAssembly.Name, Bootstrapper.ChocolateyGuiCommonAssemblySimpleName, StringComparison.OrdinalIgnoreCase))
					{
						return AssemblyResolver.ResolveOrLoadAssembly(
							Bootstrapper.ChocolateyGuiCommonAssemblySimpleName,
							AssemblyResolver.GetPublicKeyToken(requestedAssembly),
							Bootstrapper.ChocolateyGuiCommonAssemblyLocation);
					}

					if (AssemblyResolver.DoesPublicKeyTokenMatch(requestedAssembly, chocolateyGuiPublicKey)
					    && String.Equals(requestedAssembly.Name, Bootstrapper.ChocolateyGuiCommonWindowsAssemblySimpleName, StringComparison.OrdinalIgnoreCase))
					{
						return AssemblyResolver.ResolveOrLoadAssembly(
							Bootstrapper.ChocolateyGuiCommonWindowsAssemblySimpleName,
							AssemblyResolver.GetPublicKeyToken(requestedAssembly),
							Bootstrapper.ChocolateyGuiCommonWindowsAssemblyLocation);
					}
				}
				catch (Exception ex)
				{
					// TODO: Possibly make these values translatable, do not use Resources directly, instead Use L(nameof(Resources.KEY_NAME));
					var errorMessage = String.Format("Unable to load Chocolatey GUI assembly. {0}", ex.Message);
					ChocolateyMessageBox.Show(errorMessage);
					throw new ApplicationException(errorMessage);
				}

				return null;
			};

			InitializeComponent();
		}

		#endregion

		#region Properties

		internal static SplashScreen SplashScreen { get; set; }

		#endregion

		#region Public interface

		[STAThread]
		public static void Main(String[] args)
		{
			var splashScreenService = Bootstrapper.Container.Resolve<ISplashScreenService>();
			splashScreenService.Show();

			App._application.InitializeComponent();

			try
			{
				App._application.Run();
			}
			catch (Exception ex)
			{
				if (Bootstrapper.IsExiting)
				{
					Bootstrapper.Logger.Error(ex, App.L(nameof(Common.Properties.Resources.Command_GeneralError)));
					return;
				}

				throw;
			}
		}

		#endregion

		#region Application overrides

		/// <inheritdoc />
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ThemeAssist.BundledTheme.Generate("ChocolateyGui");

			var configService = Bootstrapper.Container.Resolve<IConfigService>();
			var effectiveConfiguration = configService.GetEffectiveConfiguration();

			ThemeMode themeMode;
			if (effectiveConfiguration.DefaultToDarkMode == null)
			{
				themeMode = ThemeMode.WindowsDefault;
			}
			else if (effectiveConfiguration.DefaultToDarkMode.Value)
			{
				themeMode = ThemeMode.Dark;
			}
			else
			{
				themeMode = ThemeMode.Light;
			}

			if (String.IsNullOrEmpty(effectiveConfiguration.UseLanguage))
			{
				Internationalization.Initialize();
				configService.SetConfigValue(nameof(effectiveConfiguration.UseLanguage), CultureInfo.CurrentCulture.Name);
			}
			else
			{
				Internationalization.UpdateLanguage(effectiveConfiguration.UseLanguage);
			}

			ThemeAssist.BundledTheme.SyncTheme(themeMode);
		}

		#endregion

		#region Private implementation

		private static String L(String key)
		{
			return App._translationSource[key];
		}

		#endregion
	}
}