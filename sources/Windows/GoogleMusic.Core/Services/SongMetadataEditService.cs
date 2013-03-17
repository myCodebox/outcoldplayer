// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    public interface ISongMetadataEditService
    {
        Task UpdateRatingAsync(SongBindingModel song, byte newRating);
    }

    public class SongMetadataEditService : ISongMetadataEditService
    {
        private readonly IDispatcher dispatcher;
        private readonly ISongWebService songWebService;
        private readonly ILogger logger;

        public SongMetadataEditService(
            ILogManager logManager,
            IDispatcher dispatcher,
            ISongWebService songWebService)
        {
            this.dispatcher = dispatcher;
            this.songWebService = songWebService;
            this.logger = logManager.CreateLogger("SongMetadataEditService");
        }

        public async Task UpdateRatingAsync(SongBindingModel song, byte newRating)
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
                this.logger.Debug("Updating rating for song '{0}' to rating '{1}' from '{2}'.", song.Metadata.ProviderSongId, newRating, song.Metadata.Rating);
            }

            await this.dispatcher.RunAsync(() => song.Rating = newRating);

            var ratingResp = await this.songWebService.UpdateRatingAsync(song.Metadata.ProviderSongId, newRating);
            
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Rating updated for song: {0}.", song.Metadata.ProviderSongId);
            }

            foreach (var songUpdate in ratingResp.Songs)
            {
                var songRatingResp = songUpdate;

                if (string.Equals(songUpdate.Id, song.Metadata.ProviderSongId))
                {
                    await this.dispatcher.RunAsync(() => song.Rating = songRatingResp.Rating);

                    if (this.logger.IsDebugEnabled)
                    {
                        this.logger.Debug("Song updated: {0}, Rating: {1}.", songUpdate.Id, songUpdate.Rating);
                    }
                }
            }
        }
    }
}