// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Utilities.Converters
{
	public class NullToInverseBool : NullToValue
	{
		#region Constructors

		public NullToInverseBool()
		{
			this.TrueValue = false;
			this.FalseValue = true;
		}

		#endregion
	}
}