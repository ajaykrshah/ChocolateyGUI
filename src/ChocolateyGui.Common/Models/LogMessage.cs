// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	public class LogMessage
	{
		#region Properties

		public String Context { get; set; }
		public LogLevel LogLevel { get; set; }
		public String Message { get; set; }

		#endregion
	}
}