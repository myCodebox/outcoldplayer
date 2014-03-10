// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;

    public class GoogleMusicCurrentSongPublisher : ICurrentSongPublisher
    {
        private readonly ILogger logger;
        private readonly ISongsRepository songsRepository;
        private readonly ISongsWebService songsWebService;
        private readonly IEventAggregator eventAggregator;

        public GoogleMusicCurrentSongPublisher(
            ILogManager logManager,
            ISongsRepository songsRepository,
            ISongsWebService songsWebService,
            IEventAggregator eventAggregator)
        {
            this.logger = logManager.CreateLogger("GoogleMusicCurrentSongPublisher");
            this.songsRepository = songsRepository;
            this.songsWebService = songsWebService;
            this.eventAggregator = eventAggregator;
        }

        public PublisherType PublisherType
        {
            get { return PublisherType.Delay; }
        }

        public async Task PublishAsync(Song song, IPlaylist currentPlaylist, Uri albumArtUri, CancellationToken cancellationToken)
        {
            this.logger.Debug("PublishAsync: Saving stats for song: {0}.", song.SongId);

            cancellationToken.ThrowIfCancellationRequested();

            song.StatsPlayCount ++;
            song.PlayCount++;
            song.Recent = song.StatsRecent = DateTime.UtcNow;

            if (song.UnknownSong)
            {
                await this.songsWebService.SendStatsAsync(new [] { song });
            }
            else
            {
                await this.songsRepository.RecordPlayStatAsync(song);
            }
            
            this.eventAggregator.Publish(new SongsUpdatedEvent(new[] { song }));

            this.logger.Debug("PublishAsync: stats has been saved for song {0}.", song.SongId);
        }
    }
}