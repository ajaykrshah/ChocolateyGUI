// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Attributes
{
	using System;
	using chocolatey.infrastructure.app.attributes;
	using ChocolateyGui.Common.Utilities;

	public class LocalizedCommandForAttribute : CommandForAttribute
	{
		#region Constructors

		public LocalizedCommandForAttribute(String commandName, String key)
			: base(commandName, LocalizedCommandForAttribute.Localize(key))
		{
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