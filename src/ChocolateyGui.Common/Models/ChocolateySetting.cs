// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;

	public class ChocolateySetting : INotifyPropertyChanged
	{
		#region Declarations

		private String _description;
		private String _key;
		private String _value;

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

		public String Key
		{
			get => _key;
			set
			{
				_key = value;
				OnPropertyChanged();
			}
		}

		public String Value
		{
			get => _value;
			set
			{
				_value = value;
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