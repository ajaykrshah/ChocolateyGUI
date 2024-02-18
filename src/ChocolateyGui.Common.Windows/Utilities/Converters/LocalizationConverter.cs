// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Globalization;
	using System.Windows.Data;
	using ChocolateyGui.Common.Utilities;

	/// <summary>
	///     A converter responsible for formatting strings that are localizable
	///     and accept a single value to use as the parameters.
	///     Nothing is done if the parameter do not exist.
	/// </summary>
	/// <notes>
	///     <para>
	///         The converter parameter is the string that contains the name of the resource
	///         to use as the format string.
	///     </para>
	///     <para>
	///         Be careful about the use of this converter, as dynamic language
	///         changes do not update the converter return value automatically.
	///         Additional steps needs to be taken to ensure it gets updated.
	///     </para>
	/// </notes>
	/// <seealso cref="System.Windows.Data.IValueConverter" />
	[ValueConversion(typeof(Object), typeof(String), ParameterType = typeof(String))]
	public class LocalizationConverter : IValueConverter
	{
		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			var format = "{0}";

			if (parameter is String sParameter)
			{
				format = TranslationSource.Instance[sParameter, value];
			}

			return format;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}