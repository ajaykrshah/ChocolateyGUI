// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Theming
{
	using System.Windows;

	public static class ChocolateyBrushes
	{
		#region Properties

		/// <summary>
		///     Gets the resource key for the background brush.
		/// </summary>
		public static ComponentResourceKey BackgroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.Background");

		/// <summary>
		///     Gets the resource key for the body brush.
		/// </summary>
		public static ComponentResourceKey BodyKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.Body");

		/// <summary>
		///     Gets the resource key for the installed foreground brush.
		/// </summary>
		public static ComponentResourceKey IsInstalledForegroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.IsInstalled.Foreground");

		/// <summary>
		///     Gets the resource key for the installed brush.
		/// </summary>
		public static ComponentResourceKey IsInstalledKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.IsInstalled");

		/// <summary>
		///     Gets the resource key for the out of date foreground brush.
		/// </summary>
		public static ComponentResourceKey OutOfDateForegroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.OutOfDate.Foreground");

		/// <summary>
		///     Gets the resource key for the out of date brush.
		/// </summary>
		public static ComponentResourceKey OutOfDateKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.OutOfDate");

		/// <summary>
		///     Gets the resource key for the pre-release foreground brush.
		/// </summary>
		public static ComponentResourceKey PreReleaseForegroundKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.PreRelease.Foreground");

		/// <summary>
		///     Gets the resource key for the pre-release brush.
		/// </summary>
		public static ComponentResourceKey PreReleaseKey { get; } = new ComponentResourceKey(typeof(ChocolateyBrushes), "Chocolatey.Brushes.PreRelease");

		#endregion
	}
}