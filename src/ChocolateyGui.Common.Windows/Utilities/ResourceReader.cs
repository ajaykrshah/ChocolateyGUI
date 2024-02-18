// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Reflection;

	public static class ResourceReader
	{
		#region Internal interface

		[SuppressMessage(
			"Microsoft.Usage",
			"CA2202:Do not dispose objects multiple times",
			Justification = "Based on this blog post: http://blogs.msdn.com/b/tilovell/archive/2014/02/12/the-worst-code-analysis-rule-that-s-recommended-ca2202.aspx")]
		internal static String GetFromResources(Assembly assembly, String resourceName)
		{
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
				{
					return String.Empty;
				}

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		#endregion
	}
}