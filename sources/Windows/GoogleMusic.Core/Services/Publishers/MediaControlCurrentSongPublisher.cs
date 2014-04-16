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

    public class MediaControlCurrentSongPublisher : ICurrentSongPublisher
    {
        private readonly SystemMediaTransportControls systemMediaTransportControls;

        private readonly IDispatcher dispatcher;

        public MediaControlCurrentSongPublisher(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
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
                    var updater = this.systemMediaTransportControls.DisplayUpdater;
                    updater.MusicProperties.Title = song.Title;
                    updater.MusicProperties.AlbumArtist = song.AlbumArtistTitle;
                    updater.MusicProperties.Artist = song.GetSongArtist();
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
                    updater.Update();
                });
        }
    }
}