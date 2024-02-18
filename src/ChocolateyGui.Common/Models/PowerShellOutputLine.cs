// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	public enum PowerShellLineType
	{
		Output,
		Error,
		Warning,
		Verbose,
		Debug
	}

	public class PowerShellOutputLine
	{
		#region Constructors

		public PowerShellOutputLine(String text, PowerShellLineType lineType, Boolean newLine = true)
		{
			this.Text = text;
			this.LineType = lineType;
			this.NewLine = newLine;
		}

		#endregion

		#region Properties

		public PowerShellLineType LineType { get; private set; }
		public Boolean NewLine { get; private set; }
		public String Text { get; private set; }

		#endregion
	}
}