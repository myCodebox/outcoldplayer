// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface IWhatIsNewView : IPageView
    {
    }

    public sealed partial class WhatIsNewView : UserControl, IWhatIsNewView
    {
        public WhatIsNewView()
        {
            this.InitializeComponent();
        }

        public void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
        }

        public void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
        }

        private void ClickOk(object sender, RoutedEventArgs e)
        {
            ApplicationBase.Container.Resolve<ISearchService>().Register();
            ApplicationBase.Container.Resolve<ISettingsCommands>().Register();
            ApplicationBase.Container.Resolve<INavigationService>().NavigateTo<IStartPageView>();
        }

        private void UserVoiceClick(object sender, RoutedEventArgs e)
        {
            var taskResult = Launcher.LaunchUriAsync(new Uri("https://gmusic.uservoice.com"));
        }

        private void TwitterFollowClick(object sender, RoutedEventArgs e)
        {
            var taskResult = Launcher.LaunchUriAsync(new Uri("https://twitter.com/gMusicW"));
        }
    }
}
