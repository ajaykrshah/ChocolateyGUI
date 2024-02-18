// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;

	public class AllowedCommandsService : IAllowedCommandsService
	{
		#region IAllowedCommandsService implementation

		public Boolean IsInstallCommandAllowed => true;
		public Boolean IsPinCommandAllowed => true;
		public Boolean IsUninstallCommandAllowed => true;
		public Boolean IsUpgradeAllCommandAllowed => true;
		public Boolean IsUpgradeCommandAllowed => true;

		#endregion
	}
}