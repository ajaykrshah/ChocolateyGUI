// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows
{
	using System;
	using System.ComponentModel;
	using System.Windows;

	internal static class DesignMode
	{
		#region Declarations

		private static Boolean? isInDesignMode;

		#endregion

		#region Properties

		/// <summary>
		///     Gets a value indicating whether the control is in design mode (running in Blend
		///     or Visual Studio).
		/// </summary>
		public static Boolean IsInDesignModeStatic
		{
			get
			{
				if (!DesignMode.isInDesignMode.HasValue)
				{
					var prop = DesignerProperties.IsInDesignModeProperty;
					DesignMode.isInDesignMode
						= (Boolean)DependencyPropertyDescriptor
						           .FromProperty(prop, typeof(FrameworkElement))
						           .Metadata.DefaultValue;
				}

				return DesignMode.isInDesignMode.Value;
			}
		}

		#endregion
	}
}