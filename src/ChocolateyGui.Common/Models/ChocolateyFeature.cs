// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;

	public class ChocolateyFeature : INotifyPropertyChanged
	{
		#region Declarations

		private String _description;
		private Boolean _enabled;
		private String _name;
		private Boolean _setExplicitly;

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

		public Boolean Enabled
		{
			get => _enabled;
			set
			{
				_enabled = value;
				OnPropertyChanged();
			}
		}

		public String Name
		{
			get => _name;
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}

		public Boolean SetExplicitly
		{
			get => _setExplicitly;
			set
			{
				_setExplicitly = value;
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