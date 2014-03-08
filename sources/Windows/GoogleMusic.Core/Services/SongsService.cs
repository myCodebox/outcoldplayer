// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using System.Globalization;
    using System.Diagnostics;

    public interface ISongsService
    {
        Task UpdateRatingAsync(Song song, byte newRating);

        Task<IList<Song>> AddToLibraryAsync(IList<Song> songs);

        Task<IList<Song>> RemoveFromLibraryAsync(IList<Song> songs);
    }

    public class SongsService : ISongsService
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ISongsWebService songsWebService;
        private readonly ISongsRepository songsRepository;
        private readonly IUserPlaylistsRepository userPlaylistsRepository;

        private readonly ILogger logger;

        public SongsService(
            ILogManager logManager,
            IEventAggregator eventAggregator,
            ISongsWebService songsWebService,
            ISongsRepository songsRepository,
            IUserPlaylistsRepository userPlaylistsRepository)
        {
            this.eventAggregator = eventAggregator;
            this.songsWebService = songsWebService;
            this.songsRepository = songsRepository;
            this.userPlaylistsRepository = userPlaylistsRepository;
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

        public async Task<IList<Song>> AddToLibraryAsync(IList<Song> songs)
        {
            var response = await this.songsWebService.AddSongsAsync(songs);
            if (response == null || response.MutateResponse == null || response.MutateResponse.Length != songs.Count)
            {
                return null;
            }

            List<Song> updates = new List<Song>();
            List<Song> inserts = new List<Song>();

            for (int i = 0; i < songs.Count; i++)
            {
                if (string.Equals(response.MutateResponse[i].Response_Code, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    Song song = songs[i];
                    if (song.UnknownSong)
                    {
                        inserts.Add(song);
                    }
                    else
                    {
                        int count = await this.songsRepository.AddSongToLibraryAsync(response.MutateResponse[i].Id, response.MutateResponse[i].Client_Id, song.SongId);
                        Debug.Assert(count == 1, "One song should be updated!");
                    }

                    song.IsLibrary = true;
                    song.UnknownSong = false;
                    song.SongId = response.MutateResponse[i].Id;
                    song.ClientId = response.MutateResponse[i].Client_Id;
                }
            }

            int insertsCount = await this.songsRepository.InsertAsync(inserts);
            int updatesCount = await this.songsRepository.UpdateAsync(updates);

            if (insertsCount != inserts.Count || updatesCount != updates.Count)
            {
                return null;
            }

            this.eventAggregator.Publish(new SongsUpdatedEvent(songs));

            return songs;
        }

        public async Task<IList<Song>> RemoveFromLibraryAsync(IList<Song> songs)
        {
            var response = await this.songsWebService.RemoveSongsAsync(songs);
            if (response == null || response.MutateResponse == null || response.MutateResponse.Length != songs.Count)
            {
                return null;
            }

            List<Song> updates = new List<Song>();
            List<Song> deletes = new List<Song>();

            foreach (var song in songs)
            {
                int keeped = await this.songsRepository.RemoveFromLibraryAsync(song.SongId, song.StoreId);

                song.IsLibrary = false;
                song.SongId = song.StoreId;
                song.ClientId = null;
                if (keeped == 0)
                {
                    song.UnknownSong = false;
                }
            }

            this.eventAggregator.Publish(new SongsUpdatedEvent(songs));

            return songs;
        }
    }
}