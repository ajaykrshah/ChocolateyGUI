// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Data;

	public class PackageDependenciesToString : IValueConverter
	{
		#region Declarations

		private static readonly Regex PackageNameVersionRegex = new Regex(@"(?<Id>[\w\.]*):{1,2}(?<Version>[\w\.]*)");

		#endregion

		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			var dependenciesString = value as String;
			if (String.IsNullOrWhiteSpace(dependenciesString))
			{
				return String.Empty;
			}

			var dependencyStrings = dependenciesString.Split('|');
			var items = dependencyStrings
			            .Select(dependency =>
			            {
				            var result = String.Empty;

				            var match = PackageDependenciesToString.PackageNameVersionRegex.Match(dependency);
				            var id = match.Groups["Id"];

				            if (id == null || String.IsNullOrWhiteSpace(id.Value))
				            {
					            return result;
				            }

				            result += id.Value;

				            var version = match.Groups["Version"];

				            if (version != null && !String.IsNullOrWhiteSpace(version.Value))
				            {
					            result += " (" + version + ")";
				            }

				            return result;
			            })
			            .Where(dependecy => !String.IsNullOrEmpty(dependecy));

			return String.Join(", ", items);
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}