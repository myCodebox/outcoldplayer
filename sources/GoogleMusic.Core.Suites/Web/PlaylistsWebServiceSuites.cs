// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;

    [Category("Integration")]
    public class PlaylistsWebServiceSuites : GoogleMusicSuitesBase
    {
        private IGoogleAccountWebService googleAccountWebService;
        private IGoogleMusicWebService musicWebService;
        private IPlaylistsWebService playlistsWebService;
        private ISongWebService songsWebService;

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
                registration.Register<ISongWebService>().AsSingleton<SongWebService>();
            }

            this.googleAccountWebService = this.Container.Resolve<IGoogleAccountWebService>();
            this.musicWebService = this.Container.Resolve<IGoogleMusicWebService>();
            this.playlistsWebService = this.Container.Resolve<IPlaylistsWebService>();
            this.songsWebService = this.Container.Resolve<ISongWebService>();
        }

        [Test]
        public async Task StreamingLoadAllTracksAsync_AuthentificateAndExecute_ListOfSongs()
        {
            await this.AuthenticateAsync();

            var songs = await this.songsWebService.StreamingLoadAllTracksAsync(null, null);

            Assert.IsTrue(songs.Count > 0);
        }

        [Test]
        public async Task GetAllSongsAsync_AuthentificateAndExecute_ListOfSongs()
        {
            await this.AuthenticateAsync();

            var songs = await this.songsWebService.GetAllSongsAsync(null);

            Assert.IsTrue(songs.Count > 0);
        }

        [Test]
        public async Task PlaylistsManipulations_AddModifyDelete()
        {
            await this.AuthenticateAsync();

            var playlists = await this.playlistsWebService.GetAllAsync();

            string playlistName = Guid.NewGuid().ToString();

            // Create new playlist
            var playlistResp = await this.playlistsWebService.CreateAsync(playlistName);
            Assert.NotNull(playlistResp, "Playlist is null");
            Assert.AreEqual(playlistName, playlistResp.Title, "The name of playlist is not the same");
            Assert.NotNull(playlistResp.Id, "We do not have Id of playlist");

            // Verify that playlist has been created
            var playlists2 = await this.playlistsWebService.GetAllAsync();
            Assert.IsTrue((playlists2.Playlists.Count - playlists.Playlists.Count) == 1, "Playlist has been added");
            Assert.IsTrue(playlists2.Playlists.Any(p => Guid.Parse(p.PlaylistId) == playlistResp.Id && p.Title == playlistName), "Could not find playlist");
         
            // Changing the name of playlist
            string playlistName2 = Guid.NewGuid().ToString();
            bool result = await this.playlistsWebService.ChangeNameAsync(playlistResp.Id, playlistName2);
            Assert.IsTrue(result, "Name of playlist was not changed");

            // Verify that name has been changed.
            var playlists3 = await this.playlistsWebService.GetAllAsync();
            Assert.IsTrue((playlists3.Playlists.Count - playlists.Playlists.Count) == 1);
            Assert.IsFalse(playlists3.Playlists.Any(p => Guid.Parse(p.PlaylistId) == playlistResp.Id && p.Title == playlistName), "We still have playlist with old name.");
            Assert.IsTrue(playlists3.Playlists.Any(p => Guid.Parse(p.PlaylistId) == playlistResp.Id && p.Title == playlistName2), "Could not find playlist with new name.");

            // Adding songs to playlist
            var songs = await this.songsWebService.GetAllSongsAsync();
            var googleMusicSong = songs.First();
            var resultAddSongs = await this.playlistsWebService.AddSongAsync(playlistResp.Id, new [] { googleMusicSong.Id });
            Assert.IsTrue(resultAddSongs.SongIds[0].PlaylistEntryId != Guid.Empty, "Song has been added to playlist");

            // Verify that songs has been added to playlist
            var playlist = await this.playlistsWebService.GetAsync(playlistResp.Id);
            Assert.AreEqual(googleMusicSong.Id, playlist.Playlist[0].Id, "Playlist contains song.");

            // Remove song from playlist
            var resultSongRemove = await this.playlistsWebService.RemoveSongAsync(playlistResp.Id, playlist.Playlist[0].Id, playlist.Playlist[0].PlaylistEntryId);
            Assert.IsTrue(resultSongRemove, "Song has been removed.");

            // Verify that song has been removed.
            var playlist2 = await this.playlistsWebService.GetAsync(playlistResp.Id);
            Assert.AreEqual(0, playlist2.Playlist.Count);

            // Delete playlist
            bool resultDelete = await this.playlistsWebService.DeleteAsync(playlistResp.Id);
            Assert.IsTrue(resultDelete, "Playlist has been deleted.");

            // Verify that playlist has been deleted.
            var playlists4 = await this.playlistsWebService.GetAllAsync();
            Assert.IsTrue(playlists.Playlists.Count == playlists4.Playlists.Count);
            Assert.IsFalse(playlists4.Playlists.Any(p => Guid.Parse(p.PlaylistId) == playlistResp.Id));
        }

        private async Task AuthenticateAsync()
        {
            var googleLoginResponse = await this.googleAccountWebService.AuthenticateAsync(new Uri(this.musicWebService.GetServiceUrl()), SuitesConstants.GoogleAccountName, SuitesConstants.GoogleAccountPassword);
            this.musicWebService.Initialize(googleLoginResponse.CookieCollection.Cast<Cookie>());
            // await this.musicWebService.AuthenticateAsync(SuitesConstants.GoogleAccountName, SuitesConstants.GoogleAccountPassword);
        }
    }
}