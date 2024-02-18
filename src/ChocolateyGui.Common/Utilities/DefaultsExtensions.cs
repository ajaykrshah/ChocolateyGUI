// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

#if !DEBUG
using System;
#endif

namespace ChocolateyGui.Common.Utilities
{
	using Serilog;
	using Serilog.Events;

	public static class DefaultsExtensions
	{
		#region Public interface

		public static LoggerConfiguration SetDefaultLevel(this LoggerConfiguration loggerConfig, LogEventLevel defaultLevel = LogEventLevel.Information)
		{
#if DEBUG
			return loggerConfig.MinimumLevel.Debug();
#else
			var logLevel = Environment.GetEnvironmentVariable("CHOCOLATEYGUI__LOGLEVEL");
			LogEventLevel logEventLevel;
			if (String.IsNullOrWhiteSpace(logLevel) || !Enum.TryParse(logLevel, true, out logEventLevel))
			{
				loggerConfig.MinimumLevel.Is(defaultLevel);
			}
			else
			{
				loggerConfig.MinimumLevel.Is(logEventLevel);
			}

			return loggerConfig;
#endif
		}

		#endregion
	}
}