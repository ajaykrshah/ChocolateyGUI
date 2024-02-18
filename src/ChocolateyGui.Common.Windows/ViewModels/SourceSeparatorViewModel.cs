// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.ViewModels
{
	using System;
	using ChocolateyGui.Common.ViewModels;

	public sealed class SourceSeparatorViewModel : ISourceViewModelBase
	{
		#region Declarations

		private const String DISPLAYNAME = "Separator";

		#endregion

		#region Constructors

		public SourceSeparatorViewModel()
		{
			this.DisplayName = SourceSeparatorViewModel.DISPLAYNAME;
		}

		#endregion

		#region ISourceViewModelBase implementation

		public String DisplayName { get; }

		#endregion
	}
}