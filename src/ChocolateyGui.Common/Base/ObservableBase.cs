// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Base
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Runtime.CompilerServices;
	using ChocolateyGui.Common.Utilities;

	public abstract class ObservableBase : INotifyPropertyChanged
	{
		#region Declarations

		private readonly TranslationSource _translationSource;

		#endregion

		#region Constructors

		protected ObservableBase()
			: this(TranslationSource.Instance)
		{
		}

		protected ObservableBase(TranslationSource translationSource)
		{
			_translationSource = translationSource;
			_translationSource.PropertyChanged += (sender, args) => OnLanguageChanged();
		}

		#endregion

		#region Public interface

		public void NotifyPropertyChanged(String propertyName)
		{
			var propertyChangedEvent = PropertyChanged;
			if (propertyChangedEvent != null)
			{
				propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public Boolean SetPropertyValue<T>(ref T property, T value, [CallerMemberName] String propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(property, value))
			{
				return false;
			}

			property = value;
			NotifyPropertyChanged(propertyName);
			return true;
		}

		#endregion

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Protected interface

		protected String L(String key)
		{
			return _translationSource[key];
		}

		protected String L(String key, params Object[] parameters)
		{
			return _translationSource[key, parameters];
		}

		protected virtual void OnLanguageChanged()
		{
		}

		#endregion
	}
}