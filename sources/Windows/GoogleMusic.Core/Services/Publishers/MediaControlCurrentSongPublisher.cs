// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Media;

    public class MediaControlCurrentSongPublisher : ICurrentSongPublisher
    {
        private readonly IDispatcher dispatcher;

        public MediaControlCurrentSongPublisher(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
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
                        MediaControl.ArtistName = song.GetSongArtist();
                        MediaControl.TrackName = song.Title;
                        MediaControl.AlbumArt = albumArtUri;
                    });
        }
    }
}