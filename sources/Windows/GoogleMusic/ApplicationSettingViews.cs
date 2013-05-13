// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Settings;
    using OutcoldSolutions.Shell;

    public static class ApplicationSettingViews
    {
         public static void Initialize(IApplicationSettingViewsService service, IApplicationResources resources)
         {
             service.RegisterSettings<AccountsView>("accounts", resources.GetString("SettingsAccountTitle"));
             service.RegisterSettings<AppSettingsView>("appsettings", resources.GetString("SettingsAppSettingsTitle"));
             service.RegisterSettings<OfflineCacheView>("offlinecache", resources.GetString("OfflineCacheAppSettingsTitle"), ApplicationSettingLayoutType.Large);

             bool upgradeViewRegistered = false;
             if (!InAppPurchases.HasFeature(GoogleMusicFeatures.All))
             {
                 service.RegisterSettings<UpgradeView>("upgrade", resources.GetString("SettingsUpgradeTitle"));
                 upgradeViewRegistered = true;
             }

             service.RegisterSettings<SupportView>("support", resources.GetString("SettingsSupportTitle"));
             service.RegisterSettings<PrivacyView>("privacy", resources.GetString("SettingsPrivacyPolicyTitle"));
             service.RegisterSettings<LegalView>("legal", resources.GetString("SettingsLegalTitle"));

             InAppPurchases.LicenseChanged += () =>
             {
                 if (!InAppPurchases.HasFeature(GoogleMusicFeatures.All))
                 {
                     if (!upgradeViewRegistered)
                     {
                         service.RegisterSettings<UpgradeView>("upgrade", "SettingsUpgradeTitle", ApplicationSettingLayoutType.Standard, "accounts");
                         upgradeViewRegistered = true;
                     }
                 }
                 else
                 {
                     if (upgradeViewRegistered)
                     {
                         service.UnregisterSettings("upgrade");
                         upgradeViewRegistered = false;
                     }
                 }
             };

#if DEBUG
             InAppPurchases.SimulatorInAppPurchasesInitialization();
#endif
         }
    }
}