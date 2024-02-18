// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Commands
{
	using System;
	using System.Reflection;
	using System.Windows.Input;
	using System.Windows.Markup;
	using ChocolateyGui.Common.Windows.Utilities;

	/// <summary>
	///     A markup extension that returns an <see cref="ICommand" /> that is capable of executing
	///     methods of the DataContext of a target FrameworkElement.
	/// </summary>
	/// <remarks>
	///     When the <see cref="ICommand.Execute" /> and <see cref="ICommand.CanExecute" /> methods
	///     of the returned <see cref="ICommand" /> object are invoked, methods on the DataContext
	///     whose names correspond to the values of the <see cref="Executed" /> and
	///     <see cref="CanExecute" /> properties are invoked. See the <see cref="Executed" /> and
	///     <see cref="CanExecute" /> properties for specifics on the allowable method signatures.
	/// </remarks>
	public sealed class DataContextCommandAdapter : MarkupExtension, ICommand
	{
		#region Declarations

		private Object _target;

		#endregion

		#region Constructors

		/// <summary>
		///     Initializes a new instance of the <see cref="DataContextCommandAdapter" /> class.
		/// </summary>
		public DataContextCommandAdapter()
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DataContextCommandAdapter" /> class by
		///     using the specified method name for the <see cref="Executed" /> property.
		/// </summary>
		/// <param name="executed">
		///     The name of the <see cref="Executed" /> method.
		/// </param>
		public DataContextCommandAdapter(String executed)
		{
			this.Executed = executed;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="DataContextCommandAdapter" /> class by
		///     using the specified method names for the <see cref="Executed" /> and
		///     <see cref="CanExecute" /> properties.
		/// </summary>
		/// <param name="executed">
		///     The name of the <see cref="Executed" /> method.
		/// </param>
		/// <param name="canExecute">
		///     The name of the <see cref="CanExecute" /> method.
		/// </param>
		public DataContextCommandAdapter(String executed, String canExecute)
		{
			this.Executed = executed;
			this.CanExecute = canExecute;
		}

		#endregion

		#region Properties

		/// <summary>
		///     Gets or sets the name of the method of the target object's DataContext that determines whether the
		///     command can execute in its current state.
		/// </summary>
		/// <remarks>
		///     The corresponding method must have one of two signatures below, with the first
		///     taking precedence over the other:
		///     <code>void MyCanExecuteMethod(object parameter);</code>
		///     <code>void MyCanExecuteMethod();</code>
		/// </remarks>
		public String CanExecute { get; set; }

		/// <summary>
		///     Gets or sets the Name of the method of the target object's DataContext to be called when the command
		///     is invoked.
		/// </summary>
		/// <remarks>
		///     The corresponding method must have one of two signatures below, with the first
		///     taking precedence over the other:
		///     <code>void MyExecutedMethod(object parameter);</code>
		///     <code>void MyExecutedMethod();</code>
		/// </remarks>
		public String Executed { get; set; }

		#endregion

		#region MarkupExtension overrides

		/// <summary>
		///     Returns an <see cref="ICommand" /> that is capable of executing methods of the
		///     DataContext of the target.
		/// </summary>
		/// <param name="serviceProvider">
		///     Object that can provide services for the markup extension.
		/// </param>
		/// <returns>
		///     The <see cref="ICommand" /> object.
		/// </returns>
		public override Object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}

			var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
			if (target == null)
			{
				throw new Exception("IProvideValueTarget could not be resolved.");
			}

			_target =
				target.TargetObject is InputBinding
					? DataContextCommandAdapter.GetInputBindingsCollectionOwner(target)
					: target.TargetObject;

			return this;
		}

		#endregion

		#region ICommand implementation

		Boolean ICommand.CanExecute(Object parameter)
		{
			var target = DataContext.GetDataContext(_target);
			if (_target == null)
			{
				return false;
			}

			Boolean canExecute;
			return
				CommandExecutionManager.TryExecuteCommand(target, parameter, false, this.Executed, this.CanExecute, out canExecute) &&
				canExecute;
		}

		event EventHandler ICommand.CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		void ICommand.Execute(Object parameter)
		{
			var target = DataContext.GetDataContext(_target);
			if (_target == null)
			{
				return;
			}

			Boolean canExecute;
			CommandExecutionManager.TryExecuteCommand(target, parameter, true, this.Executed, this.CanExecute, out canExecute);
		}

		#endregion

		#region Private implementation

		// This method only works with the C# 4.0 XamlParser.
		// If there was another way to do this without reflection... I would do it that way
		// Regardless, this method will only be called once when the xaml is initially parsed, so its
		// not really a performance issue.
		private static Object GetInputBindingsCollectionOwner(IProvideValueTarget targetService)
		{
			var xamlContextField = targetService.GetType()
			                                    .GetField("_xamlContext", BindingFlags.Instance | BindingFlags.NonPublic);
			if (xamlContextField == null)
			{
				return null;
			}

			var xamlContext = xamlContextField.GetValue(targetService);
			var grandParentInstanceProperty = xamlContext.GetType().GetProperty("GrandParentInstance");
			if (grandParentInstanceProperty == null)
			{
				return null;
			}

			var inputBindingsCollection = grandParentInstanceProperty.GetGetMethod().Invoke(xamlContext, null);
			var ownerField = inputBindingsCollection.GetType()
			                                        .GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic);
			if (ownerField == null)
			{
				return null;
			}

			var owner = ownerField.GetValue(inputBindingsCollection);
			return owner;
		}

		#endregion
	}
}