// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;

	public interface IAllowedCommandsService
	{
		Boolean IsInstallCommandAllowed { get; }
		Boolean IsPinCommandAllowed { get; }
		Boolean IsUninstallCommandAllowed { get; }
		Boolean IsUpgradeAllCommandAllowed { get; }
		Boolean IsUpgradeCommandAllowed { get; }
	}
}