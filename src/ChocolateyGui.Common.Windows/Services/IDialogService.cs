// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Services
{
	using System;
	using System.Threading.Tasks;
	using ChocolateyGui.Common.Windows.Controls.Dialogs;
	using ChocolateyGui.Common.Windows.Views;
	using MahApps.Metro.Controls.Dialogs;

	public interface IDialogService
	{
		ShellView ShellView { get; set; }
		event EventHandler<Object> ChildWindowClosed;
		event EventHandler<Object> ChildWindowOpened;

		/// <summary>
		///     Creates a Custom child window inside of the ShellView.
		/// </summary>
		/// <param name="title">The title of the child window.</param>
		/// <param name="dialogContent">The content within the child window.</param>
		/// <param name="dialogContext">The context of the content within the child window.</param>
		/// <typeparam name="TDialogContext">The type of the context.</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
		Task<TResult> ShowChildWindowAsync<TDialogContext, TResult>(String title, Object dialogContent, TDialogContext dialogContext)
			where TDialogContext : IClosableChildWindow<TResult>;

		/// <summary>
		///     Creates a Message dialog with Yes/No buttons inside of the ShellView.
		/// </summary>
		/// <param name="title">The title of the Dialog.</param>
		/// <param name="message">The message contained within the Dialog.</param>
		/// <returns>A task promising the result of which button was pressed.</returns>
		Task<MessageDialogResult> ShowConfirmationMessageAsync(String title, String message);

		/// <summary>
		///     Creates a Custom dialog inside of the ShellView.
		/// </summary>
		/// <param name="title">The title of the Dialog.</param>
		/// <param name="dialogContent">The content within the Dialog.</param>
		/// <param name="dialogContext">The context of the content within the Dialog.</param>
		/// <param name="settings">Optional settings that override the global dialog settings.</param>
		/// <typeparam name="TDialogContext">The type of the context.</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
		Task<TResult> ShowDialogAsync<TDialogContext, TResult>(String title, Object dialogContent, TDialogContext dialogContext, MetroDialogSettings settings = null)
			where TDialogContext : IClosableDialog<TResult>;

		/// <summary>
		///     Creates a Login dialog inside of the ShellView.
		/// </summary>
		/// <param name="title">The title of the Dialog.</param>
		/// <param name="message">The message contained within the Dialog.</param>
		/// <param name="settings">Optional settings that override the global dialog settings.</param>
		/// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
		Task<LoginDialogData> ShowLoginAsync(String title, String message, LoginDialogSettings settings = null);

		/// <summary>
		///     Creates a Message dialog with an OK button inside of the ShellView.
		/// </summary>
		/// <param name="title">The title of the Dialog.</param>
		/// <param name="message">The message contained within the Dialog.</param>
		/// <returns>A task promising the result of which button was pressed.</returns>
		Task<MessageDialogResult> ShowMessageAsync(String title, String message);
	}
}