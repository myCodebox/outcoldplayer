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

                await connection.CreateTableAsync<Song>();
                await connection.CreateTableAsync<UserPlaylist>();
                await connection.CreateTableAsync<UserPlaylistEntry>();
                await connection.CreateTableAsync<Album>();
                await connection.CreateTableAsync<Genre>();
                await connection.CreateTableAsync<Artist>();

                await connection.ExecuteAsync(@"CREATE TABLE [Enumerator] (Id integer primary key autoincrement not null);");
                await connection.ExecuteAsync(@"INSERT INTO [Enumerator] DEFAULT VALUES;");

                await connection.ExecuteAsync(@"
CREATE TRIGGER instert_song AFTER INSERT ON Song 
  BEGIN

    update [Genre]
    set 
        [SongsCount] = [Genre].[SongsCount] + 1,
        [Duration] = [Genre].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Genre].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Genre].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Genre].[LastPlayed] then new.[LastPlayed] else [Genre].[LastPlayed] end        
    where [Genre].TitleNorm = new.GenreTitleNorm;
  
    insert into Genre([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed])
    select new.GenreTitle, new.GenreTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed
    from [Enumerator] as e
         left join [Genre] as g on g.TitleNorm = new.GenreTitleNorm
    where e.[Id] = 1 and g.TitleNorm is null;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end    
    where [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]);

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end    
    where [Artist].TitleNorm = new.[ArtistTitleNorm] and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount])
    select coalesce(nullif(new.AlbumArtistTitle, ''), new.[ArtistTitle]), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, 0
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm])
    where e.[Id] = 1 and a.TitleNorm is null;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount])
    select new.[ArtistTitle], new.[ArtistTitleNorm], 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, 0
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = new.[ArtistTitleNorm]
    where e.[Id] = 1 and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm and a.TitleNorm is null;

    update [Album]
    set 
        [SongsCount] = [Album].[SongsCount] + 1,
        [Duration] = [Album].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Album].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Album].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Album].[LastPlayed] then new.[LastPlayed] else [Album].[LastPlayed] end,
        [Year] = case when nullif([Album].[Year], 0) is null then nullif(new.Year, 0) else [Album].[Year] end,
        [GenreTitleNorm] = case when nullif([Album].[GenreTitleNorm], '') is null then new.[GenreTitleNorm] else [Album].[GenreTitleNorm] end
    where [Album].ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and TitleNorm = new.AlbumTitleNorm;

    update [Artist]
    set 
        [ArtUrl] = case when [Artist].[AlbumsCount] = 0 then new.[AlbumArtUrl] else [ArtUrl] end,
        [AlbumsCount] = [Artist].[AlbumsCount] + 1
    where [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and not exists (select * from [Album] as a where a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm);

    insert into Album([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [Year], [ArtistTitleNorm], [GenreTitleNorm])
    select new.AlbumTitle, new.AlbumTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, nullif(new.Year, 0), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), new.[GenreTitleNorm]
    from [Enumerator] as e
         left join [Album] as a on a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm
    where e.[Id] = 1 and a.TitleNorm is null;

  END;");

                await connection.ExecuteAsync(@"
CREATE TRIGGER delete_song AFTER DELETE ON Song 
  BEGIN

    update [Genre]    
    set 
        [SongsCount] = [Genre].[SongsCount] - 1,
        [Duration] = [Genre].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm])      
    where [Genre].TitleNorm = old.GenreTitleNorm;

    delete from [Genre] where [SongsCount] <= 0;

    update [Album]    
    set 
        [SongsCount] = [Album].[SongsCount] - 1,
        [Duration] = [Album].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [Year] = (select max(s.[Year]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [GenreTitleNorm] = (select max(s.[GenreTitleNorm]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm])
    where [Album].TitleNorm = old.AlbumTitleNorm and ArtistTitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]);    

    delete from [Album] where [SongsCount] <= 0;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] - 1,
        [Duration] = [Artist].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),        
        [AlbumsCount] = (select count(*) from [Album] as a where a.ArtistTitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]))
    where TitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]);    

    update [Artist]    
    set 
        [SongsCount] = [SongsCount] - 1,
        [Duration] = [Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm])
    where [Artist].TitleNorm = old.[ArtistTitleNorm] and old.[AlbumArtistTitleNorm] <> old.[ArtistTitleNorm];
    
    delete from [Artist] where [SongsCount] <= 0;

  END;
");

                await connection.ExecuteAsync(@"
CREATE TRIGGER instert_userplaylistentry AFTER INSERT ON UserPlaylistEntry 
  BEGIN
  
    update [UserPlaylist]
    set 
        [SongsCount] = [SongsCount] + 1,
        [Duration] = [UserPlaylist].[Duration] + (select s.[Duration] from [Song] as s where s.[SongId] = new.[SongId]),
        [ArtUrl] = case when nullif([UserPlaylist].[ArtUrl], '') is null then (select s.[AlbumArtUrl] from [Song] as s where s.[SongId] = new.[SongId]) else [UserPlaylist].[ArtUrl] end,
        [LastPlayed] = (select case when [UserPlaylist].[LastPlayed] > s.[LastPlayed] then [UserPlaylist].[LastPlayed] else s.[LastPlayed] end from [Song] as s where s.[SongId] = new.[SongId]) 
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
        [LastPlayed] = coalesce((select max(s.[LastPlayed]) from [Song] s inner join [UserPlaylistEntry] e on s.SongId = e.SongId where e.PlaylistId = old.PlaylistID), 0) 
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
    where old.[AlbumArtUrl] <> new.[AlbumArtUrl] and [PlaylistId] in (select distinct e.[PlaylistId] from [UserPlaylistEntry] e where new.[SongId] = e.[SongId]);

    update [Artist]
    set [ArtUrl] = new.[AlbumArtUrl] 
    where old.[AlbumArtUrl] <> new.[AlbumArtUrl] and [TitleNorm] = new.[ArtistTitleNorm] or [TitleNorm] = new.[AlbumArtistTitleNorm];

    update [Album]
    set [ArtUrl] = new.[AlbumArtUrl] 
    where old.[AlbumArtUrl] <> new.[AlbumArtUrl] and [TitleNorm] = new.[AlbumTitleNorm] and [ArtistTitleNorm] = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]);

    update [Genre]
    set [ArtUrl] = new.[AlbumArtUrl] 
    where old.[AlbumArtUrl] <> new.[AlbumArtUrl] and [TitleNorm] = new.[GenreTitleNorm];

  END;
");

                await connection.ExecuteAsync(@"
CREATE TRIGGER update_song_parenttitlesupdate AFTER UPDATE OF [AlbumTitleNorm], [GenreTitleNorm], [ArtistAlbumTitleNorm], [ArtistTitleNorm] ON [Song]
  BEGIN  

    -------------- DELETE -------------------

    update [Genre]    
    set 
        [SongsCount] = [Genre].[SongsCount] - 1,
        [Duration] = [Genre].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[GenreTitleNorm] = old.[GenreTitleNorm])      
    where new.[GenreTitleNorm] <> old.[GenreTitleNorm] and [Genre].TitleNorm = old.GenreTitleNorm;

    delete from [Genre] where [SongsCount] <= 0;

    update [Album]    
    set 
        [SongsCount] = [Album].[SongsCount] - 1,
        [Duration] = [Album].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [Year] = (select max(s.[Year]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm]),
        [GenreTitleNorm] = (select max(s.[GenreTitleNorm]) from [Song] s where s.[AlbumTitleNorm] = old.[AlbumTitleNorm])
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and [Album].TitleNorm = old.AlbumTitleNorm and [Album].ArtistTitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]);    

    delete from [Album] where [SongsCount] <= 0;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] - 1,
        [Duration] = [Artist].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm])),        
        [AlbumsCount] = (select count(*) from [Album] as a where a.ArtistTitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]))
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and [Artist].TitleNorm = coalesce(nullif(old.AlbumArtistTitleNorm, ''), old.[ArtistTitleNorm]);    

    update [Artist]    
    set 
        [SongsCount] = [Artist].[SongsCount] - 1,
        [Duration] = [Artist].[Duration] - old.[Duration],
        [ArtUrl] = (select max(s.[AlbumArtUrl]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm]),
        [LastPlayed] = (select max(s.[LastPlayed]) from [Song] s where coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = old.[ArtistTitleNorm])
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and [Artist].TitleNorm = old.[ArtistTitleNorm] and old.[AlbumArtistTitleNorm] <> old.[ArtistTitleNorm];
    
    delete from [Artist] where [SongsCount] <= 0;

    -------------- INSERT -------------------
    
    update [Genre]
    set 
        [SongsCount] = [Genre].[SongsCount] + 1,
        [Duration] = [Genre].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Genre].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Genre].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Genre].[LastPlayed] then new.[LastPlayed] else [Genre].[LastPlayed] end        
    where new.[GenreTitleNorm] <> old.[GenreTitleNorm] and [Genre].TitleNorm = new.GenreTitleNorm;
  
    insert into Genre([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed])
    select new.GenreTitle, new.GenreTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed
    from [Enumerator] as e
         left join [Genre] as g on g.TitleNorm = new.GenreTitleNorm
    where new.[GenreTitleNorm] <> old.[GenreTitleNorm] and e.[Id] = 1 and g.TitleNorm is null;

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end    
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]);

    update [Artist]
    set 
        [SongsCount] = [Artist].[SongsCount] + 1,
        [Duration] = [Artist].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Artist].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Artist].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Artist].[LastPlayed] then new.[LastPlayed] else [Artist].[LastPlayed] end    
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and [Artist].TitleNorm = new.[ArtistTitleNorm] and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount])
    select coalesce(nullif(new.AlbumArtistTitle, ''), new.[ArtistTitle]), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, 0
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm])
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and e.[Id] = 1 and a.TitleNorm is null;

    insert into Artist([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [AlbumsCount])
    select new.[ArtistTitle], new.[ArtistTitleNorm], 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, 0
    from [Enumerator] as e
         left join [Artist] as a on a.TitleNorm = new.[ArtistTitleNorm]
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and e.[Id] = 1 and new.AlbumArtistTitleNorm <> ''  and new.[ArtistTitleNorm] <> new.AlbumArtistTitleNorm and a.TitleNorm is null;

    update [Album]
    set 
        [SongsCount] = [Album].[SongsCount] + 1,
        [Duration] = [Album].[Duration] + new.[Duration],
        [ArtUrl] = case when nullif([Album].[ArtUrl], '') is null then new.[AlbumArtUrl] else [Album].[ArtUrl] end,
        [LastPlayed] = case when new.[LastPlayed] > [Album].[LastPlayed] then new.[LastPlayed] else [Album].[LastPlayed] end,
        [Year] = case when nullif([Album].[Year], 0) is null then nullif(new.Year, 0) else [Album].[Year] end,
        [GenreTitleNorm] = case when nullif([Album].[GenreTitleNorm], '') is null then new.[GenreTitleNorm] else [Album].[GenreTitleNorm] end
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and [Album].ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and TitleNorm = new.AlbumTitleNorm;

    update [Artist]
    set 
        [ArtUrl] = case when [Artist].[AlbumsCount] = 0 then new.[AlbumArtUrl] else [ArtUrl] end,
        [AlbumsCount] = [Artist].[AlbumsCount] + 1
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and [Artist].TitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and not exists (select * from [Album] as a where a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm);

    insert into Album([Title], [TitleNorm], [SongsCount], [Duration], [ArtUrl], [LastPlayed], [Year], [ArtistTitleNorm], [GenreTitleNorm])
    select new.AlbumTitle, new.AlbumTitleNorm, 1, new.Duration, new.AlbumArtUrl, new.LastPlayed, nullif(new.Year, 0), coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]), new.[GenreTitleNorm]
    from [Enumerator] as e
         left join [Album] as a on a.ArtistTitleNorm = coalesce(nullif(new.AlbumArtistTitleNorm, ''), new.[ArtistTitleNorm]) and a.TitleNorm = new.AlbumTitleNorm
    where (new.[AlbumTitleNorm] <> old.[AlbumTitleNorm] or new.[ArtistTitleNorm] <> old.[ArtistTitleNorm] or new.[AlbumArtistTitleNorm] <> old.[AlbumArtistTitleNorm])
          and e.[Id] = 1 and a.TitleNorm is null;

  END; 
");
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
