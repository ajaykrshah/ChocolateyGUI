// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Startup
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Reflection;
	using Autofac;
	using chocolatey.infrastructure.information;
	using chocolatey.infrastructure.licensing;
	using chocolatey.infrastructure.registration;

	public static class AutoFacConfiguration
	{
		#region Public interface

		[SuppressMessage(
			"Microsoft.Maintainability",
			"CA1506:AvoidExcessiveClassCoupling",
			Justification = "This is really a requirement due to required registrations.")]
		public static IContainer RegisterAutoFac(String chocolateyGuiAssemblySimpleName, String licensedGuiAssemblyLocation, String publicKey)
		{
			var builder = new ContainerBuilder();
			builder.RegisterAssemblyModules(Assembly.GetCallingAssembly());

			var license = License.ValidateLicense();
			if (license.IsValid)
			{
				if (File.Exists(licensedGuiAssemblyLocation))
				{
					var licensedGuiAssembly = AssemblyResolution.ResolveOrLoadAssembly(
						chocolateyGuiAssemblySimpleName,
						publicKey,
						licensedGuiAssemblyLocation);

					if (licensedGuiAssembly != null)
					{
						license.AssemblyLoaded = true;
						license.Assembly = licensedGuiAssembly;
						license.Version = VersionInformation.GetCurrentInformationalVersion(licensedGuiAssembly);

						builder.RegisterAssemblyModules(licensedGuiAssembly.UnderlyingType);
					}
				}
			}

			return builder.Build();
		}

		#endregion
	}
}