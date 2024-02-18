// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Input;
	using ChocolateyGui.Common.Base;
	using Serilog;

	public sealed class NotifyTaskCompletion<TResult> : ObservableBase
	{
		#region Declarations

		private static readonly ILogger Logger = Log.ForContext<NotifyTaskCompletion<TResult>>();

		#endregion

		#region Constructors

		public NotifyTaskCompletion(Task<TResult> task)
		{
			this.Task = task;
			if (!task.IsCompleted)
			{
#pragma warning disable IDE0059 // Unnecessary assignment of a value
				var watchTask = WatchTaskAsync(task);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			}
		}

		#endregion

		#region Properties

		public String ErrorMessage => this.InnerException?.Message;
		public AggregateException Exception => this.Task.Exception;
		public Exception InnerException => this.Exception?.InnerException;
		public Boolean IsCanceled => this.Task.IsCanceled;
		public Boolean IsCompleted => this.Task.IsCompleted;
		public Boolean IsFaulted => this.Task.IsFaulted;
		public Boolean IsNotCompleted => !this.Task.IsCompleted;
		public Boolean IsSuccessfullyCompleted => this.Task.Status == TaskStatus.RanToCompletion;
		public TResult Result => this.Task.Status == TaskStatus.RanToCompletion ? this.Task.Result : default;
		public TaskStatus Status => this.Task.Status;
		public Task<TResult> Task { get; }

		#endregion

		#region Private implementation

		private async Task WatchTaskAsync(Task task)
		{
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				NotifyTaskCompletion<TResult>.Logger.Error(ex, "Ran into an error while executing a task.");
			}

			NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.Status));
			NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.IsCompleted));
			NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.IsNotCompleted));

			if (task.IsCanceled)
			{
				NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.IsCanceled));
			}
			else if (task.IsFaulted)
			{
				NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.IsFaulted));
				NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.Exception));
				NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.InnerException));
				NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.ErrorMessage));
			}
			else
			{
				NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.IsSuccessfullyCompleted));
				NotifyPropertyChanged(nameof(NotifyTaskCompletion<TResult>.Result));
			}

			CommandManager.InvalidateRequerySuggested();
		}

		#endregion
	}
}