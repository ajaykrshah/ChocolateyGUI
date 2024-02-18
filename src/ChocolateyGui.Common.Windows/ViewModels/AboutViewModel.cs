// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Services;

	public sealed class AboutViewModel : ViewModelScreen
	{
		#region Declarations

		private readonly IEventAggregator _eventAggregator;
		private readonly IVersionService _versionService;

		#endregion

		#region Constructors

		public AboutViewModel(IEventAggregator eventAggregator, IVersionService versionService)
		{
			_eventAggregator = eventAggregator;
			_versionService = versionService;
		}

		#endregion

		#region Properties

		public String ChocolateyGuiInformationalVersion => _versionService.InformationalVersion;
		public String ChocolateyGuiVersion => _versionService.Version;

		#endregion

		#region Public interface

		public void Back()
		{
			_eventAggregator.PublishOnUIThread(new AboutGoBackMessage());
		}

		#endregion
	}
}