// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Startup
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using Serilog;

	public class AssemblyResolver
	{
		#region Declarations

		private const Int32 LOCKRESOLUTIONTIMEOUTSECONDS = 5;
		private static readonly Object _lockObject = new Object();

		#endregion

		#region Public interface

		public static Boolean DoesPublicKeyTokenMatch(AssemblyName assemblyName, String expectedKeyToken)
		{
			var publicKey = AssemblyResolver.GetPublicKeyToken(assemblyName);

			return String.Equals(publicKey, expectedKeyToken, StringComparison.OrdinalIgnoreCase);
		}

		public static String GetPublicKeyToken(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				return String.Empty;
			}

			var publicKeyToken = assemblyName.GetPublicKeyToken();

			if (publicKeyToken == null || publicKeyToken.Length == 0)
			{
				return String.Empty;
			}

			return publicKeyToken.Select(x => x.ToString("x2")).Aggregate((x, y) => x + y);
		}

		/// <summary>
		///     Resolves or loads an assembly. If an assembly is already loaded, no need to reload it.
		/// </summary>
		/// <param name="assemblySimpleName">Simple Name of the assembly, such as "chocolatey"</param>
		/// <param name="publicKeyToken">The public key token.</param>
		/// <param name="assemblyFileLocation">The assembly file location. Typically the path to the DLL on disk.</param>
		/// <returns>An assembly</returns>
		/// <exception cref="Exception">Unable to enter synchronized code to determine assembly loading</exception>
		public static Assembly ResolveOrLoadAssembly(String assemblySimpleName, String publicKeyToken, String assemblyFileLocation)
		{
			return AssemblyResolver.ResolveOrLoadAssemblyInternal(
				assemblySimpleName,
				publicKeyToken,
				assemblyFileLocation,
				assembly => String.Equals(assembly.GetName().Name, assemblySimpleName, StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region Private implementation

		private static Assembly ResolveOrLoadAssemblyInternal(String assemblySimpleName, String publicKeyToken, String assemblyFileLocation, Func<Assembly, Boolean> assemblyPredicate)
		{
			var lockTaken = false;
			try
			{
				Monitor.TryEnter(AssemblyResolver._lockObject, TimeSpan.FromSeconds(AssemblyResolver.LOCKRESOLUTIONTIMEOUTSECONDS), ref lockTaken);
			}
			catch (Exception)
			{
				throw new Exception("Unable to enter synchronized code to determine assembly loading");
			}

			Assembly resolvedAssembly = null;
			if (lockTaken)
			{
				try
				{
					foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(assemblyPredicate))
					{
						if (String.IsNullOrWhiteSpace(publicKeyToken) || String.Equals(AssemblyResolver.GetPublicKeyToken(assembly.GetName()), publicKeyToken, StringComparison.OrdinalIgnoreCase))
						{
							Log.Debug("Returning loaded assembly type for '{0}'", assemblySimpleName);

							resolvedAssembly = assembly;
							break;
						}
					}

					if (resolvedAssembly == null)
					{
						Log.Debug("Loading up '{0}' assembly type from '{1}'", assemblySimpleName, assemblyFileLocation);

						// Reading the raw bytes and calling 'Load' causes an exception, as such we use LoadFrom instead.
						resolvedAssembly = Assembly.LoadFrom(assemblyFileLocation);
					}
				}
				finally
				{
					Monitor.Pulse(AssemblyResolver._lockObject);
					Monitor.Exit(AssemblyResolver._lockObject);
				}
			}

			return resolvedAssembly;
		}

		#endregion
	}
}