// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.IO;
	using System.Windows.Forms;
	using ChocolateyGui.Common.Services;
	using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
	using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

	public class PersistenceService : IPersistenceService
	{
		#region IPersistenceService implementation

		public String GetFilePath(String defaultExtension, String filter)
		{
			var fd = new SaveFileDialog { DefaultExt = defaultExtension, Filter = filter };

			var result = fd.ShowDialog();

			return result != null && result.Value ? fd.FileName : null;
		}

		public String GetFolderPath(String defaultLocation, String description = null)
		{
			var fd = new FolderBrowserDialog();
			fd.SelectedPath = defaultLocation;
			fd.Description = description;

			if (fd.ShowDialog() == DialogResult.OK)
			{
				var path = fd.SelectedPath;

				return path;
			}

			return null;
		}

		public Stream OpenFile(String defaultExtension, String filter)
		{
			var fd = new OpenFileDialog { DefaultExt = defaultExtension, Filter = filter };

			var result = fd.ShowDialog();

			return result != null && result.Value ? fd.OpenFile() : null;
		}

		public Stream SaveFile(String defaultExtension, String filter)
		{
			var fd = new SaveFileDialog { DefaultExt = defaultExtension, Filter = filter };

			var result = fd.ShowDialog();

			return result != null && result.Value ? fd.OpenFile() : null;
		}

		public String SelectFile(Int32 defaultExtensionIndex, String filter)
		{
			var fd = new OpenFileDialog { FilterIndex = defaultExtensionIndex, Filter = filter };

			return fd.ShowDialog() == true ? fd.FileName : null;
		}

		#endregion
	}
}