// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.WebServices
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    using Xunit;

    public class AuthorizationServiceSuites : SuitesBase
    {
        private API api = new API();

        [Fact(Skip = "Web Service")]
        public async Task Authorize_SetCredentials_CookiesLoaded()
        {
            var authorizationDataService = new Mock<IUserAuthorizationDataService>();
                authorizationDataService.Setup(s => s.GetUserSecurityDataAsync()).Returns(
                    () => Task.Factory.StartNew(() => new UserInfo() { Email = "outcoldman.test@gmail.com", Password = "Qw12er34" }));

            var cookieManager = new Mock<ICookieManager>();
            cookieManager.Setup(m => m.GetCookies()).Returns(() => null);

            using (var registration = this.Container.Registration())
            {
                registration.Register<IUserAuthorizationDataService>()
                    .AsSingleton(authorizationDataService.Object);

                registration.Register<IClientLoginService>().As<ClientLoginService>();
                registration.Register<IGoogleWebService>().As<GoogleWebService>();

                registration.Register<ICookieManager>().AsSingleton(cookieManager.Object);
            }

            var service = this.Container.Resolve<AuthorizationService>();
            await service.AuthorizeAsync();

            cookieManager.Verify(x => x.SaveCookies(It.IsAny<Uri>(), It.IsAny<CookieCollection>()), Times.Once());
        }

        [Fact(Skip = "Web Service")]
        public async Task Login_DefaultCredentials_Init()
        {
            object a = null;

            var service = this.Container.Resolve<ClientLoginService>();
            var webResponse = await service.LoginAsync("outcoldman.test@gmail.com", "Qw12er34");

            

            //var webResponse = httpWebRequest.GetResponse();
            

            GoogleMusicPlaylists playlists = null;

            api.Login("outcoldman.test@gmail.com", "Qw12er34");

            api.OnLoginComplete += (sender, args) => this.api.GetPlaylist();

            api.OnGetPlaylistsComplete += pls =>
                { 
                    playlists = pls;
                };

            while (playlists == null && a == null)
            {
                Thread.Sleep(100);
            }

            Assert.NotEmpty(playlists.Playlists);
        }
    }
}