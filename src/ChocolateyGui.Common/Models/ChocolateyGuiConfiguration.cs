// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	[Serializable]
	public class ChocolateyGuiConfiguration
	{
		#region Constructors

		public ChocolateyGuiConfiguration()
		{
			this.RegularOutput = true;
			this.Information = new InformationCommandConfiguration();
			this.FeatureCommand = new FeatureCommandConfiguration();
			this.ConfigCommand = new ConfigCommandConfiguration();
			this.PurgeCommand = new PurgeCommandConfiguration();
		}

		#endregion

		#region Properties

		public String CommandName { get; set; }
		public ConfigCommandConfiguration ConfigCommand { get; set; }
		public FeatureCommandConfiguration FeatureCommand { get; set; }
		public Boolean Global { get; set; }
		public Boolean HelpRequested { get; set; }
		public InformationCommandConfiguration Information { get; set; }
		public String Input { get; set; }
		public PurgeCommandConfiguration PurgeCommand { get; set; }
		public Boolean RegularOutput { get; set; }
		public Boolean UnsuccessfulParsing { get; set; }

		#endregion
	}
}