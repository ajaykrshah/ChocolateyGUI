// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	[Serializable]
	public sealed class InformationCommandConfiguration
	{
		#region Properties

		public String ChocolateyGuiProductVersion { get; set; }
		public String ChocolateyGuiVersion { get; set; }
		public String DisplayVersion { get; set; }
		public String FullName { get; set; }

		#endregion
	}
}