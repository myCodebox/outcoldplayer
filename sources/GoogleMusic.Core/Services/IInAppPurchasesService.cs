// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Threading.Tasks;

    public interface IInAppPurchasesService
    {
        bool IsActive(string inAppPurchaseName);

        Task RequestPurchase(string inAppPurchaseName);
    }
}
