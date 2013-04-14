// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Lastfm
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using OutcoldSolutions.Web;

    public class LastfmAccountWebService : ILastfmAccountWebService
    {
        private readonly ILastfmWebService webService;

        public LastfmAccountWebService(ILastfmWebService webService)
        {
            this.webService = webService;
        }

        public async Task<TokenResp> GetTokenAsync()
        {
            this.webService.SetToken(null);

            HttpResponseMessage response = await this.webService.CallAsync("auth.gettoken");
            TokenResp tokenResp = await response.Content.ReadAsJsonObject<TokenResp>();

            if (!string.IsNullOrEmpty(tokenResp.Token))
            {
                this.webService.SetToken(tokenResp.Token);
            }

            return tokenResp;
        }

        public async Task<GetSessionResp> GetSessionAsync(string token)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
                                                        {
                                                            { "token", token }
                                                        };

            HttpResponseMessage response = await this.webService.CallAsync("auth.getSession", parameters);
            GetSessionResp getSessionResp = await response.Content.ReadAsJsonObject<GetSessionResp>();

            if (getSessionResp.Session != null)
            {
                this.webService.SetSession(getSessionResp.Session);
            }

            return getSessionResp;
        }

        public string GetAuthUrl(string token)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "http://www.last.fm/api/auth/?api_key={0}&token={1}",
                WebUtility.UrlEncode(LastfmWebService.ApiKey),
                WebUtility.UrlEncode(token));
        }
    }
}