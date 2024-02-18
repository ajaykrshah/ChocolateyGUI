// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models.Messages
{
	using System;
	using NuGet.Versioning;

	public enum PackageChangeType
	{
		Updated,
		Uninstalled,
		Installed,
		Pinned,
		Unpinned
	}

	public class PackageChangedMessage
	{
		#region Constructors

		public PackageChangedMessage(String id, PackageChangeType changeType, NuGetVersion version = null)
		{
			this.Id = id;
			this.ChangeType = changeType;
			this.Version = version;
		}

		#endregion

		#region Properties

		public PackageChangeType ChangeType { get; }
		public String Id { get; }
		public NuGetVersion Version { get; }

		#endregion
	}
}