// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System.Threading.Tasks;

    using Moq;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class GoogleAccountWebServiceSuites : SuitesBase
    {
        private IGoogleMusicWebService googleMusicWebService;
        private GoogleAccountWebService googleAccountWebService;

        [SetUp]
        public override void SetFixture()
        {
            base.SetFixture();

            var logManager = this.Container.Resolve<ILogManager>();
            this.googleMusicWebService = new GoogleMusicWebService(logManager, Mock.Of<IUserDataStorage>());
            this.googleAccountWebService = new GoogleAccountWebService(logManager, this.googleMusicWebService);
        }

        [Test]
        public async Task LoginAsync_RightCredentials_GetAuth()
        {
            var googleLoginResponse = await this.googleAccountWebService.LoginAsync("outcoldman.test@gmail.com", "Qw12er34");

            Assert.IsTrue(googleLoginResponse.Success);
            Assert.IsNotEmpty(googleLoginResponse.Auth);
            Assert.IsNotEmpty(googleLoginResponse.SID);
            Assert.IsNotEmpty(googleLoginResponse.LSID);
            Assert.IsNull(googleLoginResponse.Error);
            Assert.IsNull(googleLoginResponse.CaptchaToken);
            Assert.IsNull(googleLoginResponse.CaptchaUrl);
        }

        [Test]
        public async Task LoginAsync_WrongCredentials_GetAuth()
        {
            var googleLoginResponse = await this.googleAccountWebService.LoginAsync("outcoldman.test@gmail.com", "WrongPassword");

            Assert.IsFalse(googleLoginResponse.Success);
            Assert.IsNotNull(googleLoginResponse.Error);
            Assert.AreEqual(GoogleLoginResponse.ErrorResponseCode.BadAuthentication, googleLoginResponse.Error.Value); 
        }
    }
}