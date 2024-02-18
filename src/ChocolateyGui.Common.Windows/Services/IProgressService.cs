// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading;
	using System.Threading.Tasks;
	using ChocolateyGui.Common.Controls;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Windows.Views;

	public interface IProgressService : INotifyPropertyChanged, IProgress<Double>
	{
		Boolean IsLoading { get; }
		ObservableRingBufferCollection<PowerShellOutputLine> Output { get; }
		ShellView ShellView { get; set; }

		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate")]
		CancellationToken GetCancellationToken();

		Task StartLoading(String title = null, Boolean isCancelable = false);
		Task StopLoading();
		void WriteMessage(String message, PowerShellLineType type = PowerShellLineType.Output, Boolean newLine = true);
	}
}