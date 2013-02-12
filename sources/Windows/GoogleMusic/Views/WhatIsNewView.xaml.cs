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

    public interface IWhatIsNewView : IView
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
            App.Container.Resolve<ISearchService>().Register();
            App.Container.Resolve<ISettingsCommands>().Register();
            App.Container.Resolve<INavigationService>().NavigateTo<IStartView>();
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
