// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Views;

    internal class ApplicationSettingViewsService : IApplicationSettingViewsService
    {
        private readonly List<string> settingViewsOrder = new List<string>();

        public void Show()
        {
            
        }

        public void Close()
        {
            
        }

        public void Show(string name)
        {
            
        }

        public IEnumerable<string> GetRegisteredViews()
        {
            return this.settingViewsOrder.ToList();
        }

        public void RegisterSettings<TApplicationSettingsView>(
            string name, 
            string title, 
            ApplicationSettingLayoutType layoutType = ApplicationSettingLayoutType.Standard,
            string insertAfterName = null,
            bool visibleInSettings = true) 
            where TApplicationSettingsView : IApplicationSettingsContent
        {
            
        }

        public bool UnregisterSettings(string name)
        {
            return true;
        }

       
    }
}