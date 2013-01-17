// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;

    [Category("Integration")]
    public class PlaylistsWebServiceSuites : SuitesBase
    {
        private IGoogleAccountWebService googleAccountWebService;
        private IGoogleMusicWebService musicWebService;
        private IPlaylistsWebService playlistsWebService;

        private Mock<IGoogleMusicSessionService> sessionService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Create new session
            this.sessionService = new Mock<IGoogleMusicSessionService>();
            this.sessionService.Setup(x => x.GetSession()).Returns(new UserSession());

            // Registration
            using (var registration = this.Container.Registration())
            {
                registration.Register<IGoogleMusicSessionService>().AsSingleton(this.sessionService.Object);
                registration.Register<IGoogleAccountWebService>().AsSingleton<GoogleAccountWebService>();
                registration.Register<IGoogleMusicWebService>().AsSingleton<GoogleMusicWebService>();
                registration.Register<IPlaylistsWebService>().AsSingleton<PlaylistsWebService>();
            }

            this.googleAccountWebService = this.Container.Resolve<IGoogleAccountWebService>();
            this.musicWebService = this.Container.Resolve<IGoogleMusicWebService>();
            this.playlistsWebService = this.Container.Resolve<IPlaylistsWebService>();
        }

        [Test]
        public async Task StreamingLoadAllTracksAsync_AuthentificateAndExecute_ListOfSongs()
        {
            await this.AthenticateAsync();

            var songs = await this.playlistsWebService.StreamingLoadAllTracksAsync(null);

            Assert.IsTrue(songs.Count > 0);
        }

        [Test]
        public async Task GetAllSongsAsync_AuthentificateAndExecute_ListOfSongs()
        {
            await this.AthenticateAsync();

            var songs = await this.playlistsWebService.GetAllSongsAsync(null);

            Assert.IsTrue(songs.Count > 0);
        }

        private async Task AthenticateAsync()
        {
            await this.googleAccountWebService.Authenticate(SuitesConstants.GoogleAccountName, SuitesConstants.GoogleAccountPassword);
            var cookies = await this.googleAccountWebService.GetCookiesAsync(this.musicWebService.GetServiceUrl());
            this.musicWebService.Initialize(cookies);
        }
    }
}