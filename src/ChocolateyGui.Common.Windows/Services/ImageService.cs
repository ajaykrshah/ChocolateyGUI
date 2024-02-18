// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;

	public class ImageService : IImageService
	{
		#region IImageService implementation

		public ImageSource PrimaryApplicationImage
		{
			get
			{
				var image = new BitmapImage(new Uri("pack://application:,,,/ChocolateyGui;component/chocolatey_logo.png", UriKind.RelativeOrAbsolute));
				image.Freeze();
				return image;
			}
		}

		public ImageSource SecondaryApplicationImage
		{
			get
			{
				var image = new BitmapImage(new Uri("pack://application:,,,/ChocolateyGui;component/ivanti.png", UriKind.RelativeOrAbsolute));
				image.Freeze();
				return image;
			}
		}

		public String SplashScreenImageName
		{
			get
			{
				var dpi = NativeMethods.GetScaleFactor();
				var img = "chocolatey.png";

				if (dpi >= 2f)
				{
					img = "chocolatey@3.png";
				}
				else if (dpi > 1.00f)
				{
					img = "chocolatey@2.png";
				}

				return img;
			}
		}

		public Uri ToolbarIconUri => new Uri("pack://application:,,,/chocolateyicon.ico");

		#endregion
	}
}