// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Startup
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using ChocolateyGui.Common.Utilities;

	public static class Internationalization
	{
		#region Declarations

		private static readonly HashSet<CultureInfo> _cachedCultures = new HashSet<CultureInfo>();
		private static readonly Lazy<CultureInfo> _fallbackCulture = new Lazy<CultureInfo>(() => new CultureInfo("en"));

		#endregion

		#region Events

		public static event EventHandler<CultureInfo> LanguageChanged;

		#endregion

		#region Public interface

		public static IEnumerable<CultureInfo> GetAllSupportedCultures()
		{
			if (Internationalization._cachedCultures.Count > 0)
			{
				return Internationalization._cachedCultures;
			}

			Internationalization._cachedCultures.Add(Internationalization.GetFallbackCulture());

			var installLocation = Bootstrapper.ApplicationFilesPath;

			foreach (var directory in Directory.EnumerateDirectories(installLocation))
			{
				var resourceAssemblyPath = Path.Combine(directory, "ChocolateyGui.Common.resources.dll");
				if (!File.Exists(resourceAssemblyPath))
				{
					continue;
				}

				try
				{
					var directoryName = Path.GetFileName(directory);

					var culture = new CultureInfo(directoryName);

					if (!Internationalization._cachedCultures.Contains(culture))
					{
						Internationalization._cachedCultures.Add(culture);
					}
				}
				catch (Exception)
				{
					// Ignored on purpose.
				}
			}

			return Internationalization._cachedCultures;
		}

		public static CultureInfo GetFallbackCulture()
		{
			return Internationalization._fallbackCulture.Value;
		}

		public static CultureInfo GetSupportedCultureInfo(String languageCode)
		{
			if (String.IsNullOrEmpty(languageCode))
			{
				return Internationalization.GetFallbackCulture();
			}

			var cultureInfo = new CultureInfo(languageCode);

			var foundCulture = Internationalization.GetAllSupportedCultures().FirstOrDefault(c =>
				                                                                                 String.Equals(c.Name, cultureInfo.Name, StringComparison.OrdinalIgnoreCase));

			if (foundCulture != null)
			{
				return foundCulture;
			}

			if (!String.IsNullOrEmpty(cultureInfo.Parent.Name))
			{
				return Internationalization.GetSupportedCultureInfo(cultureInfo.Parent.Name);
			}

			return Internationalization.GetFallbackCulture();
		}

		public static void Initialize()
		{
			Internationalization.UpdateLanguage(CultureInfo.CurrentCulture.Name);
		}

		public static void UpdateLanguage(String languageCode)
		{
			var existingLanguage = TranslationSource.Instance.CurrentCulture ?? CultureInfo.CurrentCulture;

			var culture = Internationalization.GetSupportedCultureInfo(languageCode);

			if (!object.Equals(culture, existingLanguage))
			{
				TranslationSource.Instance.CurrentCulture = culture;
				CultureInfo.DefaultThreadCurrentCulture = culture;
				CultureInfo.DefaultThreadCurrentUICulture = culture;
				Thread.CurrentThread.CurrentUICulture = culture;
				Thread.CurrentThread.CurrentCulture = culture;

				Internationalization.LanguageChanged?.Invoke(typeof(Internationalization), culture);
			}
		}

		#endregion
	}
}