// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    using SQLite;

    using Windows.Storage;

    public class DbContext
    {
        private readonly string dbFileName;

        public DbContext(string dbFileName = "db.v1.sqlite")
        {
            if (dbFileName == null)
            {
                throw new ArgumentNullException("dbFileName");
            }

            if (Path.IsPathRooted(dbFileName))
            {
                throw new ArgumentException("Path cannot be rooted.", "dbFileName");
            }
            else
            {
                this.dbFileName = Path.Combine(ApplicationData.Current.LocalFolder.Path, dbFileName);
            }
        }

        public SQLiteAsyncConnection CreateConnection()
        {
            return new SQLiteAsyncConnection(this.GetDatabaseFilePath(), storeDateTimeAsTicks: true);
        }

        public async Task InitializeAsync()
        {
            var dbFile = (await ApplicationData.Current.LocalFolder.GetFilesAsync())
                .FirstOrDefault(f => string.Equals(f.Name, this.dbFileName));

            if (dbFile == null)
            {
                var connection = this.CreateConnection();
                await connection.CreateTableAsync<SongMetadata>();
            }
        }

        private string GetDatabaseFilePath()
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, this.dbFileName);
        }
    }
}
