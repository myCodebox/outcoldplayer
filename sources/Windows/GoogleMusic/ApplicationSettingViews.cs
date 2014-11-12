// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views.Settings;

    public static class ApplicationSettingViews
    {
         public static void Initialize(IApplicationSettingViewsService service, IApplicationResources resources)
         {
             service.RegisterSettings<AccountsView>("accounts", resources.GetString("SettingsAccountTitle"));
             service.RegisterSettings<AppSettingsView>("appsettings", resources.GetString("SettingsAppSettingsTitle"));
             service.RegisterSettings<OfflineCacheView>("offlinecache", resources.GetString("OfflineCacheAppSettingsTitle"), ApplicationSettingLayoutType.Large);
             service.RegisterSettings<SupportView>("support", resources.GetString("SettingsSupportTitle"));
             service.RegisterSettings<PrivacyView>("privacy", resources.GetString("SettingsPrivacyPolicyTitle"));
             service.RegisterSettings<LegalView>("legal", resources.GetString("SettingsLegalTitle"));

#if DEBUG
             InAppPurchasesService.SimulatorInAppPurchasesInitialization();
#endif
         }
    }
}