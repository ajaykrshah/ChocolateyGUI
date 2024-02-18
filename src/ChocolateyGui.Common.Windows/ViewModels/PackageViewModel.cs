// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Models.Messages;
	using ChocolateyGui.Common.Properties;
	using ChocolateyGui.Common.ViewModels.Items;

	public sealed class PackageViewModel : ViewModelScreen
	{
		#region Declarations

		private readonly IEventAggregator _eventAggregator;

		#endregion

		#region Constructors

		public PackageViewModel(IEventAggregator eventAggregator)
		{
			_eventAggregator = eventAggregator;
		}

		#endregion

		#region Properties

		public new String DisplayName => L(nameof(Resources.PackageViewModel_DisplayName), this.Package?.Title);
		public IPackageViewModel Package { get; set; }

		#endregion

		#region Public interface

		public void Back()
		{
			_eventAggregator.PublishOnUIThread(new ShowSourcesMessage());
		}

		#endregion

		#region ViewModelScreen overrides

		protected override void OnLanguageChanged()
		{
			NotifyOfPropertyChange(nameof(PackageViewModel.DisplayName));
		}

		#endregion
	}
}