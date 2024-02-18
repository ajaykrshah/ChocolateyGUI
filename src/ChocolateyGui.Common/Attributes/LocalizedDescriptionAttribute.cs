// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Attributes
{
	using System;
	using System.ComponentModel;
	using ChocolateyGui.Common.Utilities;

	[AttributeUsage(AttributeTargets.Property)]
	public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
	{
		#region Constructors

		public LocalizedDescriptionAttribute(String key)
			: base(LocalizedDescriptionAttribute.Localize(key))
		{
			this.Key = key;
		}

		#endregion

		#region Properties

		public String Key { get; set; }

		#endregion

		#region Private implementation

		private static String Localize(String key)
		{
			return TranslationSource.Instance[key];
		}

		#endregion
	}
}