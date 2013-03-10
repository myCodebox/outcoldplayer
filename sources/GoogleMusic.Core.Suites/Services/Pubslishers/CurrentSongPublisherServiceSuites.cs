// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Services.Pubslishers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class CurrentSongPublisherServiceSuites : GoogleMusicSuitesBase
    {
        private Song song;
        private Playlist playlist;
        private Mock<ISettingsService> settingsService;

        public override void SetUp()
        {
            base.SetUp();

            this.song = new Song(new GoogleMusicSong() { DurationMillis = 60000 });
            this.playlist = new Album(new List<Song>());

            this.settingsService = new Mock<ISettingsService>();
        }

        [Test]
        public async void PublishAsync_SetSongAndPlaylist_ShouldPassRightArgumentsToPublishers()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(10);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object, this.Container);

            var publisher = new Mock<CurrentSongPublisher1>();
            publisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Immediately);

            using (var registration = this.Container.Registration())
            {
                registration.Register<CurrentSongPublisher1>().AsSingleton(publisher.Object);
            }

            service.AddPublisher<CurrentSongPublisher1>();

            // Act
            await service.PublishAsync(this.song, this.playlist);

            // Assert
            publisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async void PublishAsync_CancelOnDelayAndImmediatelyPublishers_OnlyImmediatelyPublisherWorks()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(1000);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object, this.Container);

            var immidiatelyPublisher = new Mock<CurrentSongPublisher1>();
            immidiatelyPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Immediately);
            using (var registration = this.Container.Registration())
            {
                registration.Register<CurrentSongPublisher1>().AsSingleton(immidiatelyPublisher.Object);
            }

            service.AddPublisher<CurrentSongPublisher1>();

            var delayPublisher = new Mock<CurrentSongPublisher2>();
            delayPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Delay);
            using (var registration = this.Container.Registration())
            {
                registration.Register<CurrentSongPublisher2>().AsSingleton(delayPublisher.Object);
            }

            service.AddPublisher<CurrentSongPublisher2>();

            // Act
            var task = service.PublishAsync(this.song, this.playlist);

            service.CancelActiveTasks();

            await TaskEx.WhenAllSafe(task);

            // Assert
            immidiatelyPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once());
            delayPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public async void PublishAsync_DelayAndImmediatelyPublishers_BothPublishersNotified()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(1);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object, this.Container);

            var immidiatelyPublisher = new Mock<CurrentSongPublisher1>();
            immidiatelyPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Immediately);
            using (var registration = this.Container.Registration())
            {
                registration.Register<CurrentSongPublisher1>().AsSingleton(immidiatelyPublisher.Object);
            }

            service.AddPublisher<CurrentSongPublisher1>();

            var delayPublisher = new Mock<CurrentSongPublisher2>();
            delayPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Delay);
            using (var registration = this.Container.Registration())
            {
                registration.Register<CurrentSongPublisher2>().AsSingleton(delayPublisher.Object);
            }

            service.AddPublisher<CurrentSongPublisher2>();

            // Act
            await service.PublishAsync(this.song, this.playlist);

            // Assert
            immidiatelyPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once());
            delayPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async void PublishAsync_PublishRightAfterPublish_SecondCommandCancelPrevious()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(100);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object, this.Container);

            var delayPublisher = new Mock<CurrentSongPublisher1>();
            delayPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Delay);
            using (var registration = this.Container.Registration())
            {
                registration.Register<CurrentSongPublisher1>().AsSingleton(delayPublisher.Object);
            }

            service.AddPublisher<CurrentSongPublisher1>();

            Song song2 = new Song(new GoogleMusicSong() { DurationMillis = 60000 });

            // Act
            var publish1 = service.PublishAsync(this.song, this.playlist);
            var publish2 = service.PublishAsync(song2, this.playlist);

            await TaskEx.WhenAllSafe(publish1, publish2);

            // Assert
            delayPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Never());
            delayPublisher.Verify(p => p.PublishAsync(song2, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async void PublishAsync_SmallSongDuration_SongPublished()
        {
            // Arrange
            this.settingsService.Setup(x => x.GetValue("DelayPublishersHoldUp", It.IsAny<int>())).Returns(10000);
            var service = new CurrentSongPublisherService(this.LogManager, this.settingsService.Object, this.Container);

            var delayPublisher = new Mock<CurrentSongPublisher1>();
            delayPublisher.SetupGet(x => x.PublisherType).Returns(PublisherType.Delay);
            using (var registration = this.Container.Registration())
            {
                registration.Register<CurrentSongPublisher1>().AsSingleton(delayPublisher.Object);
            }

            service.AddPublisher<CurrentSongPublisher1>();

            this.song.Metadata.Duration = TimeSpan.FromMilliseconds(100);

            // Act
            await Task.WhenAny(
                service.PublishAsync(this.song, this.playlist),
                Task.Delay((int)this.song.Metadata.Duration.TotalMilliseconds));

            // Assert
            delayPublisher.Verify(p => p.PublishAsync(this.song, this.playlist, It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        public abstract class CurrentSongPublisher1 : ICurrentSongPublisher
        {
            public abstract PublisherType PublisherType { get; }

            public abstract Task PublishAsync(Song song, Playlist currentPlaylist, Uri imageUri, CancellationToken cancellationToken);
        }

        public abstract class CurrentSongPublisher2 : ICurrentSongPublisher
        {
            public abstract PublisherType PublisherType { get; }

            public abstract Task PublishAsync(Song song, Playlist currentPlaylist, Uri imageUri, CancellationToken cancellationToken);
        }
    }
}
