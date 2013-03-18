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
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    using SQLite;

#if NETFX_CORE
    using Windows.Storage;
#endif

    public class DbContext
    {
        private readonly string dbFileName;

        public DbContext(string dbFileName = "db.sqlite")
        {
            if (dbFileName == null)
            {
                throw new ArgumentNullException("dbFileName");
            }

            if (Path.IsPathRooted(dbFileName))
            {
                throw new ArgumentException("Path to database cannot be rooted. It should be relative path to base (local) folder.", "dbFileName");
            }
            else
            {
                this.dbFileName = dbFileName;
            }
        }

        public enum DatabaseStatus
        {
            Unknown = 0,
            New = 1,
            Existed = 2
        }

        public SQLiteAsyncConnection CreateConnection()
        {
            return new SQLiteAsyncConnection(this.GetDatabaseFilePath(), openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.NoMutex, storeDateTimeAsTicks: true);
        }

        public async Task<DatabaseStatus> InitializeAsync()
        {
            bool fDbExists = false;

#if NETFX_CORE
            fDbExists = (await ApplicationData.Current.LocalFolder.GetFilesAsync())
                .Any(f => string.Equals(f.Name, this.dbFileName));
#else
            fDbExists = File.Exists(this.GetDatabaseFilePath());
#endif
            SQLite3.Config(SQLite3.ConfigOption.MultiThread);

            if (!fDbExists)
            {
                var connection = this.CreateConnection();
                await connection.ExecuteAsync("PRAGMA page_size = 65536 ;");
                await connection.ExecuteAsync("PRAGMA user_version = 1 ;");

                await connection.CreateTableAsync<SongEntity>();
                await connection.CreateTableAsync<UserPlaylistEntity>();
                await connection.CreateTableAsync<UserPlaylistEntryEntity>();
                await connection.CreateTableAsync<AlbumEntity>();
                await connection.CreateTableAsync<GenreEntity>();
                await connection.CreateTableAsync<ArtistEntity>();
            }

            return fDbExists ? DatabaseStatus.Existed : DatabaseStatus.New;
        }

        public async Task DeleteDatabaseAsync()
        {
#if NETFX_CORE
            var dbFile = (await ApplicationData.Current.LocalFolder.GetFilesAsync())
                .FirstOrDefault(f => string.Equals(f.Name, this.dbFileName));

            if (dbFile != null)
            {
                await dbFile.DeleteAsync();
            }
#else
            await Task.Run(
                () =>
                    {
                        var databaseFilePath = this.GetDatabaseFilePath();
                        if (File.Exists(databaseFilePath))
                        {
                            File.Delete(databaseFilePath);
                        }
                    });
#endif
        }

        private string GetDatabaseFilePath()
        {
#if NETFX_CORE
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, this.dbFileName);
#else
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.dbFileName);
#endif
        }
    }
}
