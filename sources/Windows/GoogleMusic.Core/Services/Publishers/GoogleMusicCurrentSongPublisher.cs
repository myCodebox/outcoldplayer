// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;

    public class GoogleMusicCurrentSongPublisher : ICurrentSongPublisher
    {
        private readonly ILogger logger;
        private readonly ISongsWebService songsWebService;
        private readonly ISongsRepository songsRepository;
        private readonly IEventAggregator eventAggregator;

        public GoogleMusicCurrentSongPublisher(
            ILogManager logManager,
            ISongsWebService songsWebService,
            ISongsRepository songsRepository,
            IEventAggregator eventAggregator)
        {
            this.logger = logManager.CreateLogger("GoogleMusicCurrentSongPublisher");
            this.songsWebService = songsWebService;
            this.songsRepository = songsRepository;
            this.eventAggregator = eventAggregator;
        }

        public PublisherType PublisherType
        {
            get { return PublisherType.Delay; }
        }

        public async Task PublishAsync(Song song, IPlaylist currentPlaylist, Uri albumArtUri, CancellationToken cancellationToken)
        {
            this.logger.Debug("PublishAsync: Publishing song playing to GoogleMusic services. ProviderSongId: {0}.", song.ProviderSongId);

            cancellationToken.ThrowIfCancellationRequested();

            ushort playCount = (ushort)(song.PlayCount + 1);

            string playlistId = song.Title;
            bool updateRecentAlbum = false;
            bool updateRecentPlaylist = false;

            if (currentPlaylist != null)
            {
                var userPlaylist = currentPlaylist as UserPlaylist;
                if (userPlaylist != null)
                {
                    updateRecentPlaylist = true;
                    playlistId = userPlaylist.ProviderPlaylistId;
                }
                else 
                {
                    updateRecentAlbum = true;
                    playlistId = string.Format("{0} - {1}", song.ArtistTitle, song.AlbumTitle);
                }
            }

            await Task.WhenAll(
                this.RecordLocalAsync(song, playCount),
                this.RecordOnServerAsync(song.ProviderSongId, playlistId, updateRecentAlbum, updateRecentPlaylist, playCount));

            this.logger.Debug("PublishAsync: Song playing published to GoogleMusic services. ProviderSongId: {0}. Plays count: {1}.", song.ProviderSongId, playCount);
        }

        private async Task RecordLocalAsync(Song song, ushort playCount)
        {
            song.PlayCount = playCount;
            song.LastPlayed = DateTime.UtcNow;

            var songs = new[] { song };
            await this.songsRepository.UpdateAsync(new[] { song });
            this.eventAggregator.Publish(new SongsUpdatedEvent(songs));
        }

        private async Task RecordOnServerAsync(string googleSongId, string playlistId, bool updateRecentAlbum, bool updateRecentPlaylist, int playCount)
        {
            try
            {
                if (!(await this.songsWebService.RecordPlayingAsync(googleSongId, playlistId, updateRecentAlbum, updateRecentPlaylist, playCount)))
                {
                    this.logger.Warning("PublishAsync: Could not update GoogleMusic services for ProviderSongId: {0}.", googleSongId);
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Exceptin when we tried to update play count on server.");
                this.logger.LogErrorException(e);
            }
        }
    }
}