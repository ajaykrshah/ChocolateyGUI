// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Views
{
	using System;
	using System.Windows.Controls;

	/// <summary>
	///     Interaction logic for AdvancedInstallView.xaml
	/// </summary>
	public partial class AdvancedInstallView : UserControl
	{
		#region Constructors

		public AdvancedInstallView()
		{
			InitializeComponent();

			this.Is32BitPlatform = !Environment.Is64BitOperatingSystem;
		}

		#endregion

		#region Properties

		public Boolean Is32BitPlatform { get; set; }

		#endregion
	}
}