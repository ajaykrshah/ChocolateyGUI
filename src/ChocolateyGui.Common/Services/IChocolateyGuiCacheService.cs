// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	public interface IChocolateyGuiCacheService
	{
		void PurgeIcons();
		void PurgeOutdatedPackages();
	}
}