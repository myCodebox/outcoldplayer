// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class GoogleAccountWebServiceSuites : SuitesBase
    {
        private GoogleAccountWebService googleAccountWebService;
        private GoogleMusicWebService musicWebService;
        private PlaylistsWebService playlistsWebService;

        private Mock<IGoogleMusicSessionService> sessionService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            this.sessionService = new Mock<IGoogleMusicSessionService>();
            this.sessionService.Setup(x => x.GetSession()).Returns(new UserSession());

            var logManager = this.Container.Resolve<ILogManager>();
            this.googleAccountWebService = new GoogleAccountWebService(logManager);
            this.musicWebService = new GoogleMusicWebService(logManager, this.sessionService.Object);
            this.playlistsWebService = new PlaylistsWebService(this.musicWebService, this.sessionService.Object);

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        [Test, Ignore]
        public async Task LoginAsync_RightCredentials_GetAuth()
        {
            var googleLoginResponse = await this.googleAccountWebService.Authenticate("outcoldman.test@gmail.com", "Qw12er34");

            Assert.IsTrue(googleLoginResponse.Success);
            Assert.IsNull(googleLoginResponse.Error);
        }

        [Test, Ignore]
        public async Task LoginAsync_WrongCredentials_GetAuth()
        {
            var googleLoginResponse = await this.googleAccountWebService.Authenticate("outcoldman.test@gmail.com", "WrongPassword");

            Assert.IsFalse(googleLoginResponse.Success);
            Assert.IsNotNull(googleLoginResponse.Error);
            Assert.AreEqual(GoogleLoginResponse.ErrorResponseCode.BadAuthentication, googleLoginResponse.Error.Value); 
        }

        [Test, Ignore]
        public async Task Test()
        {
            await this.googleAccountWebService.Authenticate("outcoldman@gmail.com", "Stud10w0rks18467");

            var cookies = await this.googleAccountWebService.GetCookiesAsync(this.musicWebService.GetServiceUrl());

            this.musicWebService.Initialize(cookies);

            var songs = await this.playlistsWebService.StreamingLoadAllTracksAsync(null);

            Assert.IsTrue(songs.Count > 0);
        }
    }
}