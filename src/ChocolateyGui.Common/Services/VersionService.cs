// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;
	using System.Reflection;

	public class VersionService : IVersionService
	{
		#region IVersionService implementation

		public String DisplayVersion => String.Format("{0} v{1}", "Chocolatey GUI", this.Version);
		public String InformationalVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
		public String Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

		#endregion
	}
}