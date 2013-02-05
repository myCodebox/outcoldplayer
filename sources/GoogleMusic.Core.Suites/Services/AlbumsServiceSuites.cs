// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Services
{
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;

    public class AlbumsServiceSuites : SuitesBase
    {
        private Mock<ISongsRepository> songsRepository;
        private AlbumsService albumsService;

        public override void SetUp()
        {
            base.SetUp();

            this.songsRepository = new Mock<ISongsRepository>();
            this.songsRepository.Setup(x => x.GetAll()).Returns(SongStubs.GetAllSongs);

            this.albumsService = new AlbumsService(this.songsRepository.Object);
        }

        [Test]
        public void GetAll_SetSongsRepository_ReturnsAllAlbums()
        {
            // Act
            var albums = this.albumsService.GetAll().ToList();

            // Assert
            Assert.AreEqual(3, albums.Count);
            Assert.IsTrue(albums.Any(a => string.Equals(a.Artist, SongStubs.Artist1) && string.Equals(a.Title, SongStubs.Artist1Album1) && a.Songs.Count == 2));
            Assert.IsTrue(albums.Any(a => string.Equals(a.Artist, SongStubs.Artist1) && string.Equals(a.Title, SongStubs.Artist1Album2) && a.Songs.Count == 2));
            Assert.IsTrue(albums.Any(a => string.Equals(a.Artist, SongStubs.Artist2) && string.Equals(a.Title, SongStubs.Artist2Album1) && a.Songs.Count == 2));
        }

        [Test]
        public void GetCount_SetSongsRepository_ReturnsCountOfAllAlbums()
        {
            // Act
            var albumsCount = this.albumsService.Count();

            // Assert
            Assert.AreEqual(3, albumsCount);
        }
    }
}