// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Windows.Data;

	public class StringListToString : IValueConverter
	{
		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			if (value is String valueAsString)
			{
				return String.Join(", ", valueAsString.Split(new[] { " ", ",", ";", "|" }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()));
			}

			return value is IEnumerable<String> items
				? String.Join(", ", items.Select(item => item.Trim()))
				: String.Empty;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}