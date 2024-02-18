// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Collections.Generic;

	public interface IPackageArgumentsService
	{
		IEnumerable<String> DecryptPackageArgumentsFile(String id, String version);
	}
}