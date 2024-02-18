// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media;
	using ChocolateyGui.Common.Base;
	using ChocolateyGui.Common.Enums;
	using ChocolateyGui.Common.Windows.Commands;
	using ChocolateyGui.Common.Windows.Theming;
	using ControlzEx.Theming;

	public class BundledThemeService : ObservableBase, IBundledThemeService
	{
		#region Declarations

		private Theme _dark;
		private Boolean _isLightTheme;
		private Theme _light;
		private ThemeMode _themeMode;

		#endregion

		#region Constructors

		public BundledThemeService()
		{
			this.ToggleTheme =
				new RelayCommand(
					o => { ThemeManager.Current.ChangeTheme(Application.Current, this.IsLightTheme ? this.Light : this.Dark); },
					o => this.Light != null && this.Dark != null);
		}

		#endregion

		#region IBundledThemeService implementation

		/// <inheritdoc />
		public Theme Dark
		{
			get => _dark;
			private set => SetPropertyValue(ref _dark, value);
		}

		/// <inheritdoc />
		public void Generate(String scheme)
		{
			this.Light = ThemeManager.Current.AddTheme(BundledThemeService.GenerateTheme(scheme, false));
			this.Dark = ThemeManager.Current.AddTheme(BundledThemeService.GenerateTheme(scheme, true));

			if (this.Light != null)
			{
				ThemeManager.Current.ChangeTheme(Application.Current, this.Light);
			}

			this.IsLightTheme = true;
		}

		/// <inheritdoc />
		public Boolean IsLightTheme
		{
			get => _isLightTheme;
			set
			{
				if (SetPropertyValue(ref _isLightTheme, value))
				{
					this.ThemeMode = value ? ThemeMode.Light : ThemeMode.Dark;
				}
			}
		}

		/// <inheritdoc />
		public Theme Light
		{
			get => _light;
			private set => SetPropertyValue(ref _light, value);
		}

		/// <inheritdoc />
		public void SyncTheme(ThemeMode mode)
		{
			if (mode == ThemeMode.WindowsDefault)
			{
				ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
				ThemeManager.Current.SyncTheme();

				var theme = ThemeManager.Current.DetectTheme();
				this.IsLightTheme = theme is null || theme.BaseColorScheme == ThemeManager.BaseColorLight;
			}
			else
			{
				this.IsLightTheme = mode == ThemeMode.Light;
				ThemeManager.Current.ChangeTheme(Application.Current, this.IsLightTheme ? this.Light : this.Dark);
			}

			this.ThemeMode = mode;
		}

		/// <inheritdoc />
		public ThemeMode ThemeMode
		{
			get => _themeMode;
			private set => SetPropertyValue(ref _themeMode, value);
		}

		/// <inheritdoc />
		public ICommand ToggleTheme { get; }

		#endregion

		#region Private implementation

		private static Theme GenerateTheme(String scheme, Boolean isDark)
		{
			var baseColor = isDark ? ThemeManager.BaseColorDark : ThemeManager.BaseColorLight;
			var accentColor = isDark ? ThemeAssist.ColorFromString("#FF4F6170") : ThemeAssist.ColorFromString("#FF202F3C");

			var theme = new Theme(
				name: $"{baseColor}.{scheme}",
				displayName: $"{scheme} theme ({baseColor})",
				baseColorScheme: baseColor,
				colorScheme: scheme,
				primaryAccentColor: accentColor,
				showcaseBrush: new SolidColorBrush(accentColor),
				isRuntimeGenerated: true,
				isHighContrast: false);

			var backgroundColor = isDark ? ThemeAssist.ColorFromString("#333333") : ThemeAssist.ColorFromString("#F0EEE0");
			theme.Resources[ChocolateyColors.BackgroundKey] = backgroundColor;
			theme.Resources[ChocolateyBrushes.BackgroundKey] = backgroundColor.ToBrush();

			var bodyColor = isDark ? ThemeAssist.ColorFromString("#F0EEE0") : ThemeAssist.ColorFromString("#333333");
			theme.Resources[ChocolateyColors.BodyKey] = bodyColor;
			theme.Resources[ChocolateyBrushes.BodyKey] = bodyColor.ToBrush();

			var outOfDateColor = ThemeAssist.ColorFromString("#b71c1c");
			theme.Resources[ChocolateyColors.OutOfDateKey] = outOfDateColor;
			theme.Resources[ChocolateyBrushes.OutOfDateKey] = outOfDateColor.ToBrush();

			theme.Resources[ChocolateyColors.OutOfDateForegroundKey] = Colors.White;
			theme.Resources[ChocolateyBrushes.OutOfDateForegroundKey] = Colors.White.ToBrush();

			var isInstalledColor = ThemeAssist.ColorFromString("#1b5e20");
			theme.Resources[ChocolateyColors.IsInstalledKey] = isInstalledColor;
			theme.Resources[ChocolateyBrushes.IsInstalledKey] = isInstalledColor.ToBrush();

			theme.Resources[ChocolateyColors.IsInstalledForegroundKey] = Colors.White;
			theme.Resources[ChocolateyBrushes.IsInstalledForegroundKey] = Colors.White.ToBrush();

			var preReleaseColor = ThemeAssist.ColorFromString("#ff8f00");
			theme.Resources[ChocolateyColors.PreReleaseKey] = preReleaseColor;
			theme.Resources[ChocolateyBrushes.PreReleaseKey] = preReleaseColor.ToBrush();

			theme.Resources[ChocolateyColors.PreReleaseForegroundKey] = Colors.Black;
			theme.Resources[ChocolateyBrushes.PreReleaseForegroundKey] = Colors.Black.ToBrush();

			return theme;
		}

		#endregion
	}
}