// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface ISystemPlaylistsRepository : IPlaylistRepository<SystemPlaylist>
    {
        Task<SystemPlaylist> GetAsync(SystemPlaylistType systemPlaylistType);

        Task<IList<Song>> GetSongsAsync(SystemPlaylistType systemPlaylistType, bool includeAll = false);

        Task<IList<SystemPlaylist>> GetAllAsync();

        Task<Uri[]> GetUrisAsync(SystemPlaylistType systemPlaylistType);
    }

    public class SystemPlaylistsRepository : RepositoryBase, ISystemPlaylistsRepository
    {
        private const int HighlyRatedValue = 4;
        private const int LastAddedSongsCount = 500;

        private const string SqlHiglyRatedSongsPlaylists = @"
select count(*) as SongsCount, sum(s.[Duration]) as Duration, sum(s.IsCached) as OfflineSongsCount, sum(s.[Duration]) as OfflineDuration, ?3 as [SystemPlaylistType]
from [Song] as s
where (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 and s.[Rating] >= ?2 
";

        private const string SqlHiglyRatedSongsPlaylistsAlbumArts = @"
select * 
from
(
 select distinct(s.AlbumArtUrl) as Url
 from Song s
 where s.AlbumArtUrl is not null and (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 and s.[Rating] >= ?2 
 order by s.Rating, s.PlayCount, s.Recent desc
)
limit 4 ";

        private const string SqlLastAddedPlaylist = @"
select count(*) as SongsCount, sum(x.[Duration]) as Duration, sum(x.IsCached) as OfflineSongsCount, sum(x.[Duration]) as OfflineDuration, ?3 as [SystemPlaylistType] from
(
  select *
  from [Song] as s  
  where (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1
  order by s.[CreationDate] desc
  limit ?2
) as x
";

        private const string SqlLastAddedPlaylistAlbumArts = @"
select * 
from
(
 select distinct(s.AlbumArtUrl) as Url
 from Song s
 where s.AlbumArtUrl is not null and (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 
 order by s.CreationDate desc
)
limit 4 ";

        private const string SqlAllSongsPlaylist = @"
select count(*) as SongsCount, sum(s.[Duration]) as Duration, sum(s.IsCached) as OfflineSongsCount, sum(s.[Duration]) as OfflineDuration, ?2 as [SystemPlaylistType] from [Song] as s where (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 
";

        private const string SqlAllSongsPlaylistAlbumArts = @"
select * 
from
(
 select distinct(s.AlbumArtUrl) as Url
 from Song s
 where s.AlbumArtUrl is not null and (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1 
 order by s.Recent desc
)
limit 4 ";

        private const string SqlAllSongs = @"
select s.* 
from [Song] as s
where (?1 = 1 or s.[IsCached] = 1) and s.IsLibrary = 1
order by coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]), s.[AlbumTitleNorm], coalesce(nullif(s.Disc, 0), 1), s.Track
";

        private const string SqlHighlyRatedSongs = @"
select s.* 
from [Song] as s
where (?1 = 1 or s.[IsCached] = 1) and s.[Rating] >= ?2 and s.IsLibrary = 1
order by s.TitleNorm
";

        private const string SqlLastAddedSongs = @"
select *
from [Song] as x  
where (?1 = 1 or x.[IsCached] = 1) and x.IsLibrary = 1
order by x.[CreationDate] desc
limit ?2
";

        private readonly IApplicationStateService stateService;

        public SystemPlaylistsRepository(IApplicationStateService stateService)
        {
            this.stateService = stateService;
        }

        public async Task<SystemPlaylist> GetHighlyRatedPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlHiglyRatedSongsPlaylists, this.stateService.IsOnline(), HighlyRatedValue, SystemPlaylistType.HighlyRated)).First();
        }

        public async Task<SystemPlaylist> GetLastAddedSongsPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlLastAddedPlaylist, this.stateService.IsOnline(), LastAddedSongsCount, SystemPlaylistType.LastAdded)).First();
        }

        public async Task<SystemPlaylist> GetAllSongsPlaylistAsync()
        {
            return (await this.Connection.QueryAsync<SystemPlaylist>(SqlAllSongsPlaylist, this.stateService.IsOnline(), SystemPlaylistType.AllSongs)).First();
        }

        public Task<IList<Song>> GetSongsAsync(string id, bool includeAll = false)
        {
            return this.GetSongsAsync((SystemPlaylistType)Enum.Parse(typeof(SystemPlaylistType), id), includeAll);
        }

        public Task<IList<Song>> GetSongsAsync(SystemPlaylistType systemPlaylistType, bool includeAll = false)
        {
            switch (systemPlaylistType)
            {
                case SystemPlaylistType.AllSongs:
                    return this.GetAllSongsAsync(includeAll);
                case SystemPlaylistType.HighlyRated:
                    return this.GetHighlyRatedSongsAsync(includeAll);
                case SystemPlaylistType.LastAdded:
                    return this.GetLastAddedSongsAsync(includeAll);
                default:
                    throw new ArgumentOutOfRangeException("systemPlaylistType");
            }
        }

        public Task<IList<SystemPlaylist>> GetAllAsync()
        {
            return this.GetAllAsync(order: Order.None, take: null);
        }

        public Task<int> GetCountAsync()
        {
            return Task.FromResult(3);
        }

        public async Task<IList<SystemPlaylist>> GetAllAsync(Order order, uint? take = null)
        {
            return await Task.WhenAll(
                this.GetAllSongsPlaylistAsync(),
                this.GetHighlyRatedPlaylistAsync(),
                this.GetLastAddedSongsPlaylistAsync());
        }

        public Task<IList<SystemPlaylist>> SearchAsync(string searchQuery, uint? take)
        {
            throw new NotSupportedException();
        }

        public Task<SystemPlaylist> GetAsync(string id)
        {
            return this.GetAsync((SystemPlaylistType)Enum.Parse(typeof(SystemPlaylistType), id));
        }

        public Task<SystemPlaylist> GetAsync(SystemPlaylistType systemPlaylistType)
        {
            switch (systemPlaylistType)
            {
                case SystemPlaylistType.AllSongs:
                    return this.GetAllSongsPlaylistAsync();
                case SystemPlaylistType.HighlyRated:
                    return this.GetHighlyRatedPlaylistAsync();
                case SystemPlaylistType.LastAdded:
                    return this.GetLastAddedSongsPlaylistAsync();
                default:
                    throw new ArgumentOutOfRangeException("systemPlaylistType");
            }
        }

        private async Task<IList<Song>> GetAllSongsAsync(bool includeAll = false)
        {
            return await this.Connection.QueryAsync<Song>(SqlAllSongs, includeAll || this.stateService.IsOnline());
        }

        private async Task<IList<Song>> GetHighlyRatedSongsAsync(bool includeAll = false)
        {
            return await this.Connection.QueryAsync<Song>(SqlHighlyRatedSongs, includeAll || this.stateService.IsOnline(), HighlyRatedValue);
        }

        private async Task<IList<Song>> GetLastAddedSongsAsync(bool includeAll = false)
        {
            return await this.Connection.QueryAsync<Song>(SqlLastAddedSongs, includeAll || this.stateService.IsOnline(), LastAddedSongsCount);
        }

        public async Task<Uri[]> GetUrisAsync(SystemPlaylistType systemPlaylistType)
        {
            switch (systemPlaylistType)
            {
                case SystemPlaylistType.AllSongs:
                    return (await this.Connection.QueryAsync<UrlRef>(SqlAllSongsPlaylistAlbumArts, this.stateService.IsOnline())).Select(x => new Uri(x.Url)).ToArray();
                case SystemPlaylistType.HighlyRated:
                    return (await this.Connection.QueryAsync<UrlRef>(SqlHiglyRatedSongsPlaylistsAlbumArts, this.stateService.IsOnline(), HighlyRatedValue)).Select(x => new Uri(x.Url)).ToArray();
                case SystemPlaylistType.LastAdded:
                    return (await this.Connection.QueryAsync<UrlRef>(SqlLastAddedPlaylistAlbumArts, this.stateService.IsOnline())).Select(x => new Uri(x.Url)).ToArray();
                default:
                    throw new ArgumentOutOfRangeException("systemPlaylistType");
            }
        }
    }
}
