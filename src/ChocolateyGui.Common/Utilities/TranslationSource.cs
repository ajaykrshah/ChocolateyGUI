// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Utilities
{
	using System;
	using System.ComponentModel;
	using System.Globalization;
	using System.Resources;
	using System.Runtime.CompilerServices;
	using ChocolateyGui.Common.Properties;

	public sealed class TranslationSource : INotifyPropertyChanged
	{
		#region Declarations

		private static readonly Lazy<TranslationSource> _instance =
			new Lazy<TranslationSource>(() => new TranslationSource());

		private readonly ResourceManager _resourceManager = Resources.ResourceManager;
		private CultureInfo _currentCulture;

		#endregion

		#region Properties

		public CultureInfo CurrentCulture
		{
			get => _currentCulture;
			set
			{
				if (!Object.Equals(_currentCulture, value))
				{
					_currentCulture = value;
					NotifyPropertyChanged(String.Empty); // We call with an empty string on purpose to force an update for all resource strings.
				}
			}
		}

		public static TranslationSource Instance { get; } = TranslationSource._instance.Value;

		public String this[String key]
		{
			get
			{
				if (String.IsNullOrEmpty(key))
				{
					// If the key is null we can't pass it to the resource manager.
					// As it also doesn't make sense to pass in an empty value we check for both.
					// We pass this empty string as we don't want anything to break even on empty values.
					return String.Empty;
				}

				var value = _resourceManager.GetString(key, this.CurrentCulture);
#if DEBUG
				if (String.IsNullOrEmpty(value) && !String.IsNullOrEmpty(key))
				{
					return "[" + key + "]";
				}
#endif
				return value;
			}
		}

		public String this[String key, params Object[] parameters]
		{
			get
			{
				var value = this[key];

				if (parameters != null && parameters.Length > 0)
				{
					return String.Format(this.CurrentCulture, value, parameters);
				}

				return value;
			}
		}

		#endregion

		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Private implementation

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}