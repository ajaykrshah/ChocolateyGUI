// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows
{
	using System;
	using chocolatey.infrastructure.logging;
	using ChocolateyGui.Common.Models;
	using ChocolateyGui.Common.Windows.Services;
	using Serilog;
	using LogMessage = ChocolateyGui.Common.Models.LogMessage;

	public class SerilogLogger : ILog
	{
		#region Declarations

		private readonly ILogger _logger;
		private String _context;
		private Action<LogMessage> _interceptor;
		private readonly IProgressService _progressService;

		#endregion

		#region Constructors

		public SerilogLogger(ILogger logger, IProgressService progressService)
		{
			_logger = logger;
			_progressService = progressService;
		}

		#endregion

		#region Public interface

		public IDisposable Intercept(Action<LogMessage> interceptor)
		{
			return new InterceptMessages(this, interceptor);
		}

		#endregion

		#region ILog implementation

		public void Debug(String message, params Object[] formatting)
		{
			_logger.Debug(message, formatting);

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Debug,
				                 Message = String.Format(message, formatting)
			                 };

			_interceptor?.Invoke(logMessage);
		}

		public void Debug(Func<String> message)
		{
			_logger.Debug(message());

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Debug,
				                 Message = message()
			                 };

			_interceptor?.Invoke(logMessage);
		}

		public void Error(String message, params Object[] formatting)
		{
			_logger.Error(message, formatting);

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Error,
				                 Message = String.Format(message, formatting)
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message, PowerShellLineType.Error);
		}

		public void Error(Func<String> message)
		{
			_logger.Error(message());

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Error,
				                 Message = message()
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message, PowerShellLineType.Error);
		}

		public void Fatal(String message, params Object[] formatting)
		{
			_logger.Fatal(message, formatting);

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Fatal,
				                 Message = String.Format(message, formatting)
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message, PowerShellLineType.Error);
		}

		public void Fatal(Func<String> message)
		{
			_logger.Fatal(message());

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Fatal,
				                 Message = message()
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message, PowerShellLineType.Error);
		}

		public void Info(String message, params Object[] formatting)
		{
			_logger.Information(message, formatting);

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Info,
				                 Message = String.Format(message, formatting)
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message);
		}

		public void Info(Func<String> message)
		{
			_logger.Information(message());

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Info,
				                 Message = message()
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message);
		}

		public void InitializeFor(String loggerName)
		{
			_context = loggerName;
		}

		public void Warn(String message, params Object[] formatting)
		{
			_logger.Warning(message, formatting);

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Warn,
				                 Message = String.Format(message, formatting)
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message, PowerShellLineType.Warning);
		}

		public void Warn(Func<String> message)
		{
			_logger.Warning(message());

			var logMessage = new LogMessage
			                 {
				                 Context = _context,
				                 LogLevel = LogLevel.Warn,
				                 Message = message()
			                 };

			_interceptor?.Invoke(logMessage);
			_progressService.WriteMessage(logMessage.Message, PowerShellLineType.Warning);
		}

		#endregion

		#region InterceptMessages Class

		public class InterceptMessages : IDisposable
		{
			#region Declarations

			private readonly SerilogLogger _logger;

			#endregion

			#region Constructors

			public InterceptMessages(SerilogLogger logger, Action<LogMessage> interceptor)
			{
				_logger = logger;
				logger._interceptor = interceptor;
			}

			#endregion

			#region IDisposable implementation

			public void Dispose()
			{
				_logger._interceptor = null;
			}

			#endregion
		}

		#endregion
	}
}