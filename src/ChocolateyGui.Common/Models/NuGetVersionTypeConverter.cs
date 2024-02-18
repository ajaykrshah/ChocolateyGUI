// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using System.ComponentModel;
	using System.Globalization;
	using NuGet.Versioning;

	public class NuGetVersionTypeConverter : TypeConverter
	{
		#region TypeConverter overrides

		public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(String);
		}

		public override Object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, Object value)
		{
			var version = value as String;
			NuGetVersion semanticVersion;
			if (version != null && NuGetVersion.TryParse(version, out semanticVersion))
			{
				return semanticVersion;
			}

			return null;
		}

		#endregion
	}
}