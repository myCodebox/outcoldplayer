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
    }
}