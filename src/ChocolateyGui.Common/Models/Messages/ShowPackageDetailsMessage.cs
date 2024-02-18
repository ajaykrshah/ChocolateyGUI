// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models.Messages
{
	using ChocolateyGui.Common.ViewModels.Items;

	public class ShowPackageDetailsMessage
	{
		#region Constructors

		public ShowPackageDetailsMessage(IPackageViewModel package)
		{
			this.Package = package;
		}

		#endregion

		#region Properties

		public IPackageViewModel Package { get; }

		#endregion
	}
}