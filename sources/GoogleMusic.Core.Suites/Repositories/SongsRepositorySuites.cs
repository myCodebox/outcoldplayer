// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Repositories
{
    using System;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class SongsRepositorySuites : SuitesBase
    {
        private readonly SongMetadata song1 = new SongMetadata() { Id = Guid.NewGuid() };
        private readonly SongMetadata song2 = new SongMetadata() { Id = Guid.NewGuid() };

        private SongsRepository repository;

        public override void SetUp()
        {
            base.SetUp();

            this.repository = new SongsRepository(this.LogManager, Mock.Of<ISongWebService>(), Mock.Of<IGoogleMusicSessionService>());
        }

        [Test]
        public void AddRange_AddListOfNewSongs_SongsAreAdded()
        {
            // Act
            this.repository.AddRange(new[] { this.song1, this.song2 });

            // Assert
            var songs = this.repository.GetAll().ToList();
            Assert.AreEqual(2, songs.Count);
            Assert.IsTrue(songs.Any(x => x.Metadata.Id == this.song1.Id));
            Assert.IsTrue(songs.Any(x => x.Metadata.Id == this.song2.Id));
        }

        [Test]
        public void GetAll_ReturnsIndependedCollections()
        {
            // Act
            var songs1 = this.repository.GetAll();
            this.repository.AddOrUpdate(this.song1);
            var songs2 = this.repository.GetAll();

            // Assert
            Assert.AreEqual(0, songs1.Count());
            Assert.AreEqual(1, songs2.Count());
        }

        [Test]
        public void AddOrUpdate_NewSong_SongShouldBeAdded()
        {
            // Act
            Song song = this.repository.AddOrUpdate(this.song1);

            // Assert
            Assert.NotNull(song);
            Assert.IsTrue(this.repository.GetAll().Any(x => x.Metadata.Id == this.song1.Id));
        }

        [Test]
        public void AddOrUpdate_ExistingSong_SongShouldBeUpdated()
        {
            this.repository.AddRange(new[] { this.song1 });

            // Act
            Song song = this.repository.AddOrUpdate(this.song1);

            // Assert
            Assert.NotNull(song);
            Assert.IsTrue(this.repository.GetAll().Any(x => x.Metadata.Id == this.song1.Id));
            Assert.AreEqual(1, this.repository.GetAll().Count());
        }
    }
}