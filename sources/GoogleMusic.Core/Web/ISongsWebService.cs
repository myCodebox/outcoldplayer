// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface ISongsWebService
    {
        Task<StatusResp> GetStatusAsync();

        Task<IList<GoogleMusicSong>> GetAllAsync(
            DateTime? lastUpdate,
            IProgress<int> progress,
            Func<IList<GoogleMusicSong>, Task> chunkHandler = null);

        Task<GoogleMusicSongUrl> GetSongUrlAsync(Song song, CancellationToken token);

        Task<GoogleMusicTrackStatResponse> SendStatsAsync(IList<Song> songs);

        Task<GoogleMusicSongMutateResponse> UpdateRatingsAsync(IDictionary<Song, int> ratings, DateTime modificationDateTime);

        Task<GoogleMusicSongMutateResponse> AddSongsAsync(IList<Song> songs);

        Task<GoogleMusicSongMutateResponse> RemoveSongsAsync(IList<Song> songs);
    }
}
