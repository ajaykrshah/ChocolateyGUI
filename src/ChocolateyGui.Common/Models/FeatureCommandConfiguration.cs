// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	[Serializable]
	public sealed class FeatureCommandConfiguration
	{
		#region Properties

		public FeatureCommandType Command { get; set; }
		public String Name { get; set; }

		#endregion
	}
}