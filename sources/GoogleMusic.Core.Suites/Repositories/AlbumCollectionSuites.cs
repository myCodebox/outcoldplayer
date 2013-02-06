// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Repositories
{
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Suites.Services;

    public class AlbumCollectionSuites : SuitesBase
    {
        private Mock<ISongsRepository> songsRepository;
        private AlbumCollection albumCollection;

        public override void SetUp()
        {
            base.SetUp();

            this.songsRepository = new Mock<ISongsRepository>();
            this.songsRepository.Setup(x => x.GetAll()).Returns(SongStubs.GetAllSongs);

            this.albumCollection = new AlbumCollection(this.songsRepository.Object);
        }

        [Test]
        public async void GetAll_SetSongsRepository_ReturnsAllAlbums()
        {
            // Act
            var albums = (await this.albumCollection.GetAllAsync()).ToList();

            // Assert
            Assert.AreEqual(3, albums.Count);
            Assert.IsTrue(albums.Any(a => string.Equals(a.Artist, SongStubs.Artist1) && string.Equals(a.Title, SongStubs.Artist1Album1) && a.Songs.Count == 2));
            Assert.IsTrue(albums.Any(a => string.Equals(a.Artist, SongStubs.Artist1) && string.Equals(a.Title, SongStubs.Artist1Album2) && a.Songs.Count == 2));
            Assert.IsTrue(albums.Any(a => string.Equals(a.Artist, SongStubs.Artist2) && string.Equals(a.Title, SongStubs.Artist2Album1) && a.Songs.Count == 2));
        }

        [Test]
        public async void GetAll_OrderByLastPlayed_CollectionOrderedByName()
        {
            // Act
            var albums = (await this.albumCollection.GetAllAsync(Order.LastPlayed)).ToList();

            // Assert
            Assert.AreEqual(3, albums.Count);
            var albumsOrdered = albums.OrderByDescending(a => a.Songs.Max(s => s.GoogleMusicMetadata.LastPlayed)).ToList();

            Assert.AreSame(albumsOrdered[0], albums[0]);
            Assert.AreSame(albumsOrdered[1], albums[1]);
            Assert.AreSame(albumsOrdered[2], albums[2]);
        }

        [Test]
        public async void GetCount_SetSongsRepository_ReturnsCountOfAllAlbums()
        {
            // Act
            var albumsCount = await this.albumCollection.CountAsync();

            // Assert
            Assert.AreEqual(3, albumsCount);
        }
    }
}