//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.Advertising.WinRT.UI;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Store;
    using Windows.Storage;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface IMediaElemenetContainerView : IView
    {
        MediaElement GetMediaElement();

        void Activate();

        void HideAd();

        void ShowAd();
    }

    public interface IMainView : IView, IMediaElemenetContainerView
    {
        void ShowView(IView view);

        void HideView();
    }

    public sealed partial class MainView : PageBase, IMainView, IMediaElemenetContainerView, ICurrentContextCommands
    {
        private AdControl adControl;

        public MainView()
        {
            this.InitializeComponent();
            this.InitializePresenter<MainViewPresenter>();

            this.PlayerView.DataContext = this.Presenter<MainViewPresenter>().PlayerViewPresenter;
            this.SnappedPlayerView.DataContext = this.Presenter<MainViewPresenter>().PlayerViewPresenter;

            Debug.Assert(this.BottomAppBar != null, "this.BottomAppBar != null");
            this.BottomAppBar.Opened += (sender, o) =>
                {
                    if (this.BottomAppBar.Visibility == Visibility.Collapsed)
                    {
                        this.BottomAppBar.IsOpen = false;
                    }
                    else
                    {
                        this.BottomBorder.Visibility = Visibility.Visible;
                    }
                };

            this.BottomAppBar.Closed += (sender, o) => { this.BottomBorder.Visibility = Visibility.Collapsed; };

            this.Loaded += this.OnLoaded;

#if DEBUG
            this.LoadFakeAdds();
            CurrentAppSimulator.LicenseInformation.LicenseChanged += this.UpdateAdControl;
#else
            CurrentApp.LicenseInformation.LicenseChanged += this.UpdateAdControl;
#endif

            this.UpdateAdControl();
        }

#if DEBUG
        private async void LoadFakeAdds()
        {
            StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Resources");
            StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");

            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }
#endif

        private bool IsAdFree()
        {
#if DEBUG
            return (CurrentAppSimulator.LicenseInformation.ProductLicenses.ContainsKey("AdFreeUnlimited")
                && CurrentAppSimulator.LicenseInformation.ProductLicenses["AdFreeUnlimited"].IsActive)
                || (CurrentAppSimulator.LicenseInformation.ProductLicenses.ContainsKey("Ultimate")
                && CurrentAppSimulator.LicenseInformation.ProductLicenses["Ultimate"].IsActive);
#else
            return (CurrentApp.LicenseInformation.ProductLicenses.ContainsKey("AdFreeUnlimited")
                && CurrentApp.LicenseInformation.ProductLicenses["AdFreeUnlimited"].IsActive)
                || (CurrentApp.LicenseInformation.ProductLicenses.ContainsKey("Ultimate")
                && CurrentApp.LicenseInformation.ProductLicenses["Ultimate"].IsActive);
#endif
        }

        private void UpdateAdControl()
        {
            if (this.IsAdFree())
            {
                if (this.adControl != null)
                {
                    this.MainGrid.Children.Remove(this.adControl);
                    this.adControl = null;
                }
            }
            else
            {
                if (this.adControl == null)
                {
                    this.adControl = new AdControl
                                         {
                                             ApplicationId = "8eb9e14b-2133-40db-9500-14eff7c05aab",
                                             AdUnitId = "111663",
                                             Width = 160,
                                             Height = 600,
                                             VerticalAlignment = VerticalAlignment.Center,
                                             Margin = new Thickness(0, 0, 10, 0)
                                         };
                    Grid.SetColumn(this.adControl, 1);
                    this.MainGrid.Children.Add(this.adControl);
                }
            }
        }

        public void ShowView(IView view)
        {
            var visible = this.Presenter<MainViewPresenter>().BindingModel.IsAuthenticated
                          && this.Presenter<MainViewPresenter>().HasHistory()
                          && ApplicationView.Value != ApplicationViewState.Snapped;

            this.UpdateAppBars(visible);
            this.UpdateAdControl();

            if (visible)
            {
                this.ShowAd();
            }
            else
            {
                this.HideAd();
            }

            this.ClearContext();
            this.Content.Content = view;
        }

        public void HideView()
        {
            this.ClearContext();
            this.Content.Content = null;
        }

        public MediaElement GetMediaElement()
        {
            return this.MediaElement;
        }

        public void Activate()
        {
            Debug.Assert(this.BottomAppBar != null, "this.BottomAppBar != null");
            this.BottomAppBar.IsOpen = true;
        }

        public void HideAd()
        {
            if (this.adControl != null)
            {
                this.adControl.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowAd()
        {
            if (this.adControl != null)
            {
                this.adControl.Visibility = Visibility.Visible;
            }
        }

        public void SetCommands(IEnumerable<UIElement> buttons)
        {
            this.ClearContext();
            if (buttons != null)
            {
                foreach (var buttonBase in buttons)
                {
                    this.ContextCommands.Children.Add(buttonBase);
                }

                this.Activate();
            }
        }

        public void ClearContext()
        {
            this.ContextCommands.Children.Clear(); 
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= this.OnLoaded;
            this.UpdateCurrentView();
            this.SizeChanged += (s, args) => this.UpdateCurrentView();
        }

        private void UpdateCurrentView()
        {
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                if (this.adControl != null)
                {
                    this.adControl.Visibility = Visibility.Collapsed;
                }

                this.Content.Visibility = Visibility.Collapsed;
                this.BackButton.Visibility = Visibility.Collapsed;
                this.SnappedPlayerView.Visibility = Visibility.Visible;
            }
            else
            {
                if (this.adControl != null)
                {
                    this.adControl.Visibility = Visibility.Visible;
                }

                this.Content.Visibility = Visibility.Visible;
                this.BackButton.Visibility = Visibility.Visible;
                this.SnappedPlayerView.Visibility = Visibility.Collapsed;
            }

            this.UpdateAppBars(ApplicationView.Value != ApplicationViewState.Snapped);
        }

        private void UpdateAppBars(bool visible)
        {
            if (visible)
            {
                this.BottomAppBar.IsEnabled = true;
                this.TopAppBar.IsEnabled = true;
                this.BottomAppBar.Visibility = Visibility.Visible;
                this.TopAppBar.Visibility = Visibility.Visible;
            }
            else
            {
                this.BottomAppBar.IsEnabled = false;
                this.TopAppBar.IsEnabled = false;
                this.BottomAppBar.IsOpen = false;
                this.TopAppBar.IsOpen = false;
                this.BottomAppBar.Visibility = Visibility.Collapsed;
                this.TopAppBar.Visibility = Visibility.Collapsed;
            }
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            var mainViewPresenter = this.Presenter<MainViewPresenter>();
            if (mainViewPresenter.CanGoBack())
            {
                mainViewPresenter.GoBack();
            }
        }
        
        private void HomeNavigate(object sender, RoutedEventArgs e)
        {
            this.Navigate<IStartView>();
        }

        private void QueueNavigate(object sender, RoutedEventArgs e)
        {
            this.Navigate<ICurrentPlaylistView>();
        }

        private void PlaylistsNavigate(object sender, RoutedEventArgs e)
        {
            this.Navigate<IPlaylistsView>(PlaylistsRequest.Playlists);
        }

        private void AlbumsNavigate(object sender, RoutedEventArgs e)
        {
            this.Navigate<IPlaylistsView>(PlaylistsRequest.Albums);
        }

        private void GenresNavigate(object sender, RoutedEventArgs e)
        {
            this.Navigate<IPlaylistsView>(PlaylistsRequest.Genres);
        }

        private void ArtistsNavigate(object sender, RoutedEventArgs e)
        {
            this.Navigate<IPlaylistsView>(PlaylistsRequest.Artists);
        }

        private void Navigate<TView>(object parameter = null) where TView : IView
        {
            Debug.Assert(this.TopAppBar != null, "this.TopAppBar != null");
            this.TopAppBar.IsOpen = false;
            App.Container.Resolve<INavigationService>().NavigateTo<TView>(parameter: parameter);
        }
    }
}
