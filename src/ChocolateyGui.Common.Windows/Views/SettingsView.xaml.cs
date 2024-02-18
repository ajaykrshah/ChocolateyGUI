// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Views
{
	using Caliburn.Micro;
	using ChocolateyGui.Common.Models.Messages;

	/// <summary>
	///     Interaction logic for SettingsView.xaml
	/// </summary>
	public partial class SettingsView : IHandle<SourcesUpdatedMessage>
	{
		#region Constructors

		public SettingsView(IEventAggregator eventAggregator)
		{
			eventAggregator.Subscribe(this);
			InitializeComponent();
		}

		#endregion

		#region IHandle<SourcesUpdatedMessage> implementation

		public void Handle(SourcesUpdatedMessage message)
		{
			SourcesGrid.Items.Refresh();
		}

		#endregion
	}
}