// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common
{
	using System;
	using Serilog.Core;
	using Serilog.Events;

	public sealed class ColouredConsoleSink : ILogEventSink
	{
		#region ILogEventSink implementation

		public void Emit(LogEvent logEvent)
		{
			try
			{
				switch (logEvent.Level)
				{
					case LogEventLevel.Error:
						Console.ForegroundColor = ConsoleColor.Red;
						break;
					case LogEventLevel.Warning:
						Console.ForegroundColor = ConsoleColor.Yellow;
						break;
				}

				logEvent.RenderMessage(Console.Out);
				Console.WriteLine();
			}
			finally
			{
				Console.ResetColor();
			}
		}

		#endregion
	}
}