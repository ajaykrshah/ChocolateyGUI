// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	[Serializable]
	public sealed class PurgeCommandConfiguration
	{
		#region Properties

		public PurgeCommandType Command { get; set; }

		#endregion
	}
}