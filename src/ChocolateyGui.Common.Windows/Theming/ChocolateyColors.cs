// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Theming
{
	using System.Windows;

	public static class ChocolateyColors
	{
		#region Properties

		/// <summary>
		///     Gets the resource key for the background color.
		/// </summary>
		public static ComponentResourceKey BackgroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Colors.Background");

		/// <summary>
		///     Gets the resource key for the body color.
		/// </summary>
		public static ComponentResourceKey BodyKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Colors.Body");

		/// <summary>
		///     Gets the resource key for the installed foreground color.
		/// </summary>
		public static ComponentResourceKey IsInstalledForegroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyColors), "Chocolatey.Colors.IsInstalled.Foreground");

		/// <summary>
		///     Gets the resource key for the installed color.
		/// </summary>
		public static ComponentResourceKey IsInstalledKey { get; } = new ComponentResourceKey(typeof(ChocolateyColors), "Chocolatey.Colors.IsInstalled");

		/// <summary>
		///     Gets the resource key for the out of date foreground color.
		/// </summary>
		public static ComponentResourceKey OutOfDateForegroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyColors), "Chocolatey.Colors.OutOfDate.Foreground");

		/// <summary>
		///     Gets the resource key for the out of date color.
		/// </summary>
		public static ComponentResourceKey OutOfDateKey { get; } = new ComponentResourceKey(typeof(ChocolateyColors), "Chocolatey.Colors.OutOfDate");

		/// <summary>
		///     Gets the resource key for the pre-release foreground color.
		/// </summary>
		public static ComponentResourceKey PreReleaseForegroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyColors), "Chocolatey.Colors.PreRelease.Foreground");

		/// <summary>
		///     Gets the resource key for the pre-release color.
		/// </summary>
		public static ComponentResourceKey PreReleaseKey { get; } = new ComponentResourceKey(typeof(ChocolateyColors), "Chocolatey.Colors.PreRelease");

		#endregion
	}
}