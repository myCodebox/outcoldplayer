// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
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
                this.logger.Debug("Updating rating for song '{0}' to rating '{1}' from '{2}'.", song.SongId, newRating, song.Rating);
            }

            var resp = await this.songsWebService.UpdateRatingsAsync(new Dictionary<Song, int>() { { song, newRating } });
            
            foreach (var mutation in resp.MutateResponse)
            {
                if (string.Equals(mutation.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    song.Rating = newRating;
                    
                    try
                    {
                        await this.songsRepository.UpdateRatingAsync(song);
                        this.eventAggregator.Publish(new SongsUpdatedEvent(new[] { song }));

                        if (this.logger.IsDebugEnabled)
                        {
                            this.logger.Debug("Song updated: {0}, Rating: {1}.", song.SongId, song.Rating);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.logger.Debug(exception, "UpdateRatingAsync");
                    }
                }
            }
        }
    }
}