// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.WebServices
{
    using System.Threading;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class GoogleAutificationServiceSuites
    {
        API api = new API();

        [Test]
        public void Login_DefaultCredentials_Init()
        {
            GoogleMusicPlaylists playlists = null;

            api.Login("outcoldman", "password");

            api.OnLoginComplete += (sender, args) => this.api.GetPlaylist();

            api.OnGetPlaylistsComplete += pls =>
                { 
                    playlists = pls;
                };

            while (playlists == null)
            {
                Thread.Sleep(100);
            }

            Assert.IsNotEmpty(playlists.Playlists);
        }
    }
}