// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows
{
	using System;
	using Autofac;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Windows.Utilities.Extensions;

	public class Elevation : PropertyChangedBase
	{
		#region Declarations

		private Boolean _isBackgroundRunning;
		private Boolean _isElevated = Hacks.IsElevated;

		#endregion

		#region Properties

		public static Elevation Instance => Bootstrapper.Container.Resolve<Elevation>();

		public Boolean IsBackgroundRunning
		{
			get => _isBackgroundRunning;
			set => this.SetPropertyValue(ref _isBackgroundRunning, value);
		}

		public Boolean IsElevated
		{
			get => _isElevated;
			set => this.SetPropertyValue(ref _isElevated, value);
		}

		#endregion
	}
}