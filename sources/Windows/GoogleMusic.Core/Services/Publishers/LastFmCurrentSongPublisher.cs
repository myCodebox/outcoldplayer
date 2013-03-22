// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Lastfm;
    using OutcoldSolutions.Models;

    public class LastFmCurrentSongPublisher : ICurrentSongPublisher
    {
        private readonly ILastfmWebService webService;

        public LastFmCurrentSongPublisher(ILastfmWebService webService)
        {
            this.webService = webService;
        }

        public PublisherType PublisherType
        {
            get { return PublisherType.Immediately; }
        }

        public async Task PublishAsync(Song song, IPlaylist currentPlaylist, Uri imageUri, CancellationToken cancellationToken)
        {
            var startPlaying = DateTime.UtcNow;

            cancellationToken.ThrowIfCancellationRequested();

            var parameters = new Dictionary<string, string>()
                                 {
                                     { "artist", song.Artist.Title },
                                     { "track", song.Title },
                                     { "album", song.Album.Title },
                                     { "trackNumber", song.Track.HasValue ? song.Track.Value.ToString("D") : string.Empty },
                                     { "duration", ((int)song.Duration.TotalSeconds).ToString("D") }
                                 };

            if (!string.IsNullOrEmpty(song.AlbumArtist)
                && string.Equals(song.AlbumArtist, song.Artist.Title, StringComparison.OrdinalIgnoreCase))
            {
                parameters.Add("albumArtist", song.AlbumArtist);
            }

            Task nowPlayingTask = this.webService.CallAsync("track.updateNowPlaying", new Dictionary<string, string>(parameters));

            cancellationToken.ThrowIfCancellationRequested();

            // Last.fm only accept songs with > 30 seconds
            if (song.Duration.TotalSeconds >= 30)
            {
                // 4 minutes or half of the track
                await Task.Delay(Math.Min(4 * 60 * 1000, (int)(song.Duration.TotalMilliseconds / 2)), cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var scrobbleParameters = new Dictionary<string, string>(parameters)
                                             {
                                                 { "timestamp", ((int)(startPlaying.ToUnixFileTime() / 1000)).ToString("D") }
                                             };

                await this.webService.CallAsync("track.scrobble", scrobbleParameters);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await nowPlayingTask;
        }
    }
}