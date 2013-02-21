// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System;
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    [Category("Integration")]
    public class GoogleAccountWebServiceSuites : GoogleMusicSuitesBase
    {
        private GoogleAccountWebService googleAccountWebService;
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
                registration.Register<GoogleAccountWebService>().AsSingleton();
                registration.Register<GoogleMusicWebService>().AsSingleton();
            }

            this.googleAccountWebService = this.Container.Resolve<GoogleAccountWebService>();
        }

        [Test]
        public async Task Authenticate_RightCredentials_Success()
        {
            var googleLoginResponse = await this.googleAccountWebService.AuthenticateAsync(
                                                new Uri("https://play.google.com/music"), 
                                                SuitesConstants.GoogleAccountName, 
                                                SuitesConstants.GoogleAccountPassword);

            Assert.IsTrue(googleLoginResponse.Success);
            Assert.IsNull(googleLoginResponse.Error);
            Assert.IsNotNull(googleLoginResponse.CookieCollection);
        }

        [Test]
        public async Task Authenticate_WrongCredentials_NotSuccess()
        {
            var googleLoginResponse = await this.googleAccountWebService.AuthenticateAsync(
                                                new Uri("https://play.google.com/music"), 
                                                SuitesConstants.GoogleAccountName,
                                                "WrongPassword");

            Assert.IsFalse(googleLoginResponse.Success);
            Assert.IsNotNull(googleLoginResponse.Error);
            Assert.IsNull(googleLoginResponse.CookieCollection);
            Assert.AreEqual(GoogleAuthResponse.ErrorResponseCode.BadAuthentication, googleLoginResponse.Error.Value);
        }
    }
}