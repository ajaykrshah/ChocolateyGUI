// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Controls.Dialogs
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Media;
	using ChocolateyGui.Common.Controls;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Utilities;
	using ChocolateyGui.Common.Windows.Theming;
	using ControlzEx.Theming;
	using MahApps.Metro.Controls;
	using MahApps.Metro.Controls.Dialogs;

	/// <summary>
	///     Interaction logic for ChocolateyDialog.xaml
	/// </summary>
	public partial class ChocolateyDialog : CustomDialog
	{
		#region Constructors

		internal ChocolateyDialog(MetroWindow parentWindow, Boolean showConsoleOutput)
		{
			this.ShowOutputConsole = showConsoleOutput;

			InitializeComponent();

			if (parentWindow.MetroDialogOptions.ColorScheme == MetroDialogColorScheme.Theme)
			{
				this.ProgressBarForeground = FindResource(ChocolateyBrushes.BodyKey) as Brush;
			}
			else
			{
				this.ProgressBarForeground = Brushes.White;
			}

			this.NegativeButtonText = TranslationSource.Instance[nameof(Properties.Resources.ChocolateyDialog_Cancel)];
		}

		#endregion

		#region Fields and constants

		public static readonly DependencyProperty IsCancelableProperty
			= DependencyProperty.Register(
				nameof(ChocolateyDialog.IsCancelable),
				typeof(Boolean),
				typeof(ChocolateyDialog),
				new PropertyMetadata(default(Boolean)));

		public static readonly DependencyProperty NegativeButtonTextProperty
			= DependencyProperty.Register(
				nameof(ChocolateyDialog.NegativeButtonText),
				typeof(String),
				typeof(ChocolateyDialog),
				new PropertyMetadata("Cancel"));

		public static readonly DependencyProperty OutputBufferCollectionProperty
			= DependencyProperty.Register(
				nameof(ChocolateyDialog.OutputBufferCollection),
				typeof(ObservableRingBufferCollection<PowerShellOutputLine>),
				typeof(ChocolateyDialog),
				new PropertyMetadata(
					default(ObservableRingBufferCollection<PowerShellOutputLine>),
					(s, e) =>
					{
						((ChocolateyDialog)s).PART_Console.BufferCollection =
							(ObservableRingBufferCollection<PowerShellOutputLine>)e.NewValue;
					}));

		public static readonly DependencyProperty ProgressBarForegroundProperty
			= DependencyProperty.Register(
				nameof(ChocolateyDialog.ProgressBarForeground),
				typeof(Brush),
				typeof(ChocolateyDialog),
				new PropertyMetadata(Brushes.White));

		#endregion

		#region Properties

		public Boolean IsCancelable
		{
			get => (Boolean)GetValue(ChocolateyDialog.IsCancelableProperty);
			set => SetValue(ChocolateyDialog.IsCancelableProperty, value);
		}

		public String NegativeButtonText
		{
			get => (String)GetValue(ChocolateyDialog.NegativeButtonTextProperty);
			set => SetValue(ChocolateyDialog.NegativeButtonTextProperty, value);
		}

		public ObservableRingBufferCollection<PowerShellOutputLine> OutputBufferCollection
		{
			get => (ObservableRingBufferCollection<PowerShellOutputLine>)GetValue(ChocolateyDialog.OutputBufferCollectionProperty);
			set => SetValue(ChocolateyDialog.OutputBufferCollectionProperty, value);
		}

		public Brush ProgressBarForeground
		{
			get => (Brush)GetValue(ChocolateyDialog.ProgressBarForegroundProperty);
			set => SetValue(ChocolateyDialog.ProgressBarForegroundProperty, value);
		}

		public Boolean ShowOutputConsole { get; set; }

		#endregion

		#region BaseMetroDialog overrides

		protected override void OnClose()
		{
			base.OnClose();
			this.OutputBufferCollection.Clear();
			ThemeManager.Current.ThemeChanged -= ThemeManagerIsThemeChanged;
		}

		protected override void OnLoaded()
		{
			ThemeManager.Current.ThemeChanged -= ThemeManagerIsThemeChanged;
			ThemeManager.Current.ThemeChanged += ThemeManagerIsThemeChanged;
			base.OnLoaded();
		}

		#endregion

		#region Private implementation

		private Theme DetectTheme()
		{
			if (Application.Current != null)
			{
				var theme = Application.Current.MainWindow is null
					? ThemeManager.Current.DetectTheme(Application.Current)
					: ThemeManager.Current.DetectTheme(Application.Current.MainWindow);
				return theme;
			}

			return null;
		}

		private void OnThemeChange()
		{
			var theme = DetectTheme();

			if (DesignerProperties.GetIsInDesignMode(this)
			    || theme == null)
			{
				return;
			}

			if (this.DialogSettings != null)
			{
				if (this.DialogSettings.ColorScheme == MetroDialogColorScheme.Theme)
				{
					this.ProgressBarForeground = FindResource(ChocolateyBrushes.BodyKey) as Brush;
				}
				else
				{
					this.ProgressBarForeground = theme.BaseColorScheme == ThemeManager.BaseColorLight ? Brushes.White : Brushes.Black;
				}
			}
		}

		private void ThemeManagerIsThemeChanged(Object sender, ThemeChangedEventArgs e)
		{
			this.Invoke(OnThemeChange);
		}

		#endregion
	}
}