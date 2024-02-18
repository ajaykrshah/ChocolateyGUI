// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using Microsoft.Xaml.Behaviors;

	public class ToolTipBehavior : Behavior<FrameworkElement>
	{
		#region Fields and constants

		public static readonly DependencyProperty EnabledToolTipProperty
			= DependencyProperty.Register(
				nameof(ToolTipBehavior.EnabledToolTip),
				typeof(Object),
				typeof(ToolTipBehavior),
				new PropertyMetadata(default, ToolTipBehavior.OnToolTipPropertyChanged));

		public static readonly DependencyProperty DisabledToolTipProperty
			= DependencyProperty.Register(
				nameof(ToolTipBehavior.DisabledToolTip),
				typeof(Object),
				typeof(ToolTipBehavior),
				new PropertyMetadata(default, ToolTipBehavior.OnToolTipPropertyChanged));

		public static readonly DependencyProperty IsFeatureEnabledProperty
			= DependencyProperty.Register(
				nameof(ToolTipBehavior.IsFeatureEnabled),
				typeof(Boolean),
				typeof(ToolTipBehavior),
				new PropertyMetadata(true, ToolTipBehavior.OnToolTipPropertyChanged));

		public static readonly DependencyProperty DisabledFeatureToolTipProperty
			= DependencyProperty.Register(
				nameof(ToolTipBehavior.DisabledFeatureToolTip),
				typeof(Object),
				typeof(ToolTipBehavior),
				new PropertyMetadata(default, ToolTipBehavior.OnToolTipPropertyChanged));

		#endregion

		#region Properties

		public Object DisabledFeatureToolTip
		{
			get => GetValue(ToolTipBehavior.DisabledFeatureToolTipProperty);
			set => SetValue(ToolTipBehavior.DisabledFeatureToolTipProperty, value);
		}

		public Object DisabledToolTip
		{
			get => GetValue(ToolTipBehavior.DisabledToolTipProperty);
			set => SetValue(ToolTipBehavior.DisabledToolTipProperty, value);
		}

		public Object EnabledToolTip
		{
			get => GetValue(ToolTipBehavior.EnabledToolTipProperty);
			set => SetValue(ToolTipBehavior.EnabledToolTipProperty, value);
		}

		public Boolean IsFeatureEnabled
		{
			get => (Boolean)GetValue(ToolTipBehavior.IsFeatureEnabledProperty);
			set => SetValue(ToolTipBehavior.IsFeatureEnabledProperty, value);
		}

		#endregion

		#region Behavior overrides

		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.SetCurrentValue(ToolTipService.ShowOnDisabledProperty, true);

			SetTheToolTip();

			this.AssociatedObject.IsEnabledChanged += AssociatedObject_IsEnabledChanged;
		}

		protected override void OnDetaching()
		{
			this.AssociatedObject.IsEnabledChanged -= AssociatedObject_IsEnabledChanged;

			base.OnDetaching();
		}

		#endregion

		#region Private implementation

		private void AssociatedObject_IsEnabledChanged(Object sender, DependencyPropertyChangedEventArgs e)
		{
			SetTheToolTip();
		}

		private static void OnToolTipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != e.OldValue)
			{
				(d as ToolTipBehavior)?.SetTheToolTip();
			}
		}

		private void SetTheToolTip()
		{
			if (this.AssociatedObject is null)
			{
				return;
			}

			if (!this.AssociatedObject.IsEnabled && !this.IsFeatureEnabled)
			{
				this.AssociatedObject.SetCurrentValue(FrameworkElement.ToolTipProperty, this.DisabledFeatureToolTip);
			}
			else if (this.AssociatedObject.IsEnabled)
			{
				this.AssociatedObject.SetCurrentValue(FrameworkElement.ToolTipProperty, this.EnabledToolTip);
			}
			else
			{
				this.AssociatedObject.SetCurrentValue(FrameworkElement.ToolTipProperty, this.DisabledToolTip);
			}
		}

		#endregion
	}
}