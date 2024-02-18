// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using System;
	using System.IO;

	public interface IPersistenceService
	{
		String GetFilePath(String defaultExtension, String filter);
		String GetFolderPath(String defaultLocation, String description = null);
		Stream OpenFile(String defaultExtension, String filter);
		Stream SaveFile(String defaultExtension, String filter);
		String SelectFile(Int32 defaultExtensionIndex, String filter);
	}
}