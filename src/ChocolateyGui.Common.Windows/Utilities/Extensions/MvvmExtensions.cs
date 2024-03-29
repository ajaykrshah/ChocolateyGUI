﻿// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Extensions
{
	using System;
	using System.ComponentModel;
	using System.Linq.Expressions;
	using System.Reactive.Linq;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
	using System.Windows;
	using Caliburn.Micro;
	using Action = System.Action;

	public static class MvvmExtensions
	{
		#region Public interface

		public static async Task RunOnUIThreadAsync(this Func<Task> func)
		{
			var tcs = new TaskCompletionSource<Boolean>();
			Action action = async () =>
			{
				try
				{
					await func();
					tcs.SetResult(true);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			};
			await action.OnUIThreadAsync();
			await tcs.Task;
		}

		public static async Task<T> RunOnUIThreadAsync<T>(this Func<Task<T>> func)
		{
			var tcs = new TaskCompletionSource<T>();
			Action action = async () =>
			{
				try
				{
					var result = await func();
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			};
			await action.OnUIThreadAsync();
			return await tcs.Task;
		}

		public static Boolean SetPropertyValue<T>(this PropertyChangedBase @base, ref T property, T value, [CallerMemberName] String propertyName = "")
		{
			if (object.Equals(property, value))
			{
				return false;
			}

			property = value;
			@base.NotifyOfPropertyChange(propertyName);
			return true;
		}

		public static IObservable<TType> ToObservable<TType, TObject>(this TObject dependencyObject, DependencyProperty property, Expression<Func<TType>> propertyExpression)
			where TObject : DependencyObject
		{
			return MvvmExtensions.ToObservable<TType, TObject>(dependencyObject, property);
		}

		public static IObservable<TType> ToObservable<TType, TObject>(this TObject dependencyObject, DependencyProperty property)
			where TObject : DependencyObject
		{
			if (property.PropertyType != typeof(TType))
			{
				throw new ArgumentException($"ToObservable expected \"{property.PropertyType}\", but got \"{typeof(TType)}\" instead.");
			}

			return Observable.Create<TType>(o =>
			{
				var des = DependencyPropertyDescriptor.FromProperty(property, typeof(TObject));
				var eh = new EventHandler((s, e) => o.OnNext((TType)des.GetValue(dependencyObject)));
				des.AddValueChanged(dependencyObject, eh);
				return () => des.RemoveValueChanged(dependencyObject, eh);
			});
		}

		#endregion
	}
}