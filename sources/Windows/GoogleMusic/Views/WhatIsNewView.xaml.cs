// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.Views;

    using Windows.System;
    using Windows.UI.Xaml;

    public interface IWhatIsNewView : IPageView
    {
    }

    public sealed partial class WhatIsNewView : PageViewBase, IWhatIsNewView
    {
        public WhatIsNewView()
        {
            this.InitializeComponent();
        }

        private void ClickOk(object sender, RoutedEventArgs e)
        {
            ApplicationBase.Container.Resolve<ISearchService>().Register();
            ApplicationBase.Container.Resolve<INavigationService>().NavigateTo<IStartPageView>();
        }

        private void UserVoiceClick(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://gmusic.uservoice.com")).AsTask());
        }

        private void TwitterFollowClick(object sender, RoutedEventArgs e)
        {
            this.Logger.LogTask(Launcher.LaunchUriAsync(new Uri("https://twitter.com/gMusicW")).AsTask());
        }
    }
}
