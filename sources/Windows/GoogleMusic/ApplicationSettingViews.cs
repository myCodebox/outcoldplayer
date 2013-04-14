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
         public static void Initialize(IApplicationSettingViewsService service)
         {
             service.RegisterSettings<AccountsView>("accounts", "Accounts");
             service.RegisterSettings<AppSettingsView>("appsettings", "App Settings");

             bool upgradeViewRegistered = false;
             if (!InAppPurchases.HasFeature(GoogleMusicFeatures.All))
             {
                 service.RegisterSettings<UpgradeView>("upgrade", "Upgrade");
                 upgradeViewRegistered = true;
             }

             service.RegisterSettings<SupportView>("support", "Support");
             service.RegisterSettings<PrivacyView>("privacy", "Privacy Policy");
             service.RegisterSettings<LegalView>("legal", "Legal");

             InAppPurchases.LicenseChanged += () =>
             {
                 if (!InAppPurchases.HasFeature(GoogleMusicFeatures.All))
                 {
                     if (!upgradeViewRegistered)
                     {
                         service.RegisterSettings<UpgradeView>("upgrade", "Upgrade", ApplicationSettingLayoutType.Standard, "accounts");
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