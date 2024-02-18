// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using System.Xml.Serialization;
	using NuGet.Versioning;

	public class OutdatedPackage
	{
		#region Properties

		public String Id { get; set; }

		[XmlIgnore]
		public NuGetVersion Version => NuGetVersion.Parse(this.VersionString);

		public String VersionString { get; set; }

		#endregion
	}
}