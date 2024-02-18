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

	public class BooleanToVisibility : DependencyObject, IValueConverter, IMultiValueConverter
	{
		#region IMultiValueConverter implementation

		public Object Convert(Object[] values, Type targetType, Object parameter, CultureInfo culture)
		{
			var collapsed = values.Aggregate(false, (current, value) => current || BooleanToVisibility.IsCollapsed(value, parameter));
			return collapsed ? Visibility.Collapsed : Visibility.Visible;
		}

		public Object[] ConvertBack(Object value, Type[] targetTypes, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			return BooleanToVisibility.IsCollapsed(value, parameter)
				? Visibility.Collapsed
				: Visibility.Visible;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Private implementation

		private static Boolean IsCollapsed(Object value, Object parameter)
		{
			var boolVal = (value != null && value != DependencyProperty.UnsetValue) && (Boolean)value;
			return boolVal == false ^ (parameter != null && Boolean.Parse((String)parameter));
		}

		#endregion
	}
}