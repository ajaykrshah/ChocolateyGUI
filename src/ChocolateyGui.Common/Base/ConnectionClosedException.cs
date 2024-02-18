// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Base
{
	using System;

	/// <summary>
	///     Used to represent that a connection closed peacefully while performing operation. Likely means application is
	///     closing.
	/// </summary>
	public class ConnectionClosedException : Exception
	{
	}
}