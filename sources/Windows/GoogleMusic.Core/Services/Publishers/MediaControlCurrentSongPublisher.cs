// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Windows.Storage.Streams;

    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Media;

    using OutcoldSolutions.GoogleMusic.Shell;

    public class MediaControlCurrentSongPublisher : ICurrentSongPublisher
    {
        private readonly IDispatcher dispatcher;

        private readonly IMediaControlIntegration mediaControlIntegration;

        public MediaControlCurrentSongPublisher(IDispatcher dispatcher, IMediaControlIntegration mediaControlIntegration)
        {
            this.dispatcher = dispatcher;
            this.mediaControlIntegration = mediaControlIntegration;
        }

        public PublisherType PublisherType
        {
            get
            {
                return PublisherType.ImmediatelyWithAlbumArt;
            }
        }

        public async Task PublishAsync(Song song, IPlaylist currentPlaylist, Uri albumArtUri, CancellationToken cancellationToken)
        {
            await this.dispatcher.RunAsync(
                () =>
                    {
                    var updater = this.mediaControlIntegration.GetSystemMediaTransportControls().DisplayUpdater;
                    updater.Type = MediaPlaybackType.Music;
                    updater.MusicProperties.Title = song.Title;
                    updater.MusicProperties.AlbumArtist = song.AlbumArtistTitle;
                    updater.MusicProperties.Artist = song.GetSongArtist();
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
                    updater.Update();
                    });
        }
    }
}