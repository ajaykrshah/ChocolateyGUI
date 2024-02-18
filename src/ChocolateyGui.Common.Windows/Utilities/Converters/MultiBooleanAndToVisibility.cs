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

	public class MultiBooleanAndToVisibility : IMultiValueConverter
	{
		#region IMultiValueConverter implementation

		public Object Convert(Object[] values, Type targetType, Object parameter, CultureInfo culture)
		{
			var isPrerelease = (values.ElementAtOrDefault(0) as Boolean?).GetValueOrDefault();
			var showAdditionalPackageInformation = (values.ElementAtOrDefault(1) as Boolean?).GetValueOrDefault();

			return isPrerelease && showAdditionalPackageInformation ? Visibility.Visible : Visibility.Collapsed;
		}

		public Object[] ConvertBack(Object value, Type[] targetTypes, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}