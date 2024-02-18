// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models.Messages
{
	using System;
	using NuGet.Versioning;

	public class PackageHasUpdateMessage
	{
		#region Constructors

		public PackageHasUpdateMessage(String id, NuGetVersion version)
		{
			this.Id = id;
			this.Version = version;
		}

		#endregion

		#region Properties

		public String Id { get; }
		public NuGetVersion Version { get; set; }

		#endregion
	}
}