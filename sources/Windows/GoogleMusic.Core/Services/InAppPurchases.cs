// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Store;
    using Windows.Storage;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public static class InAppPurchases
    {
        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => ApplicationBase.Container.Resolve<ILogManager>().CreateLogger(typeof(InAppPurchases).Name));
        private static readonly List<string> Purchases = new List<string>();

#if DEBUG
        public static async void SimulatorInAppPurchasesInitialization()
        {
            StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Resources");
            StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");

            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }
#endif

        public static bool IsActive(string inAppPurchaseName)
        {
            if (Purchases.Any((p) => string.Equals(inAppPurchaseName, p, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
#if DEBUG
            return CurrentAppSimulator.LicenseInformation.ProductLicenses.ContainsKey(inAppPurchaseName)
                    && CurrentAppSimulator.LicenseInformation.ProductLicenses[inAppPurchaseName].IsActive;
#else
            return CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(inAppPurchaseName)
                && CurrentApp.LicenseInformation.ProductLicenses[inAppPurchaseName].IsActive;
#endif
        }

        public static async Task<PurchaseResults> RequestPurchase(string inAppPurchaseName)
        {
            PurchaseResults receipt = null;
            try
            {
#if DEBUG
                receipt = await CurrentAppSimulator.RequestProductPurchaseAsync(inAppPurchaseName).AsTask();
#else
                receipt = await CurrentApp.RequestProductPurchaseAsync(inAppPurchaseName, true).AsTask();
#endif
                if (receipt.Status == ProductPurchaseStatus.Succeeded || receipt.Status == ProductPurchaseStatus.AlreadyPurchased)
                {
                    Purchases.Add(inAppPurchaseName);
                }
            }
            catch (Exception exception)
            {
                Logger.Value.Debug(exception, "Could not purchase");
            }

            return receipt;
        }
    }
}