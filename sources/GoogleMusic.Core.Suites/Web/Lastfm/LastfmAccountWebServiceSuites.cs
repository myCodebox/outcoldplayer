// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Web.Lastfm
{
    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Web.Lastfm;

    public class LastfmAccountWebServiceSuites : SuitesBase
    {
        private LastfmAccountWebService service;
        private LastfmWebService webService;

        public override void SetUp()
        {
            base.SetUp();

            this.webService = new LastfmWebService(this.LogManager);
            this.service = new LastfmAccountWebService(this.webService);
        }

        [Test]
        public async void GetTokenAsync_Init_ReturnsToken()
        {
            TokenResp token = await this.service.GetTokenAsync();

            Assert.IsNotNull(token);
            Assert.IsNotNull(token.Token);
            Assert.IsNull(token.Error);
            Assert.IsNull(token.Message);
        }
    }
}