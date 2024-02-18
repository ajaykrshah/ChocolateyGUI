// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;

	public interface ISplashScreenService
	{
		void Close(TimeSpan duration);
		void Show();
	}
}