// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Commands
{
	using System;
	using System.Windows.Input;

	public class RelayCommand : ICommand
	{
		#region Declarations

		private readonly Predicate<Object> _canExecute;
		private readonly Action<Object> _execute;

		#endregion

		#region Constructors

		public RelayCommand(Action<Object> execute)
			: this(execute, null)
		{
		}

		public RelayCommand(Action<Object> execute, Predicate<Object> canExecute)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand implementation

		public Boolean CanExecute(Object parameter)
		{
			return _canExecute is null || _canExecute(parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public void Execute(Object parameter)
		{
			_execute?.Invoke(parameter);
		}

		#endregion
	}
}