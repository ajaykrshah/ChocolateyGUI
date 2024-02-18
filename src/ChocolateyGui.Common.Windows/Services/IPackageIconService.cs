// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Media;

	public interface IPackageIconService
	{
		ImageSource GetEmptyIconImage();
		ImageSource GetErrorIconImage();
		Task<ImageSource> GetImage(String url, Size desiredSize, DateTime absoluteExpiration);
	}
}