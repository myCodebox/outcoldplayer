//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Settings
{
    using Windows.UI.Xaml.Input;

    using OutcoldSolutions.Views;

    public sealed partial class OfflineCacheView : ViewBase, IApplicationSettingsContent
    {
        public OfflineCacheView()
        {
            this.InitializeComponent();
        }

        private void TextBlockMaxCacheSize_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((uint)e.Key >= (uint)Windows.System.VirtualKey.Number0
                && (uint)e.Key <= (uint)Windows.System.VirtualKey.Number9)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
