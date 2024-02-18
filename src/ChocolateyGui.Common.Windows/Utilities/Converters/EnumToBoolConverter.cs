// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Globalization;
	using System.Windows.Data;

	public class EnumToBoolConverter : IValueConverter
	{
		#region Public interface

		public static Object DefaultEnumValue(Type enumType)
		{
			if (enumType != null)
			{
				if (enumType.IsEnum)
				{
					return Enum.GetValues(enumType).GetValue(0);
				}

				throw new ArgumentException("given type is not an enum");
			}

			return null;
		}

		#endregion

		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			if (value is Enum valueEnum && parameter is Enum paramEnum)
			{
				var equal = System.Convert.ToInt64(paramEnum) == 0 ? System.Convert.ToInt64(valueEnum) == 0 : valueEnum.HasFlag(paramEnum);
				return equal;
			}

			return Binding.DoNothing;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			return object.Equals(true, value) ? parameter : ((parameter != null) ? EnumToBoolConverter.DefaultEnumValue(parameter.GetType()) : Binding.DoNothing);
		}

		#endregion
	}
}