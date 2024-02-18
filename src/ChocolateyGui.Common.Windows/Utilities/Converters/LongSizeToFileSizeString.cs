// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	using System;
	using System.Globalization;
	using System.Text;
	using System.Windows.Data;

	public sealed class LongSizeToFileSizeString : IValueConverter
	{
		#region Public interface

		/// <summary>
		///     Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes,
		///     megabytes, or gigabytes, depending on the size.
		/// </summary>
		/// <param name="fileSize">The numeric value to be converted.</param>
		/// <returns>the converted string</returns>
		public static String StrFormatByteSize(Int64 fileSize)
		{
			var sb = new StringBuilder(16);
			NativeMethods.StrFormatByteSize(fileSize, sb, sb.Capacity);
			return sb.ToString();
		}

		#endregion

		#region IValueConverter implementation

		public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			return value is Int64 fileSize ? LongSizeToFileSizeString.StrFormatByteSize(fileSize) : String.Empty;
		}

		public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}