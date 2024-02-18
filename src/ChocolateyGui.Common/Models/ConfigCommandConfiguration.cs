// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	[Serializable]
	public sealed class ConfigCommandConfiguration
	{
		#region Properties

		public ConfigCommandType Command { get; set; }
		public String ConfigValue { get; set; }
		public String Name { get; set; }

		#endregion
	}
}