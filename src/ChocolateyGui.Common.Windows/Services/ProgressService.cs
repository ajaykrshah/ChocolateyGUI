// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Caliburn.Micro;
	using ChocolateyGui.Common.Base;
	using ChocolateyGui.Common.Controls;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Windows.Controls.Dialogs;
	using ChocolateyGui.Common.Windows.Utilities.Extensions;
	using ChocolateyGui.Common.Windows.Views;
	using Microsoft.VisualStudio.Threading;
	using Serilog;
	using Serilog.Events;

	public class ProgressService : ObservableBase, IProgressService
	{
		#region Declarations

		private readonly AsyncSemaphore _lock;
		private CancellationTokenSource _cst;
		private Int32 _loadingItems;
		private ChocolateyDialogController _progressController;

		#endregion

		#region Constructors

		public ProgressService()
		{
			this.IsLoading = false;
			_loadingItems = 0;
			this.Output = new ObservableRingBufferCollection<PowerShellOutputLine>(100);
			_lock = new AsyncSemaphore(1);
		}

		#endregion

		#region Properties

		public Double Progress { get; private set; }

		#endregion

		#region IProgress<double> implementation

		public void Report(Double value)
		{
			this.Progress = value;

			if (_progressController != null)
			{
				if (value < 0)
				{
					Execute.OnUIThread(() => _progressController.SetIndeterminate());
				}
				else
				{
					Execute.OnUIThread(() => _progressController.SetProgress(Math.Min(this.Progress / 100.0f, 100)));
				}
			}

			NotifyPropertyChanged("Progress");
		}

		#endregion

		#region IProgressService implementation

		public CancellationToken GetCancellationToken()
		{
			if (!this.IsLoading)
			{
				throw new InvalidOperationException("There's no current operation in process.");
			}

			return _cst.Token;
		}

		public Boolean IsLoading { get; private set; }
		public ObservableRingBufferCollection<PowerShellOutputLine> Output { get; }
		public ShellView ShellView { get; set; }

		public async Task StartLoading(String title = null, Boolean isCancelable = false)
		{
			using (await _lock.EnterAsync())
			{
				var currentCount = Interlocked.Increment(ref _loadingItems);
				if (currentCount == 1)
				{
					await ProgressService.RunOnUIAsync(async () =>
					{
						_progressController = await this.ShellView.ShowChocolateyDialogAsync(title, isCancelable);
						_progressController.SetIndeterminate();
						if (isCancelable)
						{
							_cst = new CancellationTokenSource();
							_progressController.OnCanceled += dialog =>
							{
								if (_cst != null)
								{
									_cst.Cancel();
								}
							};
						}

						this.Output.Clear();

						this.IsLoading = true;
						NotifyPropertyChanged("IsLoading");
					});
				}
			}
		}

		public async Task StopLoading()
		{
			using (await _lock.EnterAsync())
			{
				var currentCount = Interlocked.Decrement(ref _loadingItems);
				if (currentCount == 0)
				{
					await _progressController.CloseAsync();
					_progressController = null;
					Report(0);

					this.IsLoading = false;
					NotifyPropertyChanged("IsLoading");
				}
			}
		}

		public void WriteMessage(
			String message,
			PowerShellLineType type = PowerShellLineType.Output,
			Boolean newLine = true)
		{
			// Don't show debug events when not running in debug.
			if (type == PowerShellLineType.Debug && !Log.IsEnabled(LogEventLevel.Debug))
			{
				return;
			}

			Execute.BeginOnUIThread(() => this.Output.Add(new PowerShellOutputLine(message, type, newLine)));
		}

		#endregion

		#region Private implementation

		private static Task RunOnUIAsync(Func<Task> action)
		{
			return action.RunOnUIThreadAsync();
		}

		#endregion
	}
}