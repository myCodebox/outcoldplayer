// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Shell
{
    public interface ISecureStorageService
    {
        void Save(string resource, string username, string password);
        bool Get(string resource, out string username);
        bool Get(string resource, out string username, out string password);
        void Delete(string resource);
    }
}
