// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Windows;
	using System.Windows.Input;
	using Microsoft.Xaml.Behaviors;

	/// <summary>
	///     The BubbleScrollEventBehavior behavior can be used to prevent the mousewheel scrolling on a scrollable control.
	///     The event will be bubble up to the parent control.
	///     This behavior can be prevent with the left Shift key.
	/// </summary>
	public class BubbleScrollEventBehavior : Behavior<UIElement>
	{
		#region Behavior overrides

		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.PreviewMouseWheel -= BubbleScrollEventBehavior.AssociatedObject_PreviewMouseWheel;
			this.AssociatedObject.PreviewMouseWheel += BubbleScrollEventBehavior.AssociatedObject_PreviewMouseWheel;
		}

		protected override void OnDetaching()
		{
			this.AssociatedObject.PreviewMouseWheel -= BubbleScrollEventBehavior.AssociatedObject_PreviewMouseWheel;

			base.OnDetaching();
		}

		#endregion

		#region Private implementation

		private static void AssociatedObject_PreviewMouseWheel(Object sender, MouseWheelEventArgs e)
		{
			var uiElement = sender as UIElement;
			if (uiElement == null || Keyboard.IsKeyDown(Key.LeftShift))
			{
				return;
			}

			e.Handled = true;

			var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent };
			uiElement.RaiseEvent(e2);
		}

		#endregion
	}
}