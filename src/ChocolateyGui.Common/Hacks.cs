// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common
{
	using System;
	using System.Security.Principal;

	public static class Hacks
	{
		#region Properties

		public static Boolean IsElevated => (WindowsIdentity.GetCurrent().Owner?.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)).GetValueOrDefault(false);

		#endregion
	}
}