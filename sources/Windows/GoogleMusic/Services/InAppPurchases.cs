// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Store;
    using Windows.Storage;

    [Flags]
    public enum GoogleMusicFeatures
    {
        None = 0,
        AdFree = 1,
        All = 0xFFFFFFF
    }

    public static class InAppPurchases
    {
        public const string UltimateInAppPurchase = "Ultimate";
        public const string AdFreeUnlimitedInAppPurchase = "AdFreeUnlimited";

        public static event LicenseChangedEventHandler LicenseChanged
        {
            add
            {
#if DEBUG
                CurrentAppSimulator.LicenseInformation.LicenseChanged += value;
#else
                CurrentApp.LicenseInformation.LicenseChanged += value;
#endif
            }

            remove
            {
#if DEBUG
                CurrentAppSimulator.LicenseInformation.LicenseChanged -= value;
#else
                CurrentApp.LicenseInformation.LicenseChanged -= value;
#endif
            }
        }

#if DEBUG
        public static async void SimulatorInAppPurchasesInitialization()
        {
            StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Resources");
            StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");

            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }
#endif

        public static GoogleMusicFeatures GetFeatures()
        {
            GoogleMusicFeatures features = GoogleMusicFeatures.None;
            
            if (IsActive(UltimateInAppPurchase))
            {
                features |= GoogleMusicFeatures.All;
            }
            else
            {
                if (IsActive(AdFreeUnlimitedInAppPurchase))
                {
                    features |= GoogleMusicFeatures.AdFree;
                }
            }

            return features;
        }

        public static bool HasFeature(GoogleMusicFeatures features)
        {
            return (GetFeatures() & features) == features;
        }

        public static bool IsActive(string inAppPurchaseName)
        {
#if DEBUG
            return CurrentAppSimulator.LicenseInformation.ProductLicenses.ContainsKey(inAppPurchaseName)
                    && CurrentAppSimulator.LicenseInformation.ProductLicenses[inAppPurchaseName].IsActive;
#else
            return CurrentApp.LicenseInformation.ProductLicenses.ContainsKey(inAppPurchaseName)
                && CurrentApp.LicenseInformation.ProductLicenses[inAppPurchaseName].IsActive;
#endif
        }

        public static Task<string> RequestPurchase(string inAppPurchaseName)
        {
#if DEBUG
            return CurrentAppSimulator.RequestProductPurchaseAsync(inAppPurchaseName, false).AsTask();
#else
            return CurrentApp.RequestProductPurchaseAsync(inAppPurchaseName, false).AsTask();
#endif
        }
    }
}