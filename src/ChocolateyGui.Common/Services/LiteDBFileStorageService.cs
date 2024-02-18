// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Services
{
	using LiteDB;

	public class LiteDBFileStorageService : IFileStorageService
	{
		#region Declarations

		private readonly LiteDatabase _database;

		#endregion

		#region Constructors

		public LiteDBFileStorageService(LiteDatabase database)
		{
			_database = database;
		}

		#endregion

		#region IFileStorageService implementation

		public void DeleteAllFiles()
		{
			var files = _database.FileStorage.FindAll();

			foreach (var file in files)
			{
				_database.FileStorage.Delete(file.Id);
			}
		}

		#endregion
	}
}