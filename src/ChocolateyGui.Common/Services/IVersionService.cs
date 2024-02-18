// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;

	public interface IVersionService
	{
		String DisplayVersion { get; }
		String InformationalVersion { get; }
		String Version { get; }
	}
}