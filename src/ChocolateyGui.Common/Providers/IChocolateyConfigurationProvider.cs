// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Providers
{
	using System;

	public interface IChocolateyConfigurationProvider
	{
		String ChocolateyInstall { get; }
		Boolean IsChocolateyExecutableBeingUsed { get; }
	}
}