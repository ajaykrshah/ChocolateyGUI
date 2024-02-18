// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;

	public class ChocolateyGuiFeature : INotifyPropertyChanged
	{
		#region Declarations

		private String _description;
		private String _displayTitle;
		private Boolean _enabled;
		private String _title;

		#endregion

		#region Properties

		public String Description
		{
			get => _description;
			set
			{
				_description = value;
				OnPropertyChanged();
			}
		}

		public String DisplayTitle
		{
			get => _displayTitle ?? this.Title;
			set
			{
				_displayTitle = value;
				OnPropertyChanged();
			}
		}

		public Boolean Enabled
		{
			get => _enabled;
			set
			{
				_enabled = value;
				OnPropertyChanged();
			}
		}

		public String Title
		{
			get => _title;
			set
			{
				_title = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Protected interface

		protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}