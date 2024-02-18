// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Theming
{
	using System;
	using System.Windows;
	using System.Windows.Media;
	using Autofac;
	using ChocolateyGui.Common.Windows.Services;
	using ControlzEx.Theming;

	public static class ThemeAssist
	{
		#region Fields and constants

		/// <summary>
		///     Gets or sets the base color scheme for the FrameworkElement.
		/// </summary>
		public static readonly DependencyProperty BaseColorSchemeProperty
			= DependencyProperty.RegisterAttached(
				"BaseColorScheme",
				typeof(String),
				typeof(ThemeAssist),
				new FrameworkPropertyMetadata(
					ThemeManager.BaseColorLight,
					ThemeAssist.OnBaseColorSchemePropertyChanged));

		#endregion

		#region Properties

		/// <summary>
		///     Gets the bundled theme instance for this application.
		/// </summary>
		public static IBundledThemeService BundledTheme { get; } = Bootstrapper.Container.Resolve<IBundledThemeService>();

		#endregion

		#region Public interface

		/// <summary>
		///     Converts a color string to a <see cref="Color" />. If the string couldn't parsed the result will be black.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <returns>The converted <see cref="Color" /> from the string.</returns>
		public static Color ColorFromString(String value)
		{
			return ColorConverter.ConvertFromString(value) is Color color ? color : Colors.Black;
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static String GetBaseColorScheme(DependencyObject obj)
		{
			return (String)obj.GetValue(ThemeAssist.BaseColorSchemeProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
		public static void SetBaseColorScheme(DependencyObject obj, String value)
		{
			obj.SetValue(ThemeAssist.BaseColorSchemeProperty, value);
		}

		/// <summary>
		///     Creates a freezed <see cref="SolidColorBrush" /> from the given <see cref="Color" />.
		/// </summary>
		/// <param name="color">The color for the <see cref="SolidColorBrush" />.</param>
		/// <returns>The <see cref="SolidColorBrush" /> from the given <see cref="Color" />.</returns>
		public static Brush ToBrush(this Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}

		#endregion

		#region Private implementation

		private static void OnBaseColorSchemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FrameworkElement frameworkElement && e.OldValue != e.NewValue && e.NewValue is String baseColorScheme)
			{
				ThemeManager.Current.ChangeThemeBaseColor(frameworkElement, baseColorScheme);
			}
		}

		#endregion
	}
}