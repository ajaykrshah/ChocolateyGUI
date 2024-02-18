// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Data;
	using chocolatey;
	using NuGet.Versioning;

	public class NuGetVersionToString : DependencyObject, IValueConverter
	{
		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			if (value is NuGetVersion version)
			{
				return version.ToNormalizedStringChecked();
			}

			return value;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			if (value is String sValue && NuGetVersion.TryParse(sValue, out var version))
			{
				return version;
			}

			throw new InvalidOperationException("The passed in value is not a parseable string version!");
		}

		#endregion
	}
}