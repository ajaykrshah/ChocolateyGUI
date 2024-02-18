// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Providers
{
	using System;
	using System.Linq;
	using System.Reflection;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.Utilities;

	public class VersionNumberProvider : IVersionNumberProvider
	{
		#region Declarations

		private String _version;

		#endregion

		#region IVersionNumberProvider implementation

		public virtual String Version
		{
			get
			{
				if (_version != null)
				{
					return _version;
				}

				var assembly = GetType().Assembly;
				var informational =
					((AssemblyInformationalVersionAttribute[])assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute)))
					.First();

				_version = TranslationSource.Instance[nameof(Resources.VersionNumberProvider_VersionFormat), informational.InformationalVersion];
				return _version;
			}
		}

		#endregion
	}
}