// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Models
{
	using System;

	public struct PackageSearchOptions
	{
		#region Constructors

		public PackageSearchOptions(
			Int32 pageSize,
			Int32 currentPage,
			String sortColumn,
			Boolean includePrerelease,
			Boolean includeAllVersions,
			Boolean matchWord,
			String source)
			: this()
		{
			this.PageSize = pageSize;
			this.CurrentPage = currentPage;
			this.SortColumn = sortColumn;
			this.IncludeAllVersions = includeAllVersions;
			this.IncludePrerelease = includePrerelease;
			this.MatchQuery = matchWord;
			this.Source = source;
		}

		#endregion

		#region Properties

		public Int32 CurrentPage { get; set; }
		public Boolean IncludeAllVersions { get; set; }
		public Boolean IncludePrerelease { get; set; }
		public Boolean MatchQuery { get; set; }
		public Int32 PageSize { get; set; }
		public String SortColumn { get; set; }
		public String Source { get; set; }
		public String[] TagsQuery { get; set; }

		#endregion
	}
}