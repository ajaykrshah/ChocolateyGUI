// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Windows;
	using System.Windows.Data;

	public class BooleanInverter : DependencyObject, IValueConverter, IMultiValueConverter
	{
		#region IMultiValueConverter implementation

		public Object Convert(Object[] values, Type targetType, Object parameter, CultureInfo culture)
		{
			return values.All(value => value == null || value as Boolean? == false);
		}

		public Object[] ConvertBack(Object value, Type[] targetTypes, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return true;
			}

			return value as Boolean? == false;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}