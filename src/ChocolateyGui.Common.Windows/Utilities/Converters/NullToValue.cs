// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Globalization;
	using System.Windows.Data;

	public class NullToValue : IValueConverter
	{
		#region Properties

		public Object FalseValue { get; set; }
		public Object TrueValue { get; set; }

		#endregion

		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			if (value is null || (value is String valueAsString && String.IsNullOrWhiteSpace(valueAsString)))
			{
				return this.TrueValue;
			}

			return this.FalseValue;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}