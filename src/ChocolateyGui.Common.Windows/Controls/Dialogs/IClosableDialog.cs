// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Controls.Dialogs
{
	using System.Threading.Tasks;

	public interface IClosableDialog<TResult>
	{
		Task<TResult> WaitForClosingAsync();
	}
}