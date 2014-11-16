// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Threading.Tasks;

    public interface IDataProtectService
    {
        Task<string> ProtectStringAsync(string unprotectedString);

        Task<string> UnprotectStringAsync(string protectedString);

        byte[] GetMd5Hash(string content);

        string GetMd5HashStringAsBase64(string content);

        string GetHMacStringAsBase64(string key, string value);
    }
}