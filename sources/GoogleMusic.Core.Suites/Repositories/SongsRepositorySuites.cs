// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Repositories
{
    using System;

    using Moq;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;

    public class SongsRepositorySuites : GoogleMusicSuitesBase
    {
        private readonly SongMetadata song1 = new SongMetadata() { Id = Guid.NewGuid().ToString() };
        private readonly SongMetadata song2 = new SongMetadata() { Id = Guid.NewGuid().ToString() };

        private SongsRepository repository;

        public override void SetUp()
        {
            base.SetUp();

            this.repository = new SongsRepository(this.LogManager, Mock.Of<ISongWebService>(), Mock.Of<IGoogleMusicSessionService>(), Mock.Of<ISongsCacheService>());
        }
    }
}