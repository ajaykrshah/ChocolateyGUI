// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGuiCli
{
	using System;
	using System.IO;
	using System.Reflection;
	using ChocolateyGui.Common.Startup;

	public class Program
	{
		#region Declarations

		private static ResolveEventHandler _handler;

		#endregion

		#region Public interface

		public static void Main(String[] args)
		{
			Program.AddAssemblyResolver();

			Runner.Run(args);
		}

		#endregion

		#region Private implementation

		private static void AddAssemblyResolver()
		{
			Program._handler = (sender, args) =>
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
					Bootstrapper.Logger.Warning("Unable to load Chocolatey GUI assembly. {0}", ex.Message);
				}

				return null;
			};

			AppDomain.CurrentDomain.AssemblyResolve += Program._handler;
		}

		#endregion
	}
}