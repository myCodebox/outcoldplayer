// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Store;
    using Windows.Storage;

    [Flags]
    public enum GoogleMusicFeatures
    {
        None = 0,
        AdFree = 1,
        Offline = 2,
        All = 0xFFFFFFF
    }

    public static class InAppPurchases
    {
        public const string UltimateInAppPurchase = "Ultimate";
        public const string AdFreeUnlimitedInAppPurchase = "AdFreeUnlimited";

        private static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => ApplicationBase.Container.Resolve<ILogManager>().CreateLogger(typeof(InAppPurchases).Name)); 
        private static readonly List<string> Purchases = new List<string>();

        private static event LicenseChangedEventHandler LicenseChangedPrivate;

        public static event LicenseChangedEventHandler LicenseChanged
        {
            add
            {
                LicenseChangedPrivate += value;
#if DEBUG
                CurrentAppSimulator.LicenseInformation.LicenseChanged += value;
#else
                CurrentApp.LicenseInformation.LicenseChanged += value;
#endif
            }

            remove
            {
                LicenseChangedPrivate -= value;
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

        public static async Task<string> RequestPurchase(string inAppPurchaseName)
        {
            string receipt = null;
            try
            {
#if DEBUG
                receipt = await CurrentAppSimulator.RequestProductPurchaseAsync(inAppPurchaseName, true).AsTask();
#else
                receipt = await CurrentApp.RequestProductPurchaseAsync(inAppPurchaseName, true).AsTask();
#endif
                if (!string.IsNullOrWhiteSpace(receipt))
                {
                    Purchases.Add(inAppPurchaseName);
                    RaiseLicenseChanged();
                }
            }
            catch (Exception exception)
            {
                Logger.Value.Debug(exception, "Could not purchase");
            }

            return receipt;
        }

        private static void RaiseLicenseChanged()
        {
            var handler = LicenseChangedPrivate;
            if (handler != null)
            {
                handler();
            }
        }
    }
}