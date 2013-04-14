// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;

    public interface ISongsService
    {
        Task UpdateRatingAsync(Song song, byte newRating);
    }

    public class SongsService : ISongsService
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ISongsWebService songsWebService;
        private readonly ISongsRepository songsRepository;

        private readonly ILogger logger;

        public SongsService(
            ILogManager logManager,
            IEventAggregator eventAggregator,
            ISongsWebService songsWebService,
            ISongsRepository songsRepository)
        {
            this.eventAggregator = eventAggregator;
            this.songsWebService = songsWebService;
            this.songsRepository = songsRepository;
            this.logger = logManager.CreateLogger("SongMetadataEditService");
        }

        public async Task UpdateRatingAsync(Song song, byte newRating)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            if (newRating > 5)
            {
                throw new ArgumentOutOfRangeException("newRating", "Rating cannot be more than 5.");
            }

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Updating rating for song '{0}' to rating '{1}' from '{2}'.", song.ProviderSongId, newRating, song.Rating);
            }

            var ratingResp = await this.songsWebService.UpdateRatingAsync(song.ProviderSongId, newRating);
            
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Rating updated for song: {0}.", song.ProviderSongId);
            }

            foreach (var songUpdate in ratingResp.Songs)
            {
                var songRatingResp = songUpdate;

                if (string.Equals(songUpdate.Id, song.ProviderSongId))
                {
                    song.Rating = songRatingResp.Rating;
                    
                    var songs = new[] { song };
                    await this.songsRepository.UpdateAsync(songs);
                    this.eventAggregator.Publish(new SongsUpdatedEvent(songs));
                    
                    if (this.logger.IsDebugEnabled)
                    {
                        this.logger.Debug("Song updated: {0}, Rating: {1}.", songUpdate.Id, songUpdate.Rating);
                    }
                }
            }
        }
    }
}