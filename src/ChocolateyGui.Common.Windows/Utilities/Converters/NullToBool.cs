// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	public class NullToBool : NullToValue
	{
		#region Constructors

		public NullToBool()
		{
			this.TrueValue = true;
			this.FalseValue = false;
		}

		#endregion
	}
}