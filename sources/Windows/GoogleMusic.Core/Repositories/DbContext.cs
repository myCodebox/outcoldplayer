// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    using SQLite;

#if NETFX_CORE
    using Windows.Storage;
#endif

    public class DbContext
    {
        private const int CurrentDatabaseVersion = 6;
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
            Updated = 2,
            Existed = 3
        }

        public SQLiteAsyncConnection CreateConnection()
        {
            return new SQLiteAsyncConnection(this.GetDatabaseFilePath(), openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.NoMutex, storeDateTimeAsTicks: true);
        }

        public async Task<DatabaseUpdateInformation> InitializeAsync(bool forceToUpdate)
        {
            bool fDbExists = false;

#if NETFX_CORE
            fDbExists = (await ApplicationData.Current.LocalFolder.GetFilesAsync())
                .Any(f => string.Equals(f.Name, this.dbFileName));
#else
            fDbExists = File.Exists(this.GetDatabaseFilePath());
#endif
            SQLite3.Config(SQLite3.ConfigOption.MultiThread);

            int currentVersion = -1;

            SQLiteAsyncConnection connection = null;

            if (fDbExists)
            {
                connection = this.CreateConnection();
                currentVersion = await connection.ExecuteScalarAsync<int>("PRAGMA user_version");

                if (currentVersion == CurrentDatabaseVersion && !forceToUpdate)
                {
                    return new DatabaseUpdateInformation(CurrentDatabaseVersion, DatabaseStatus.Existed);
                }
            }

            bool versionUpdated = false;

            if ((currentVersion >= 2 && currentVersion < CurrentDatabaseVersion)
                && !forceToUpdate)
            {
                await this.DropAllTriggersAsync(connection);

                if (currentVersion <= 2)
                {
                    await this.Update2Async(connection);
                }

                if (currentVersion <= 3)
                {
                    await this.Update3Async(connection);
                }

                if (currentVersion <= 4)
                {
                    await this.Update4Async(connection);
                    forceToUpdate = true;
                }

                if (currentVersion <= 5)
                {
                    forceToUpdate = true;
                }

                versionUpdated = true;
            }

            if (!versionUpdated || forceToUpdate)
            {
                currentVersion = -1;

                if (connection != null)
                {
                    connection.Close();
                    await this.DeleteDatabaseAsync();
                }

                connection = this.CreateConnection();
                await this.CreateBasicObjectsAsync(connection);
            }

            if (connection != null)
            {
                await this.CreateTriggersAsync(connection);
                await connection.ExecuteAsync(string.Format(CultureInfo.InvariantCulture, "PRAGMA user_version = {0} ;", CurrentDatabaseVersion));
            }

            return new DatabaseUpdateInformation(currentVersion, currentVersion == -1 ? DatabaseStatus.New : DatabaseStatus.Updated);
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

        private async Task Update2Async(SQLiteAsyncConnection connection)
        {
            await connection.ExecuteAsync("alter table Song add column IsLibrary;");
            await connection.ExecuteAsync("update Song set IsLibrary = 1;");
        }

        private async Task Update3Async(SQLiteAsyncConnection connection)
        {
            await connection.ExecuteAsync(@"
update Song
set TitleNorm = case when TitleNorm like '@THE@ @%' then substr(TitleNorm, 7) else TitleNorm end,
    AlbumArtistTitleNorm = case when AlbumArtistTitleNorm like '@THE@ @%' then substr(AlbumArtistTitleNorm, 7) else AlbumArtistTitleNorm end,    
    ArtistTitleNorm = case when ArtistTitleNorm like '@THE@ @%' then substr(ArtistTitleNorm, 7) else ArtistTitleNorm end,    
    AlbumTitleNorm = case when AlbumTitleNorm like '@THE@ @%' then substr(AlbumTitleNorm, 7) else AlbumTitleNorm end,    
    GenreTitleNorm = case when GenreTitleNorm like '@THE@ @%' then substr(GenreTitleNorm, 7) else
            GenreTitleNorm end;
");

            await connection.ExecuteAsync(@"
update Album
set TitleNorm = case when TitleNorm like '@THE@ @%' then substr(TitleNorm, 7) else TitleNorm end,
    ArtistTitleNorm = case when ArtistTitleNorm like '@THE@ @%' then substr(ArtistTitleNorm, 7) else ArtistTitleNorm end,    
    GenreTitleNorm = case when GenreTitleNorm like '@THE@ @%' then substr(GenreTitleNorm, 7) else GenreTitleNorm end;
");

            await connection.ExecuteAsync(@"
update Artist
set TitleNorm = case when TitleNorm like '@THE@ @%' then substr(TitleNorm, 7) else TitleNorm end;
");

            await connection.ExecuteAsync(@"
update Genre
set TitleNorm = case when TitleNorm like '@THE@ @%' then substr(TitleNorm, 7) else TitleNorm end;
");

            await connection.ExecuteAsync(@"
update UserPlaylist
set TitleNorm = case when TitleNorm like '@THE@ @%' then substr(TitleNorm, 7) else TitleNorm end;
");
        }

        private async Task Update4Async(SQLiteAsyncConnection connection)
        {
            await connection.ExecuteAsync("alter table Song add column StoreId;");
        }

        private async Task CreateBasicObjectsAsync(SQLiteAsyncConnection connection)
        {
            await connection.ExecuteAsync("PRAGMA page_size = 65536 ;");

            await connection.CreateTableAsync<Song>();
            await connection.CreateTableAsync<UserPlaylist>();
            await connection.CreateTableAsync<UserPlaylistEntry>();
            await connection.CreateTableAsync<Album>();
            await connection.CreateTableAsync<Genre>();
            await connection.CreateTableAsync<Artist>();
            await connection.CreateTableAsync<CachedSong>();
            await connection.CreateTableAsync<CachedAlbumArt>();

            await connection.ExecuteAsync(@"CREATE TABLE [Enumerator] (Id integer primary key autoincrement not null);");
            await connection.ExecuteAsync(@"INSERT INTO [Enumerator] DEFAULT VALUES;");
        }

        private async Task DropAllTriggersAsync(SQLiteAsyncConnection connection)
        {
            var triggers = await connection.QueryAsync<SqliteMasterRecord>(@"select name from sqlite_master where type = 'trigger'");
            foreach (var sqliteMasterRecord in triggers)
            {
                await connection.ExecuteAsync(string.Format(CultureInfo.InvariantCulture, "drop trigger {0};", sqliteMasterRecord.Name));
            }
        }

        private async Task CreateTriggersAsync(SQLiteAsyncConnection connection)
        {
            await connection.ExecuteAsync(@"
CREATE TRIGGER insert_song AFTER INSERT ON Song 
  BEGIN

    update [Genre]
    set 
        [SongsCount] = [Genre].[SongsCount] + 1,
        [Duration] = [Genre].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Genre].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Genre].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Genre].[LastPlayed] then new.[LastPlayed] else [Genre].[LastPlayed] end        
    where [Genre].TitleNorm = new.GenreTitleNorm and new.IsLibrary = 1;
  
    insert into Genre([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], OfflineSongsCount, OfflineDuration)
    select new.GenreTitle, new.GenreTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, 0, 0
    from [Enumerator] as e
         left join [Genre] as g on g.TitleNorm = new.GenreTitleNorm
    where e.[Id] = 1 and g.TitleNorm is null and new.IsLibrary = 1;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[ArtistArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end,
        [GoogleArtistId] = case when nullif([Artist].[GoogleArtistId], '') is null then new.[GoogleArtistId] else [Artist].[GoogleArtistId] end
    where [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and new.IsLibrary = 1;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[ArtistArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end,
        [GoogleArtistId] = case when nullif([Artist].[GoogleArtistId], '') is null then new.[GoogleArtistId] else [Artist].[GoogleArtistId] end
    where [Artist].TitleNorm = new.[ArtistTitleNorm] and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm and new.IsLibrary = 1;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount], OfflineSongsCount, OfflineDuration, OfflineAlbumsCount, GoogleArtistId)
    select coalesce(nullif(new.AlbumArtistTitle, ''), new.[ArtistTitle]), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), 1, new.Duration, new.ArtistArtUrl, new.LastPlayed, 0, 0, 0, 0, new.GoogleArtistId
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm])
    where e.[Id] = 1 and a.TitleNorm is null and new.IsLibrary = 1;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount], OfflineSongsCount, OfflineDuration, OfflineAlbumsCount, GoogleArtistId)
    select new.[ArtistTitle], new.[ArtistTitleNorm], 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, 0, 0, 0, 0, new.GoogleArtistId
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = new.[ArtistTitleNorm]
    where e.[Id] = 1 and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm and a.TitleNorm is null and new.IsLibrary = 1;

    update [Album]
    set 
        [SongsCount] = [Album].[SongsCount] + 1,
        [Duration] = [Album].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Album].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Album].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Album].[LastPlayed] then new.[LastPlayed] else [Album].[LastPlayed] end,
        [Year] = case when nullif([Album].[Year], 0) is null then nullif(new.Year, 0) else [Album].[Year] end,
        [GenreTitleNorm] = case when nullif([Album].[GenreTitleNorm], '') is null then new.[GenreTitleNorm] else [Album].[GenreTitleNorm] end,
        [GoogleAlbumId] = case when nullif([Album].[GoogleAlbumId], '') is null then new.[GoogleAlbumId] else [Album].[GoogleAlbumId] end
    where [Album].ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and TitleNorm = new.AlbumTitleNorm and new.IsLibrary = 1;

    update [Artist]
    set [ArtUrl] = new.[ArtistArtUrl]
    where [Artist].[AlbumsCount] = 0 and [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and not exists (select * from [Album] as a where a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm) and new.IsLibrary = 1;

    insert into Album([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [Year], [ArtistTitleNorm], [GenreTitleNorm], OfflineSongsCount, OfflineDuration, GoogleAlbumId)
    select new.AlbumTitle, new.AlbumTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, nullif(new.Year, 0), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), new.[GenreTitleNorm], 0, 0, new.GoogleAlbumId
    from [Enumerator] as e
         left join [Album] as a on a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm
    where e.[Id] = 1 and a.TitleNorm is null and new.IsLibrary = 1;

  END;");

            await connection.ExecuteAsync(@"
CREATE TRIGGER delete_song AFTER DELETE ON Song 
  BEGIN

    update [Genre]    
    set 
        [SongsCount] = [Genre].[SongsCount] - 1,
        [Duration] = [Genre].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm]),
        [OfflineSongsCount] = [Genre].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Genre].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0)      
    where [Genre].TitleNorm = old.GenreTitleNorm and old.IsLibrary = 1;

    delete from [Genre] where [SongsCount] <= 0;

    update [Album]    
    set 
        [SongsCount] = [Album].[SongsCount] - 1,
        [Duration] = [Album].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [Year] = (select max(s.[Year]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [GenreTitleNorm] = (select max(s.[GenreTitleNorm]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [OfflineSongsCount] = [Album].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Album].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0) ,
        GoogleAlbumId =  (select max(s.[GoogleAlbumId]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm])
    where [Album].TitleNorm = old.AlbumTitleNorm and ArtistTitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]) and old.IsLibrary = 1;    

    delete from [Album] where [SongsCount] <= 0;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] - 1,
        [Duration] = [Artist].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[ArtistArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),        
        [OfflineSongsCount] = [Artist].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Artist].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0)   ,
        GoogleArtistId =  (select max(s.[GoogleAlbumId]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]))
    where TitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]) and old.IsLibrary = 1;    

    update [Artist]    
    set 
        [SongsCount] = [SongsCount] - 1,
        [Duration] = [Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[ArtistArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm]),
        [OfflineSongsCount] = [Artist].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Artist].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0)  ,
        GoogleArtistId =  (select max(s.[GoogleAlbumId]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm])
    where [Artist].TitleNorm = old.[ArtistTitleNorm] and old.[AlbumArtistTitleNorm] <> old.[ArtistTitleNorm] and old.IsLibrary = 1;
    
    delete from [Artist] where [SongsCount] <= 0;
  END;
");

            await connection.ExecuteAsync(@"
CREATE TRIGGER insert_userplaylistentry AFTER INSERT ON UserPlaylistEntry 
  BEGIN
  
    update [UserPlaylist]
    set 
        [SongsCount] = [SongsCount] + 1,
        [Duration] = [UserPlaylist].[Duration] + (select s.[Duration] from [Song] as s where s.[SongId] = new.[SongId]),
        [ArtUrl] = case when nullif([UserPlaylist].[ArtUrl], '') is null then (select s.[AlbumArtUrl] from [Song] as s where s.[SongId] = new.[SongId]) else [UserPlaylist].[ArtUrl] end,
        [LastPlayed] = (select case when [UserPlaylist].[LastPlayed] > s.[LastPlayed] then [UserPlaylist].[LastPlayed] else s.[LastPlayed] end from [Song] as s where s.[SongId] = new.[SongId]),
        [OfflineSongsCount] = [UserPlaylist].[OfflineSongsCount] + coalesce( (select 1 from CachedSong cs where new.[SongId] = cs.[SongId]) , 0),        
        [OfflineDuration] = [UserPlaylist].[OfflineDuration] + coalesce( (select s.[Duration] from [Song] as s inner join [CachedSong] as cs on s.SongId = cs.SongId where s.[SongId] = new.[SongId]), 0)
    where [PlaylistId] = new.PlaylistId;

  END;
");

            await connection.ExecuteAsync(@"
CREATE TRIGGER delete_userplaylistentry AFTER DELETE ON [UserPlaylistEntry]
  BEGIN
  
    update [UserPlaylist]
    set 
        [SongsCount] = [SongsCount] - 1,
        [Duration] = [Duration] - (select s.[Duration] from [Song] as s where s.[SongId] = old.[SongId]),
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s inner join [UserPlaylistEntry] e on s.SongId = e.SongId where e.PlaylistId = old.PlaylistID),
        [LastPlayed] = coalesce((select max(s.[LastPlayed]) from [Song] s inner join [UserPlaylistEntry] e on s.SongId = e.SongId where e.PlaylistId = old.PlaylistID), 0),
        [OfflineSongsCount] = [UserPlaylist].[OfflineSongsCount] - coalesce( (select 1 from CachedSong cs where old.[SongId] = cs.[SongId]) , 0),        
        [OfflineDuration] = [UserPlaylist].[OfflineDuration] - coalesce( (select s.[Duration] from [Song] as s inner join [CachedSong] as cs on s.SongId = cs.SongId where s.[SongId] = old.[SongId]), 0) 
    where [PlaylistId] = old.PlaylistId;

  END; 
");

            await connection.ExecuteAsync(@"
CREATE TRIGGER update_song_lastplayed AFTER UPDATE OF [LastPlayed] ON [Song]
  BEGIN
  
    update [UserPlaylist]
    set [LastPlayed] = new.[LastPlayed] 
    where old.[LastPlayed] <> new.[LastPlayed] and [PlaylistId] in (select distinct e.[PlaylistId] from [UserPlaylistEntry] e where new.[SongId] = e.[SongId]);

    update [Artist]
    set [LastPlayed] = new.[LastPlayed] 
    where old.[LastPlayed] <> new.[LastPlayed] and [TitleNorm] = new.[ArtistTitleNorm] or [TitleNorm] = new.[AlbumArtistTitleNorm];

    update [Album]
    set [LastPlayed] = new.[LastPlayed] 
    where old.[LastPlayed] <> new.[LastPlayed] and [TitleNorm] = new.[AlbumTitleNorm] and [ArtistTitleNorm] = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]);

    update [Genre]
    set [LastPlayed] = new.[LastPlayed] 
    where old.[LastPlayed] <> new.[LastPlayed] and [TitleNorm] = new.[GenreTitleNorm];

  END;    
");

            await connection.ExecuteAsync(@"
CREATE TRIGGER update_song_albumarturl AFTER UPDATE OF [AlbumArtUrl] ON [Song]
  BEGIN
  
    update [UserPlaylist]
    set [ArtUrl] = new.[AlbumArtUrl] 
    where old.[AlbumArtUrl] <> new.[AlbumArtUrl] and [PlaylistId] in (select distinct e.[PlaylistId] from [UserPlaylistEntry] e where new.[SongId] = e.[SongId]) and new.IsLibrary = 1;

    update [Album]
    set [ArtUrl] = new.[AlbumArtUrl] 
    where old.[AlbumArtUrl] <> new.[AlbumArtUrl] and [TitleNorm] = new.[AlbumTitleNorm] and [ArtistTitleNorm] = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and new.IsLibrary = 1;

    update [Genre]
    set [ArtUrl] = new.[AlbumArtUrl] 
    where old.[AlbumArtUrl] <> new.[AlbumArtUrl] and [TitleNorm] = new.[GenreTitleNorm] and new.IsLibrary = 1;

  END;
");

            await connection.ExecuteAsync(@"
CREATE TRIGGER update_song_artistarturl AFTER UPDATE OF [ArtistArtUrl] ON [Song]
  BEGIN
  
    update [Artist]
    set [ArtUrl] = new.[ArtistArtUrl] 
    where old.[ArtistArtUrl] <> new.[ArtistArtUrl] and [TitleNorm] = new.[ArtistTitleNorm] or [TitleNorm] = new.[AlbumArtistTitleNorm] and new.IsLibrary = 1;

  END;
");

            await connection.ExecuteAsync(@"
CREATE TRIGGER update_song_parenttitlesupdate AFTER UPDATE OF [AlbumTitleNorm], [GenreTitleNorm], [AlbumArtistTitleNorm], [ArtistTitleNorm], [GoogleAlbumId], [GoogleArtistId] ON [Song]
  BEGIN  

    -------------- DELETE -------------------

    update [Genre]    
    set 
        [SongsCount] = [Genre].[SongsCount] - 1,
        [Duration] = [Genre].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm]),
        [OfflineSongsCount] = [Genre].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Genre].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0)          
    where (new.[GenreTitleNorm] <> old.[GenreTitleNorm] or (old.IsLibrary = 1 and new.IsLibrary = 0)) and [Genre].TitleNorm = old.GenreTitleNorm;

    delete from [Genre] where [SongsCount] <= 0;

    update [Album]    
    set 
        [SongsCount] = [Album].[SongsCount] - 1,
        [Duration] = [Album].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [Year] = (select max(s.[Year]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [GenreTitleNorm] = (select max(s.[GenreTitleNorm]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),        
        [OfflineSongsCount] = [Album].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Album].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [GoogleAlbumId] = (select max(s.[GoogleAlbumId]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm])
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleAlbumId <> old.GoogleAlbumId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 1 and new.IsLibrary = 0))
          and [Album].TitleNorm = old.AlbumTitleNorm and [Album].ArtistTitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]);    

    delete from [Album] where [SongsCount] <= 0;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] - 1,
        [Duration] = [Artist].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[ArtistArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),        
        [OfflineSongsCount] = [Artist].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Artist].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [GoogleArtistId] = (select max(s.[GoogleArtistId]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]))
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleArtistId <> old.GoogleArtistId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 1 and new.IsLibrary = 0))
          and [Artist].TitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]);    

    update [Artist]    
    set 
        [SongsCount] = [Artist].[SongsCount] - 1,
        [Duration] = [Artist].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[ArtistArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm]),        
        [OfflineSongsCount] = [Artist].[OfflineSongsCount] - coalesce( (select 1 from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [OfflineDuration] = [Artist].[OfflineSongsCount] - coalesce( (select old.[Duration] from CachedSong as cs where cs.SongId = old.SongId) , 0),
        [GoogleArtistId] = (select max(s.[GoogleArtistId]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm])
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleArtistId <> old.GoogleArtistId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 1 and new.IsLibrary = 0))
          and [Artist].TitleNorm = old.[ArtistTitleNorm] and old.[AlbumArtistTitleNorm] <> old.[ArtistTitleNorm];
    
    delete from [Artist] where [SongsCount] <= 0;

    -------------- INSERT -------------------
    
    update [Genre]
    set 
        [SongsCount] = [Genre].[SongsCount] + 1,
        [Duration] = [Genre].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Genre].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Genre].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Genre].[LastPlayed] then new.[LastPlayed] else [Genre].[LastPlayed] end,        
        [OfflineSongsCount] = [Genre].[OfflineSongsCount] + coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0),
        [OfflineDuration] = [Genre].[OfflineSongsCount] + coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0)        
    where (new.[GenreTitleNorm] <> old.[GenreTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1)) and [Genre].TitleNorm = new.GenreTitleNorm;
  
    insert into Genre([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [OfflineSongsCount], [OfflineDuration])
    select new.GenreTitle, new.GenreTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, 
      coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0), 
      coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0)
    from [Enumerator] as e
         left join [Genre] as g on g.TitleNorm = new.GenreTitleNorm
    where (new.[GenreTitleNorm] <> old.[GenreTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1)) and e.[Id] = 1 and g.TitleNorm is null;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[ArtistArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end,        
        [OfflineSongsCount] = [Artist].[OfflineSongsCount] + coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0),
        [OfflineDuration] = [Artist].[OfflineSongsCount] + coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0),
        [GoogleArtistId] = case when nullif([Artist].[GoogleArtistId], '') is null then new.[GoogleArtistId] else [Artist].[GoogleArtistId] end
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleArtistId <> old.GoogleArtistId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1))
          and [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]);

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[ArtistArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end,        
        [OfflineSongsCount] = [Artist].[OfflineSongsCount] + coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0),
        [OfflineDuration] = [Artist].[OfflineSongsCount] + coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0),
        [GoogleArtistId] = case when nullif([Artist].[GoogleArtistId], '') is null then new.[GoogleArtistId] else [Artist].[GoogleArtistId] end 
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleArtistId <> old.GoogleArtistId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1))
          and [Artist].TitleNorm = new.[ArtistTitleNorm] and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount], [OfflineSongsCount], [OfflineDuration], OfflineAlbumsCount, GoogleArtistId)
    select coalesce(nullif(new.AlbumArtistTitle, ''), new.[ArtistTitle]), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), 1, new.Duration, new.ArtistArtUrl, new.LastPlayed, 0, 
      coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0), 
      coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0),
      coalesce( (select count(*) from [Album] where [ArtistTitleNorm] = coalesce(nullif(new.AlbumArtistTitle, ''), new.[ArtistTitle]) and [OfflineSongsCount] > 0) , 0),
      new.GoogleArtistId
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm])
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleArtistId <> old.GoogleArtistId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1))
          and e.[Id] = 1 and a.TitleNorm is null;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount], [OfflineSongsCount], [OfflineDuration], OfflineAlbumsCount, GoogleArtistId)
    select new.[ArtistTitle], new.[ArtistTitleNorm], 1, new.Duration, new.ArtistArtUrl, new.LastPlayed, 0, 
      coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0), 
      coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0),
      coalesce( (select count(*) from [Album] where [ArtistTitleNorm] = new.[ArtistTitleNorm] and [OfflineSongsCount] > 0) , 0),
      new.GoogleArtistId
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = new.[ArtistTitleNorm]
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleArtistId <> old.GoogleArtistId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1))
          and e.[Id] = 1 and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm and a.TitleNorm is null;

    update [Album]
    set 
        [SongsCount] = [Album].[SongsCount] + 1,
        [Duration] = [Album].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Album].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Album].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Album].[LastPlayed] then new.[LastPlayed] else [Album].[LastPlayed] end,
        [Year] = case when nullif([Album].[Year], 0) is null then nullif(new.Year, 0) else [Album].[Year] end,
        [GenreTitleNorm] = case when nullif([Album].[GenreTitleNorm], '') is null then new.[GenreTitleNorm] else [Album].[GenreTitleNorm] end,        
        [OfflineSongsCount] = [Album].[OfflineSongsCount] + coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0),
        [OfflineDuration] = [Album].[OfflineSongsCount] + coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0),
        GoogleAlbumId =  case when nullif([Album].[GoogleAlbumId], '') is null then new.[GoogleAlbumId] else [Album].[GoogleAlbumId] end
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleAlbumId <> old.GoogleAlbumId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1))
          and [Album].ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and TitleNorm = new.AlbumTitleNorm;

    update [Artist]
    set [ArtUrl] = new.[ArtistArtUrl]
    where [Artist].[AlbumsCount] = 0 and (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleArtistId <> old.GoogleArtistId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1))
          and [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and not exists (select * from [Album] as a where a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm);

    insert into Album([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [Year], [ArtistTitleNorm], [GenreTitleNorm], [OfflineSongsCount], [OfflineDuration], GoogleAlbumId)
    select new.AlbumTitle, new.AlbumTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, nullif(new.Year, 0), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), new.[GenreTitleNorm], 
      coalesce( (select 1 from CachedSong as cs where cs.SongId = new.SongId) , 0), 
      coalesce( (select new.[Duration] from CachedSong as cs where cs.SongId = new.SongId) , 0),
      new.GoogleAlbumId
    from [Enumerator] as e
         left join [Album] as a on a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.GoogleAlbumId <> old.GoogleAlbumId or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm] or (old.IsLibrary = 0 and new.IsLibrary = 1))
          and e.[Id] = 1 and a.TitleNorm is null;

  END; 
");

            await connection.ExecuteAsync(@"CREATE TRIGGER insert_cachedsong AFTER INSERT ON CachedSong
  BEGIN  
        
    update [Genre]    
    set     
      [OfflineSongsCount] = [Genre].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Genre].[OfflineDuration] + (select s.Duration from Song s where s.SongId = new.SongId)
    where nullif(new.FileName, '') is not null and [Genre].TitleNorm = (select s.GenreTitleNorm from Song s where s.SongId = new.SongId and s.IsLibrary = 1);    

    update [Artist]    
    set     
      [OfflineSongsCount] = [Artist].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Artist].[OfflineDuration] + (select s.Duration from Song s where s.SongId = new.SongId)  
    where nullif(new.FileName, '') is not null and [Artist].TitleNorm = (select coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) from Song s where s.SongId = new.SongId and s.IsLibrary = 1);

    update [Artist]    
    set     
      [OfflineSongsCount] = [Artist].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Artist].[OfflineDuration] + (select s.Duration from Song s where s.SongId = new.SongId)  
    where nullif(new.FileName, '') is not null and [Artist].TitleNorm = (select s.[ArtistTitleNorm] from Song s where s.SongId = new.SongId and s.AlbumArtistTitleNorm <> ''  and s.[ArtistTitleNorm] <> s.AlbumArtistTitleNorm and s.IsLibrary = 1);    

    update [Album]    
    set     
      [OfflineSongsCount] = [Album].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Album].[OfflineDuration] + (select s.Duration from Song s where s.SongId = new.SongId)  
    where nullif(new.FileName, '') is not null and exists (select * from Song s where s.SongId = new.SongId and [Album].TitleNorm = s.[AlbumTitleNorm] and [Album].[ArtistTitleNorm] == coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) and s.IsLibrary = 1);

    update [UserPlaylist]    
    set     
      [OfflineSongsCount] = [UserPlaylist].[OfflineSongsCount] + (select count(*) from UserPlaylistEntry as e where e.PlaylistId = [UserPlaylist].[PlaylistId] and e.SongId = new.SongId),
      [OfflineDuration] = [UserPlaylist].[OfflineDuration] + (select sum(s.Duration) from Song s inner join UserPlaylistEntry as e on e.SongId = s.[SongId] and e.PlaylistId = [UserPlaylist].[PlaylistId] where s.SongId = new.SongId)  
    where nullif(new.FileName, '') is not null and exists (select * from UserPlaylistEntry as e where e.SongId = new.SongId and e.PlaylistId = [UserPlaylist].[PlaylistId]);

    update [Song]    
    set     
       [IsCached] = 1       
    where nullif(new.FileName, '') is not null and [Song].SongId = new.SongId;

  END;");

            await connection.ExecuteAsync(@"CREATE TRIGGER delete_cachedsong AFTER DELETE ON CachedSong
  BEGIN  
        
    update [Genre]    
    set     
      [OfflineSongsCount] = [Genre].[OfflineSongsCount] - 1,
      [OfflineDuration] = [Genre].[OfflineDuration] - (select s.Duration from Song s where s.SongId = old.SongId)
    where nullif(old.FileName, '') is not null and [Genre].TitleNorm = (select s.GenreTitleNorm from Song s where s.SongId = old.SongId and s.IsLibrary = 1);    

    update [Artist]    
    set     
      [OfflineSongsCount] = [Artist].[OfflineSongsCount] - 1,
      [OfflineDuration] = [Artist].[OfflineDuration] - (select s.Duration from Song s where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is not null and [Artist].TitleNorm = (select coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) from Song s where s.SongId = old.SongId and s.IsLibrary = 1);

    update [Artist]    
    set     
      [OfflineSongsCount] = [Artist].[OfflineSongsCount] - 1,
      [OfflineDuration] = [Artist].[OfflineDuration] - (select s.Duration from Song s where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is not null and [Artist].TitleNorm = (select s.[ArtistTitleNorm] from Song s where s.SongId = old.SongId and s.AlbumArtistTitleNorm <> ''  and s.[ArtistTitleNorm] <> s.AlbumArtistTitleNorm and s.IsLibrary = 1);    

    update [Album]    
    set     
      [OfflineSongsCount] = [Album].[OfflineSongsCount] - 1,
      [OfflineDuration] = [Album].[OfflineDuration] - (select s.Duration from Song s where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is not null and exists (select * from Song s where s.SongId = old.SongId and [Album].TitleNorm = s.[AlbumTitleNorm] and [Album].[ArtistTitleNorm] == coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) and s.IsLibrary = 1);

    update [UserPlaylist]    
    set     
      [OfflineSongsCount] = [UserPlaylist].[OfflineSongsCount] - (select count(*) from UserPlaylistEntry as e where e.PlaylistId = [UserPlaylist].[PlaylistId] and e.SongId = old.SongId),
      [OfflineDuration] = [UserPlaylist].[OfflineDuration] - (select sum(s.Duration) from Song s inner join UserPlaylistEntry as e on e.SongId = s.[SongId] and e.PlaylistId = [UserPlaylist].[PlaylistId] where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is not null and exists (select * from UserPlaylistEntry as e where e.SongId = old.SongId and e.PlaylistId = [UserPlaylist].[PlaylistId]);

    update [Song]    
    set     
       [IsCached] = 0     
    where [Song].SongId = old.SongId;

  END;");

            await connection.ExecuteAsync(@"CREATE TRIGGER update_cachedsong AFTER UPDATE ON CachedSong
  BEGIN  
        
    update [Genre]    
    set     
      [OfflineSongsCount] = [Genre].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Genre].[OfflineDuration] + (select s.Duration from Song s where s.SongId = old.SongId)
    where nullif(old.FileName, '') is null and nullif(new.[FileName], '') is not null and new.SongId = old.SongId and [Genre].TitleNorm = (select s.GenreTitleNorm from Song s where s.SongId = old.SongId and s.IsLibrary = 1);    

    update [Artist]    
    set     
      [OfflineSongsCount] = [Artist].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Artist].[OfflineDuration] + (select s.Duration from Song s where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is null and nullif(new.[FileName], '') is not null and new.SongId = old.SongId and [Artist].TitleNorm = (select coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) from Song s where s.SongId = old.SongId and s.IsLibrary = 1);

    update [Artist]    
    set     
      [OfflineSongsCount] = [Artist].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Artist].[OfflineDuration] + (select s.Duration from Song s where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is null and nullif(new.[FileName], '') is not null and new.SongId = old.SongId and [Artist].TitleNorm = (select s.[ArtistTitleNorm] from Song s where s.SongId = old.SongId and s.AlbumArtistTitleNorm <> ''  and s.[ArtistTitleNorm] <> s.AlbumArtistTitleNorm and s.IsLibrary = 1);    

    update [Album]    
    set     
      [OfflineSongsCount] = [Album].[OfflineSongsCount] + 1,
      [OfflineDuration] = [Album].[OfflineDuration] + (select s.Duration from Song s where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is null and nullif(new.[FileName], '') is not null and new.SongId = old.SongId and exists (select * from Song s where s.SongId = old.SongId and [Album].TitleNorm = s.[AlbumTitleNorm] and [Album].[ArtistTitleNorm] == coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) and s.IsLibrary = 1);

    update [UserPlaylist]    
    set     
      [OfflineSongsCount] = [UserPlaylist].[OfflineSongsCount] + (select count(*) from UserPlaylistEntry as e where e.PlaylistId = [UserPlaylist].[PlaylistId] and e.SongId = old.SongId),
      [OfflineDuration] = [UserPlaylist].[OfflineDuration] + (select sum(s.Duration) from Song s inner join UserPlaylistEntry as e on e.SongId = s.[SongId] and e.PlaylistId = [UserPlaylist].[PlaylistId] where s.SongId = old.SongId)  
    where nullif(old.FileName, '') is null and nullif(new.[FileName], '') is not null and new.SongId = old.SongId and exists (select * from UserPlaylistEntry as e where e.SongId = old.SongId and e.PlaylistId = [UserPlaylist].[PlaylistId]);

    update [Song]    
    set     
       [IsCached] = 1       
    where nullif(old.FileName, '') is null and nullif(new.[FileName], '') is not null and [Song].SongId = new.SongId;

  END;");

            await connection.ExecuteAsync(@"CREATE TRIGGER insert_album AFTER INSERT ON [Album]
  BEGIN      
                                                    
    update [Artist]    
    set    
      [AlbumsCount] = [Artist].[AlbumsCount] + 1,      
      [OfflineAlbumsCount] = [Artist].[OfflineAlbumsCount] + (case when new.[OfflineSongsCount] > 0 then 1 else 0 end)
    where [Artist].[TitleNorm] = new.[ArtistTitleNorm];

  END;");

            await connection.ExecuteAsync(@"CREATE TRIGGER delete_album AFTER DELETE ON [Album]
  BEGIN      
                                                    
    update [Artist]    
    set    
      [AlbumsCount] = [Artist].[AlbumsCount] - 1,      
      [OfflineAlbumsCount] = [Artist].[OfflineAlbumsCount] - (case when old.[OfflineSongsCount] > 0 then 1 else 0 end)
    where [Artist].[TitleNorm] = old.[ArtistTitleNorm];

  END;");

            await connection.ExecuteAsync(@"CREATE TRIGGER update_album AFTER UPDATE OF [OfflineSongsCount] ON [Album]
  BEGIN                             

    update [Artist]    
    set         
      [OfflineAlbumsCount] = [Artist].[OfflineAlbumsCount] + (case when new.[OfflineSongsCount] > 0 then 1 else 0 end)
    where  old.[OfflineSongsCount] = 0 and new.[OfflineSongsCount] > 0 and [Artist].[TitleNorm] = new.[ArtistTitleNorm];
                                                    
    update [Artist]    
    set       
      [OfflineAlbumsCount] = [Artist].[OfflineAlbumsCount] - (case when old.[OfflineSongsCount] > 0 then 1 else 0 end)
    where old.[OfflineSongsCount] > 0 and new.[OfflineSongsCount] = 0 and [Artist].[TitleNorm] = old.[ArtistTitleNorm];

  END;");
        }

        private string GetDatabaseFilePath()
        {
#if NETFX_CORE
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, this.dbFileName);
#else
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.dbFileName);
#endif
        }

        [Table("sqlite_master")]
        public class SqliteMasterRecord
        {
            [Column("name")]
            public string Name { get; set; }
        }

        public class DatabaseUpdateInformation
        {
            public DatabaseUpdateInformation(int dbVersion, DatabaseStatus databaseStatus)
            {
                this.PreviousVersion = dbVersion;
                this.Status = databaseStatus;
            }

            public int PreviousVersion { get; private set; }

            public DatabaseStatus Status { get; private set; }
        }
    }
}
