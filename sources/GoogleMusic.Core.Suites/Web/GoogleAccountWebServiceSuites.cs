// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class GoogleAccountWebServiceSuites : SuitesBase
    {
        private GoogleAccountWebService googleAccountWebService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var logManager = this.Container.Resolve<ILogManager>();
            this.googleAccountWebService = new GoogleAccountWebService(logManager);
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
    }
}