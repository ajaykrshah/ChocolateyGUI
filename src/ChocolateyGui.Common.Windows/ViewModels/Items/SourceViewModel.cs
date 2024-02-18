// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels.Items
{
	using System;
	using ChocolateyGui.Common.Base;

	public class SourceViewModel : ObservableBase
	{
		#region Declarations

		private String _name;
		private String _url;

		#endregion

		#region Properties

		public String Name
		{
			get => _name;
			set => SetPropertyValue(ref _name, value);
		}

		public String Url
		{
			get => _url;
			set => SetPropertyValue(ref _url, value);
		}

		#endregion
	}
}