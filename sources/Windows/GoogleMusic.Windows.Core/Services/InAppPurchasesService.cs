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

    public class InAppPurchasesService
    {
        private readonly ILogger logger; 
        private readonly List<string> purchases = new List<string>();

        public InAppPurchasesService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("InAppPurchasesService");
        }

#if DEBUG
        public static async void SimulatorInAppPurchasesInitialization()
        {
            StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Resources");
            StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");

            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }
#endif

        public bool IsActive(string inAppPurchaseName)
        {
            if (this.purchases.Any((p) => string.Equals(inAppPurchaseName, p, StringComparison.OrdinalIgnoreCase)))
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

        public async Task RequestPurchase(string inAppPurchaseName)
        {
            PurchaseResults receipt = null;
            try
            {
#if DEBUG
                receipt = await CurrentAppSimulator.RequestProductPurchaseAsync(inAppPurchaseName).AsTask();
#else
                receipt = await CurrentApp.RequestProductPurchaseAsync(inAppPurchaseName).AsTask();
#endif
                if (receipt.Status == ProductPurchaseStatus.Succeeded || receipt.Status == ProductPurchaseStatus.AlreadyPurchased)
                {
                    this.purchases.Add(inAppPurchaseName);
                    this.logger.Debug("Purchased, {0}.", receipt.ReceiptXml);
                }
            }
            catch (Exception exception)
            {
                this.logger.Debug(exception, "Could not purchase");
            }
        }
    }
}