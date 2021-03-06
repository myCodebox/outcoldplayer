﻿// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IRadioStationsRepository : IPlaylistRepository<Radio>
    {
        Task<int> InsertAsync(IEnumerable<Radio> radios);

        Task<int> DeleteAsync(IEnumerable<Radio> radios);

        Task<int> UpdateAsync(IEnumerable<Radio> radios);

        Task<Radio> FindByGoogleArtistId(string googleArtistId);

        Task<Radio> FindByGoogleAlbumId(string googleAlbumId);

        Task<Radio> FindByGoogleSongId(string googleSongId);

        Task<Radio> FindByGoogleGenreId(string googleGenreId);

        Task<Radio> FindByGoogleRadioId(string googleRadioId);

        Task<Radio> FindByCuratedStationId(string curatedStationId);
    }

    public class RadioStationsRepository : RepositoryBase, IRadioStationsRepository
    {
        private const string SqlSearchRadio = @"
select x.*
from [Radio] as x  
where x.[TitleNorm] like ?1
order by x.[TitleNorm]
";

        public Task<int> GetCountAsync()
        {
            return this.Connection.Table<Radio>().CountAsync();
        }

        public async Task<IList<Radio>> GetAllAsync(Order order, uint? take = null)
        {
            var query = this.Connection.Table<Radio>();

            if (order == Order.Name)
            {
                query = query.OrderBy(x => x.TitleNorm);
            }
            else if (order == Order.LastPlayed)
            {
                query = query.OrderByDescending(x => x.Recent);
            }

            if (take.HasValue)
            {
                query = query.Take((int)take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<IList<Radio>> SearchAsync(string searchQuery, uint? take)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchRadio);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Radio>(sql.ToString(), string.Format("%{0}%", searchQueryNorm));
        }

        public Task<Radio> GetAsync(string id)
        {
            return this.Connection.Table<Radio>().Where(a => a.RadioId == id).FirstOrDefaultAsync();
        }

        public Task<IList<Song>> GetSongsAsync(string id, bool includeAll = false)
        {
            throw new System.NotSupportedException("Radios do not have songs");
        }

        public Task<int> InsertAsync(IEnumerable<Radio> radios)
        {
            return this.Connection.InsertAllAsync(radios);
        }

        public async Task<int> DeleteAsync(IEnumerable<Radio> radios)
        {
            int deletedCount = 0;
            await this.Connection.RunInTransactionAsync(
                (connection) =>
                {
                    foreach (var radio in radios)
                    {
                        deletedCount += connection.Delete(radio);
                    }
                });

            return deletedCount;
        }

        public async Task<int> UpdateAsync(IEnumerable<Radio> radios)
        {
            int updatedCount = 0;
            await this.Connection.RunInTransactionAsync(
                (connection) => updatedCount += connection.UpdateAll(radios));
            return updatedCount;
        }

        public Task<Radio> FindByGoogleArtistId(string googleArtistId)
        {
            return this.Connection.Table<Radio>().Where(x => x.GoogleArtistId == googleArtistId).FirstOrDefaultAsync();
        }

        public Task<Radio> FindByGoogleAlbumId(string googleAlbumId)
        {
            return this.Connection.Table<Radio>().Where(x => x.GoogleAlbumId == googleAlbumId).FirstOrDefaultAsync();
        }

        public Task<Radio> FindByGoogleSongId(string googleSongId)
        {
            return this.Connection.Table<Radio>().Where(x => x.SongId == googleSongId || x.TrackLockerId == googleSongId).FirstOrDefaultAsync();
        }

        public Task<Radio> FindByGoogleGenreId(string googleGenreId)
        {
            return this.Connection.Table<Radio>().Where(x => x.GoogleGenreId == googleGenreId).FirstOrDefaultAsync();
        }

        public Task<Radio> FindByGoogleRadioId(string googleRadioId)
        {
            return this.Connection.Table<Radio>().Where(x => x.RadioId == googleRadioId).FirstOrDefaultAsync();
        }

        public Task<Radio> FindByCuratedStationId(string curatedStationId)
        {
            return this.Connection.Table<Radio>().Where(x => x.CuratedStationId == curatedStationId).FirstOrDefaultAsync();
        }
    }
}
