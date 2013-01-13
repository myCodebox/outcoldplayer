// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Services.Pubslishers
{
    using System.Collections.Generic;
    using System.Threading;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class CurrentSongPublisherServiceSuites : SuitesBase
    {
        private Song song;
        private Playlist playlist;
        private Mock<ISettingsService> settingsService;

        public override void SetUp()
        {
            base.SetUp();

            this.song = new Song(new GoogleMusicSong());
            this.playlist = new Album(new List<Song>());

            this.settingsService = new Mock<ISettingsService>();
        }

        [Test]
        public async void PublishAsync_SetSongAndPlaylist_ShouldPassRightArgumentsToPublishers()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(10);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object);

            var publisher = new Mock<ICurrentSongPublisher>();
            publisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Immediately);
            service.AddPublisher(publisher.Object);

            // Act
            await service.PublishAsync(this.song, this.playlist);

            // Assert
            publisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async void PublishAsync_CancelOnDelayAndImmediatelyPublishers_OnlyImmediatelyPublisherWorks()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(1000);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object);

            var immidiatelyPublisher = new Mock<ICurrentSongPublisher>();
            immidiatelyPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Immediately);
            service.AddPublisher(immidiatelyPublisher.Object);

            var delayPublisher = new Mock<ICurrentSongPublisher>();
            delayPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Delay);
            service.AddPublisher(delayPublisher.Object);

            // Act
            var task = service.PublishAsync(this.song, this.playlist);

            service.CancelActiveTasks();

            await TaskEx.WhenAllSafe(task);

            // Assert
            immidiatelyPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<CancellationToken>()), Times.Once());
            delayPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async void PublishAsync_DelayAndImmediatelyPublishers_BothPublishersNotified()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(1);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object);

            var immidiatelyPublisher = new Mock<ICurrentSongPublisher>();
            immidiatelyPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Immediately);
            service.AddPublisher(immidiatelyPublisher.Object);

            var delayPublisher = new Mock<ICurrentSongPublisher>();
            delayPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Delay);
            service.AddPublisher(delayPublisher.Object);

            // Act
            await service.PublishAsync(this.song, this.playlist);

            // Assert
            immidiatelyPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<CancellationToken>()), Times.Once());
            delayPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async void PublishAsync_PublishRightAfterPublish_SecondCommandCancelPrevious()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(100);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object);

            var delayPublisher = new Mock<ICurrentSongPublisher>();
            delayPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Delay);
            service.AddPublisher(delayPublisher.Object);

            Song song2 = new Song(new GoogleMusicSong());

            // Act
            var publish1 = service.PublishAsync(this.song, this.playlist);
            var publish2 = service.PublishAsync(song2, this.playlist);

            await TaskEx.WhenAllSafe(publish1, publish2);

            // Assert
            delayPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<CancellationToken>()), Times.Never());
            delayPublisher.Verify(p => p.PublishAsync(song2, this.playlist, It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
