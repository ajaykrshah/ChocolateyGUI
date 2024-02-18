// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Controls
{
	using System;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Media;
	using ChocolateyGui.Common.Controls;
	using ChocolateyGui.Common.Models;

	/// <summary>
	///     Fake PowerShell Output Console
	/// </summary>
	public class FauxPowerShellConsole : RichTextBox
	{
		#region Declarations

		/// <summary>
		///     The container paragraph for all the console's text.
		/// </summary>
		private readonly Paragraph _backingParagraph;

		/// <summary>
		///     Creates a unique identifier for each text line.
		/// </summary>
		private readonly Func<String, String> _getNameHash;

		#endregion

		#region Constructors

		public FauxPowerShellConsole()
			: base(new FlowDocument())
		{
			var hashAlg = MD5.Create();
			_getNameHash =
				unhashed =>
					"_" +
					hashAlg.ComputeHash(Encoding.UTF8.GetBytes(unhashed))
					       .Aggregate(
						       new StringBuilder(),
						       (sb, piece) => sb.Append(piece.ToString("X2", CultureInfo.CurrentCulture)));

			_backingParagraph = new Paragraph();
			this.Document.Blocks.Add(_backingParagraph);
		}

		#endregion

		#region Delegates

		private delegate void RunStringOnUI(PowerShellOutputLine line);

		#endregion

		#region Fields and constants

		/// <summary>
		///     The input buffer the console writes to the text box
		/// </summary>
		public static readonly DependencyProperty BufferCollectionProperty = DependencyProperty.Register(
			"BufferCollectionCollection",
			typeof(ObservableRingBufferCollection<PowerShellOutputLine>),
			typeof(FauxPowerShellConsole),
			new FrameworkPropertyMetadata
			{
				DefaultValue = null,
				PropertyChangedCallback = FauxPowerShellConsole.OnBufferChanged,
				BindsTwoWayByDefault = true,
				DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});

		#endregion

		#region Properties

		public ObservableRingBufferCollection<PowerShellOutputLine> BufferCollection
		{
			get => GetValue<ObservableRingBufferCollection<PowerShellOutputLine>>(FauxPowerShellConsole.BufferCollectionProperty);
			set => SetValue(FauxPowerShellConsole.BufferCollectionProperty, value);
		}

		#endregion

		#region Protected interface

		protected T GetValue<T>(DependencyProperty dependencyProperty)
		{
			return (T)GetValue(dependencyProperty);
		}

		protected void OnBufferUpdated(Object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					Application.Current.Dispatcher.InvokeAsync(() => _backingParagraph.Inlines.Clear());
					break;
				case NotifyCollectionChangedAction.Add:
					foreach (PowerShellOutputLine item in args.NewItems)
					{
						this.Dispatcher.BeginInvoke(
							new RunStringOnUI(
								line =>
								{
#if !DEBUG
                                    if (line.LineType == PowerShellLineType.Debug ||
                                        line.LineType == PowerShellLineType.Verbose)
                                    {
                                        return;
                                    }
#endif // DEBUG
									var runBrushes = FauxPowerShellConsole.GetOutputLineBrush(line);
									var run = new Run
									          {
										          Text = item.Text,
										          Name = _getNameHash(line.Text),
										          Foreground = runBrushes.Item1,
										          Background = runBrushes.Item2
									          };

									if (item.NewLine)
									{
										run.Text += Environment.NewLine;
									}

									_backingParagraph.Inlines.Add(run);
									this.Selection.Select(run.ContentStart, run.ContentEnd);
									ScrollToEnd();
								}),
							item);
					}

					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (PowerShellOutputLine item in args.OldItems)
					{
						this.Dispatcher.BeginInvoke(
							new RunStringOnUI(
								line =>
								{
									var key = _getNameHash(line.Text);
									var run = _backingParagraph.Inlines.FirstOrDefault(inline => inline.Name == key);
									if (run != null)
									{
										_backingParagraph.Inlines.Remove(run);
									}
								}),
							item);
					}

					break;
			}
		}

		#endregion

		#region Private implementation

		private static Tuple<Brush, Brush> GetOutputLineBrush(PowerShellOutputLine line)
		{
			switch (line.LineType)
			{
				case PowerShellLineType.Debug:
				case PowerShellLineType.Verbose:
					return new Tuple<Brush, Brush>(Brushes.Gray, Brushes.Transparent);
				case PowerShellLineType.Warning:
					return new Tuple<Brush, Brush>(Brushes.Yellow, Brushes.Black);
				case PowerShellLineType.Error:
					return new Tuple<Brush, Brush>(Brushes.Red, Brushes.Black);
				default:
					return new Tuple<Brush, Brush>(Brushes.White, Brushes.Transparent);
			}
		}

		private static void OnBufferChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			var pob = d as FauxPowerShellConsole;
			if (pob != null)
			{
				pob.OnBufferChanged(args);
			}
		}

		private void OnBufferChanged(DependencyPropertyChangedEventArgs args)
		{
			// If we had a previous buffer, clear our event holder.
			if (args.OldValue != null)
			{
				((ObservableRingBufferCollection<PowerShellOutputLine>)args.OldValue).CollectionChanged -=
					OnBufferUpdated;
			}

			var newBuffer = (ObservableRingBufferCollection<PowerShellOutputLine>)args.NewValue;
			newBuffer.CollectionChanged += OnBufferUpdated;

			// Reset the current console.
			OnBufferUpdated(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			var bufferItems = newBuffer.ToList();

			// Add in any lines written to the buffer.
			OnBufferUpdated(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, bufferItems));
		}

		#endregion
	}
}