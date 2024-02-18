// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Providers
{
	using System;

	public interface IPlatformProvider
	{
		Tuple<Single, Single> GetDpiScaleFactor();
	}
}