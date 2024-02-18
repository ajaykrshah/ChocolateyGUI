// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Windows.Media;

	public interface IImageService
	{
		ImageSource PrimaryApplicationImage { get; }
		ImageSource SecondaryApplicationImage { get; }
		String SplashScreenImageName { get; }
		Uri ToolbarIconUri { get; }
	}
}