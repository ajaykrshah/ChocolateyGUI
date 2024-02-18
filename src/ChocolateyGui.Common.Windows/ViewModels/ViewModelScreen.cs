// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Utilities;

	public abstract class ViewModelScreen : Screen
	{
		#region Declarations

		private readonly TranslationSource _translationSource;

		#endregion

		#region Constructors

		protected ViewModelScreen()
			: this(TranslationSource.Instance)
		{
		}

		protected ViewModelScreen(TranslationSource translationSource)
		{
			_translationSource = translationSource;
			_translationSource.PropertyChanged += (sender, args) => OnLanguageChanged();
		}

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