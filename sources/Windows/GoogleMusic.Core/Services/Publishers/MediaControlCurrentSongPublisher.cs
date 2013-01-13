// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Media;
    using Windows.Storage;

    public class MediaControlCurrentSongPublisher : ICurrentSongPublisher
    {
        private const string AlbumArtCacheFolder = "AlbumArtCache";
        private const string CurrentAlbumArtFile = "current.jpg";

        private readonly ILogger logger;
        private readonly IDispatcher dispatcher;

        private readonly HttpClient client = new HttpClient();

        public MediaControlCurrentSongPublisher(
            ILogManager logManager,
            IDispatcher dispatcher)
        {
            this.logger = logManager.CreateLogger("MediaControlCurrentSongPublisher");
            this.dispatcher = dispatcher;
        }

        public PublisherType PublisherType
        {
            get
            {
                return PublisherType.Immediately;
            }
        }

        public async Task PublishAsync(Song song, Playlist currentPlaylist, CancellationToken cancellationToken)
        {
            Uri albumArtUri = new Uri("ms-appx:///Assets/Logo.png");

            try
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Song album art: {0}.", song.GoogleMusicMetadata.AlbumArtUrl);
                }

                if (!string.IsNullOrEmpty(song.GoogleMusicMetadata.AlbumArtUrl))
                {
                    byte[] bytes = await this.client.GetByteArrayAsync("http:" + song.GoogleMusicMetadata.AlbumArtUrl);
                    var localFolder = ApplicationData.Current.LocalFolder;

                    var folder = await localFolder.CreateFolderAsync(AlbumArtCacheFolder, CreationCollisionOption.OpenIfExists);

                    var file = await folder.CreateFileAsync(CurrentAlbumArtFile, CreationCollisionOption.ReplaceExisting);

                    await FileIO.WriteBytesAsync(file, bytes);

                    albumArtUri = new Uri(string.Format(CultureInfo.InvariantCulture, "ms-appdata:///local/{0}/{1}", AlbumArtCacheFolder, CurrentAlbumArtFile));
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("Cannot download album art.");
                this.logger.LogErrorException(exception);
            }

            await this.dispatcher.RunAsync(
                () =>
                    {
                        MediaControl.ArtistName = song.Artist;
                        MediaControl.TrackName = song.Title;
                        MediaControl.AlbumArt = albumArtUri;
                    });
        }
    }
}