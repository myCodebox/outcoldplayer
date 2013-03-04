// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
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

        public async Task PublishAsync(Song song, Playlist currentPlaylist, Uri albumArtUri, CancellationToken cancellationToken)
        {
            this.logger.Debug("PublishAsync: Publishing song playing to GoogleMusic services. SongId: {0}.", song.Metadata.Id);

            cancellationToken.ThrowIfCancellationRequested();

            ushort playCount = (ushort)(song.PlayCount + 1);

            string playlistId = song.Metadata.Title;
            bool updateRecentAlbum = false;
            bool updateRecentPlaylist = false;

            if (currentPlaylist != null)
            {
                if (currentPlaylist is MusicPlaylist)
                {
                    updateRecentPlaylist = true;
                    playlistId = ((MusicPlaylist)currentPlaylist).Id.ToString();
                }
                else 
                {
                    updateRecentAlbum = true;
                    playlistId = string.Format("{0} - {1}", song.Metadata.Artist, song.Metadata.Album);
                }
            }

            await Task.WhenAll(
                this.dispatcher.RunAsync(() =>
                    { 
                        song.PlayCount = playCount;
                        song.Metadata.LastPlayed = DateTime.Now;
                    }),
                Task.Run(async () =>
                    {
                        if (!(await this.songWebService.RecordPlayingAsync(song.Metadata.Id, playlistId, updateRecentAlbum, updateRecentPlaylist, playCount)))
                        {
                            this.logger.Warning("PublishAsync: Could not update GoogleMusic services for SongId: {0}.", song.Metadata.Id);
                        }
                    }));

            this.logger.Debug("PublishAsync: Song playing published to GoogleMusic services. SongId: {0}. Plays count: {1}.", song.Metadata.Id, playCount);
        }
    }
}