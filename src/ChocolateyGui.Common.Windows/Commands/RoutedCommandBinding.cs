﻿// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Commands
{
	using System;
	using System.Windows;
	using System.Windows.Input;

	/// <summary>
	///     The base class for <see cref="CommandBinding" /> types that invoke command logic in
	///     locations other than the code behind file.
	/// </summary>
	public abstract class RoutedCommandBinding : CommandBinding
	{
		#region Constructors

		static RoutedCommandBinding()
		{
			RoutedCommandMonitor.Init();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RoutedCommandBinding" /> class.
		/// </summary>
		protected RoutedCommandBinding()
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RoutedCommandBinding" /> class by
		///     using the specified <see cref="ICommand" />.
		/// </summary>
		/// <param name="command">
		///     The command that is to be bound.
		/// </param>
		protected RoutedCommandBinding(ICommand command)
			: base(command)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		///     Gets or sets a value indicating whether or not the methods associated with this
		///     <see cref="RoutedCommandBinding" /> will be executed when the Handled property
		///     of the <see cref="RoutedEventArgs" /> is set to true during the bubbling or
		///     tunneling of the command's <see cref="RoutedEvent" />.
		/// </summary>
		public Boolean ViewHandledEvents { get; set; }

		#endregion

		#region Protected interface

		/// <summary>
		///     The method that is called when the CanExecute <see cref="RoutedEvent" /> for the
		///     <see cref="ICommand" /> associated with this <see cref="RoutedCommandBinding" />
		///     should be handled. Inheriting types must provide an implementation for this method.
		/// </summary>
		/// <param name="sender">The command target on which the command is executing.</param>
		/// <param name="e">The event data.</param>
		protected internal abstract void OnCanExecute(Object sender, CanExecuteRoutedEventArgs e);

		/// <summary>
		///     The method that is called when the Executed <see cref="RoutedEvent" /> for the
		///     <see cref="ICommand" /> associated with this <see cref="RoutedCommandBinding" />
		///     should be handled. Inheriting types must provide an implementation for this method.
		/// </summary>
		/// <param name="sender">The command target on which the command is executing.</param>
		/// <param name="e">The event data.</param>
		protected internal abstract void OnExecuted(Object sender, ExecutedRoutedEventArgs e);

		/// <summary>
		///     The method that is called when the PreviewCanExecute <see cref="RoutedEvent" /> for
		///     the <see cref="ICommand" /> associated with this <see cref="RoutedCommandBinding" />
		///     should be handled. Inheriting types must provide an implementation for this method.
		/// </summary>
		/// <param name="sender">The command target on which the command is executing.</param>
		/// <param name="e">The event data.</param>
		protected internal abstract void OnPreviewCanExecute(Object sender, CanExecuteRoutedEventArgs e);

		/// <summary>
		///     The method that is called when the PreviewExecuted <see cref="RoutedEvent" /> for
		///     the <see cref="ICommand" /> associated with this <see cref="RoutedCommandBinding" />
		///     should be handled. Inheriting types must provide an implementation for this method.
		/// </summary>
		/// <param name="sender">The command target on which the command is executing.</param>
		/// <param name="e">The event data.</param>
		protected internal abstract void OnPreviewExecuted(Object sender, ExecutedRoutedEventArgs e);

		#endregion
	}
}