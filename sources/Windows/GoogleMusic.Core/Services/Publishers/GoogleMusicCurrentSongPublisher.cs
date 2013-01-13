// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web;

    public class GoogleMusicCurrentSongPublisher : ICurrentSongPublisher
    {
        private readonly ILogger logger;
        private readonly ISongWebService songWebService;
        private readonly IDispatcher dispatcher;

        public GoogleMusicCurrentSongPublisher(
            ILogManager logManager,
            ISongWebService songWebService,
            IDispatcher dispatcher)
        {
            this.logger = logManager.CreateLogger("GoogleMusicCurrentSongPublisher");
            this.songWebService = songWebService;
            this.dispatcher = dispatcher;
        }

        public PublisherType PublisherType
        {
            get { return PublisherType.Delay; }
        }

        public async Task PublishAsync(Song song, Playlist currentPlaylist, CancellationToken cancellationToken)
        {
            this.logger.Debug("PublishAsync: Publishing song playing to GoogleMusic services. SongId: {0}.", song.GoogleMusicMetadata.Id);

            cancellationToken.ThrowIfCancellationRequested();

            int playCount = song.PlayCount + 1;

            string playlistId = song.GoogleMusicMetadata.Title;
            bool updateRecentAlbum = false;
            bool updateRecentPlaylist = false;

            if (currentPlaylist != null)
            {
                if (currentPlaylist is MusicPlaylist)
                {
                    updateRecentPlaylist = true;
                    playlistId = ((MusicPlaylist)currentPlaylist).Id;
                }
                else if (currentPlaylist is Album || currentPlaylist is Genre || currentPlaylist is Artist || currentPlaylist is SystemPlaylist)
                {
                    updateRecentAlbum = true;
                    playlistId = string.Format("{0} - {1}", song.GoogleMusicMetadata.Artist, song.GoogleMusicMetadata.Album);
                }
                else
                {
                    Debug.Assert(false, "Unknown type of playlist!");
                }
            }

            await Task.WhenAll(
                this.dispatcher.RunAsync(() =>
                    { 
                        song.PlayCount = playCount;

                        // This is not the same value which Google Gives Us, but bigger than Google gives us
                        song.GoogleMusicMetadata.LastPlayed = DateTime.Now.ToFileTime();
                    }),
                Task.Run(async () =>
                    {
                        if (!(await this.songWebService.RecordPlayingAsync(song.GoogleMusicMetadata, playlistId, updateRecentAlbum, updateRecentPlaylist, playCount)))
                        {
                            this.logger.Warning("PublishAsync: Could not update GoogleMusic services for SongId: {0}.", song.GoogleMusicMetadata.Id);
                        }
                    }));

            this.logger.Debug("PublishAsync: Song playing published to GoogleMusic services. SongId: {0}. Plays count: {1}.", song.GoogleMusicMetadata.Id, playCount);
        }
    }
}