// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Controls.Dialogs
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using MahApps.Metro.Controls.Dialogs;

	/// <summary>
	///     A class for manipulating an open ChocolateyDialog.
	/// </summary>
	public class ChocolateyDialogController
	{
		#region Constructors

		internal ChocolateyDialogController(ChocolateyDialog dialog, Func<Task> closeCallBack)
		{
			this.WrappedDialog = dialog;
			this.CloseCallback = closeCallBack;

			this.IsOpen = dialog.IsVisible;

			this.WrappedDialog.PART_NegativeButton.Dispatcher.Invoke(
				() => { this.WrappedDialog.PART_NegativeButton.Click += PART_NegativeButton_Click; });
		}

		#endregion

		#region Delegates

		public delegate void DialogCanceledEventHandler(BaseMetroDialog dialog);

		#endregion

		#region Events

		public event DialogCanceledEventHandler OnCanceled;

		#endregion

		#region Properties

		private Func<Task> CloseCallback { get; }

		/// <summary>
		///     Gets a value indicating whether the Cancel button has been pressed.
		/// </summary>
		public Boolean IsCanceled { get; private set; }

		/// <summary>
		///     Gets a value indicating whether the wrapped ProgressDialog is open.
		/// </summary>
		public Boolean IsOpen { get; private set; }

		private ChocolateyDialog WrappedDialog { get; }

		#endregion

		#region Public interface

		/// <summary>
		///     Begins an operation to close the ProgressDialog.
		/// </summary>
		/// <returns>A task representing the operation.</returns>
		public async Task CloseAsync()
		{
			Action action = () =>
			{
				if (!this.IsOpen)
				{
					throw new InvalidOperationException();
				}

				this.WrappedDialog.Dispatcher.VerifyAccess();
				this.WrappedDialog.PART_NegativeButton.Click -= PART_NegativeButton_Click;
			};

			if (this.WrappedDialog.Dispatcher.CheckAccess())
			{
				action();
			}
			else
			{
				this.WrappedDialog.Dispatcher.Invoke(action);
			}

			await this.CloseCallback();

			this.WrappedDialog.Dispatcher.Invoke(() => { this.IsOpen = false; });
		}

		/// <summary>
		///     Sets if the Cancel button is visible.
		/// </summary>
		/// <param name="value">Default is false</param>
		public void SetCancelable(Boolean value)
		{
			if (this.WrappedDialog.Dispatcher.CheckAccess())
			{
				this.WrappedDialog.IsCancelable = value;
			}
			else
			{
				this.WrappedDialog.Dispatcher.Invoke(
					() => { this.WrappedDialog.IsCancelable = value; });
			}
		}

		/// <summary>
		///     Sets the ProgressBar's IsIndeterminate to true. To set it to false, call SetProgress.
		/// </summary>
		public void SetIndeterminate()
		{
			if (this.WrappedDialog.Dispatcher.CheckAccess())
			{
				this.WrappedDialog.PART_ProgressBar.IsIndeterminate = true;
			}
			else
			{
				this.WrappedDialog.Dispatcher.Invoke(
					() => { this.WrappedDialog.PART_ProgressBar.IsIndeterminate = true; });
			}
		}

		/// <summary>
		///     Sets the dialog's progress bar value and sets IsIndeterminate to false.
		/// </summary>
		/// <param name="value">The percentage to set as the value.</param>
		public void SetProgress(Double value)
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException(nameof(value));
			}

			Action action = () =>
			{
				this.WrappedDialog.PART_ProgressBar.IsIndeterminate = false;
				this.WrappedDialog.PART_ProgressBar.Value = value;
				this.WrappedDialog.PART_ProgressBar.Maximum = 1.0;
				this.WrappedDialog.PART_ProgressBar.ApplyTemplate();
			};

			if (this.WrappedDialog.Dispatcher.CheckAccess())
			{
				action();
			}
			else
			{
				this.WrappedDialog.Dispatcher.Invoke(action);
			}
		}

		/// <summary>
		///     Sets the dialog's title content.
		/// </summary>
		/// <param name="title">
		///     The title.
		/// </param>
		public void SetTitle(String title)
		{
			if (this.WrappedDialog.Dispatcher.CheckAccess())
			{
				this.WrappedDialog.Title = title;
			}
			else
			{
				this.WrappedDialog.Dispatcher.Invoke(() => { this.WrappedDialog.Title = title; });
			}
		}

		#endregion

		#region Private implementation

		private void PART_NegativeButton_Click(Object sender, RoutedEventArgs e)
		{
			if (this.WrappedDialog.Dispatcher.CheckAccess())
			{
				this.IsCanceled = true;
				this.WrappedDialog.PART_NegativeButton.IsEnabled = false;
			}
			else
			{
				this.WrappedDialog.Dispatcher.Invoke(() =>
				{
					this.IsCanceled = true;
					this.WrappedDialog.PART_NegativeButton.IsEnabled = false;
					OnCanceled?.Invoke(this.WrappedDialog);
				});
			}

			// Close();
		}

		#endregion
	}
}