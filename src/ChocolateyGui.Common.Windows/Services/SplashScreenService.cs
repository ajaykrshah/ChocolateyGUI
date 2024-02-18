// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Windows;
	using ChocolateyGui.Common.Services;

	public class SplashScreenService : ISplashScreenService
	{
		#region Declarations

		private readonly IImageService _imageService;
		private SplashScreen _splashScreen;

		#endregion

		#region Constructors

		public SplashScreenService(IImageService imageService)
		{
			_imageService = imageService;
		}

		#endregion

		#region ISplashScreenService implementation

		public void Close(TimeSpan duration)
		{
			_splashScreen.Close(duration);
		}

		public void Show()
		{
			_splashScreen = new SplashScreen(_imageService.SplashScreenImageName);
			_splashScreen.Show(true, true);
		}

		#endregion
	}
}