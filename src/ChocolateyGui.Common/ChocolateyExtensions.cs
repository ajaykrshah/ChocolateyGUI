// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using chocolatey;
	using chocolatey.infrastructure.results;

	public static class ChocolateyExtensions
	{
		#region Public interface

		public static Task<ICollection<T>> ListAsync<T>(this GetChocolatey chocolatey)
		{
			return Task.Run(() => (ICollection<T>)chocolatey.List<T>().ToList());
		}

		public static Task<ICollection<PackageResult>> ListPackagesAsync(this GetChocolatey chocolatey)
		{
			return Task.Run(() => (ICollection<PackageResult>)chocolatey.List<PackageResult>().ToList());
		}

		public static Task RunAsync(this GetChocolatey chocolatey)
		{
			return Task.Run(() => chocolatey.Run());
		}

		#endregion
	}
}