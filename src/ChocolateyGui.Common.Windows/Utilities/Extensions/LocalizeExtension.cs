// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Extensions
{
	using System;
	using System.Windows.Data;
	using ChocolateyGui.Common.Utilities;

	public sealed class LocalizeExtension : Binding
	{
		#region Constructors

		public LocalizeExtension(String name)
			: base("[" + name + "]")
		{
			this.Mode = BindingMode.OneWay;
			this.Source = TranslationSource.Instance;
		}

		#endregion
	}
}