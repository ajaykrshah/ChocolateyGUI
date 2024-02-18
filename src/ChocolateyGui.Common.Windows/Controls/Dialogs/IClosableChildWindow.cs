// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Controls.Dialogs
{
	using System;

	public interface IClosableChildWindow<TResult>
	{
		Action<TResult> Close { get; set; }
	}
}