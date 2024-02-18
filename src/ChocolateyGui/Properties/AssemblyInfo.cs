// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

using System.Reflection;
using System.Resources;
using System.Windows;

[assembly: AssemblyTitle("Chocolatey GUI")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ThemeInfo(
	// where theme specific resource dictionaries are located
	// (used if a resource is not found in the page,
	// or application resource dictionaries)
#pragma warning disable SA1114 // Parameter list must follow declaration
	ResourceDictionaryLocation.None,
#pragma warning restore SA1114 // Parameter list must follow declaration

	// where the generic resource dictionary is located
	// (used if a resource is not found in the page,
	// app, or any theme specific resource dictionaries)
#pragma warning disable SA1115 // Parameter must follow comma
	ResourceDictionaryLocation.SourceAssembly)]
#pragma warning restore SA1115 // Parameter must follow comma