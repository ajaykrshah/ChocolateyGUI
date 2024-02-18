// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models.Messages
{
	using System;

	public class ShowSourcesMessage
	{
		#region Constructors

		public ShowSourcesMessage(String sourceId = null)
		{
			this.SourceId = sourceId;
		}

		#endregion

		#region Properties

		public String SourceId { get; }

		#endregion
	}
}