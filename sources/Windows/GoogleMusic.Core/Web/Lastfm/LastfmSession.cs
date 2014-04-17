// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Lastfm
{
    public class LastfmSession
    {
        public LastfmSession(string name, string key)
        {
            this.Name = name;
            this.Key = key;
        }

        public string Name { get; private set; }

        public string Key { get; private set; }
    }
}