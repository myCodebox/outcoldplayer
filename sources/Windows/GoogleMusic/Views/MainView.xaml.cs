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
    using Windows.UI.Xaml.Automation;
    using Windows.UI.Xaml.Controls;

    public interface IMediaElemenetContainerView : IView
    {
        MediaElement GetMediaElement();

        void Activate();
    }

    public interface IMainView : IView, IMediaElemenetContainerView, IViewRegionProvider
    {
    }

    public sealed partial class MainView : PageBase, IMainView, IMediaElemenetContainerView, ICurrentContextCommands, IApplicationToolbar
    {
        private MainViewPresenter presenter;

        private AdControl adControl;

        public MainView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<MainViewPresenter>();

            this.PlayerView.DataContext = this.presenter.PlayerViewPresenter;
            this.SnappedPlayerView.DataContext = this.presenter.PlayerViewPresenter;

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

                this.RemoveAdsButton.Visibility = Visibility.Collapsed;
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
                                             VerticalAlignment = VerticalAlignment.Top,
                                             Margin = new Thickness(0, 20, 10, 0),
                                             UseStaticAnchor = true
                                         };
                    Grid.SetColumn(this.adControl, 1);
                    Grid.SetRowSpan(this.adControl, 2);
                    this.MainGrid.Children.Add(this.adControl);
                }

                var visible = this.presenter.BindingModel.IsAuthenticated
                          && this.presenter.HasHistory()
                          && ApplicationView.Value != ApplicationViewState.Snapped;

                if (this.adControl != null)
                {
                    this.RemoveAdsButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                    this.adControl.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public void Show(IView view)
        {
            var visible = this.presenter.BindingModel.IsAuthenticated
                          && this.presenter.HasHistory()
                          && ApplicationView.Value != ApplicationViewState.Snapped;

            this.UpdateAppBars(visible);
            this.UpdateAdControl();

            this.ClearContext();
            this.MainContent.Content = view;
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

        public void SetCommands(IEnumerable<UIElement> buttons)
        {
            this.ClearContext();
            if (buttons != null)
            {
                foreach (var buttonBase in buttons)
                {
                    this.ViewCommandsPanel.Children.Add(buttonBase);
                }

                this.Activate();
            }
        }

        public void ClearContext()
        {
            this.ViewCommandsPanel.Children.Clear(); 
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
                this.MainContent.Visibility = Visibility.Collapsed;
                this.BackButton.Visibility = Visibility.Collapsed;
                this.SnappedPlayerView.Visibility = Visibility.Visible;
                this.LinksPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.MainContent.Visibility = Visibility.Visible;
                this.BackButton.Visibility = Visibility.Visible;
                this.SnappedPlayerView.Visibility = Visibility.Collapsed;
                this.LinksPanel.Visibility = Visibility.Visible;
            }

            this.UpdateAppBars(ApplicationView.Value != ApplicationViewState.Snapped);
            this.UpdateAdControl();
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
            this.presenter.GoBack();
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

        private void Navigate<TView>(object parameter = null) where TView : IPageView
        {
            Debug.Assert(this.TopAppBar != null, "this.TopAppBar != null");
            this.TopAppBar.IsOpen = false;
            App.Container.Resolve<INavigationService>().NavigateTo<TView>(parameter: parameter);
        }

        private void RemoveAdsClick(object sender, RoutedEventArgs e)
        {
            App.Container.Resolve<ISettingsCommands>().ActivateSettings("upgrade");
        }

        public void SetViewCommands(IEnumerable<CommandMetadata> commands)
        {
            this.ClearViewCommands();
            
            this.SetCommands(this.ViewCommandsPanel, commands);

            if (this.ViewCommandsPanel.Children.Count > 0)
            {
                this.Activate();
            }
        }

        public void ClearViewCommands()
        {
            this.ViewCommandsPanel.Children.Clear();
            this.ClearContextCommands(); 
        }

        public void SetContextCommands(IEnumerable<CommandMetadata> commands)
        {
            this.ClearContextCommands();

            this.SetCommands(this.ContextCommandsPanel, commands);

            if (this.ContextCommandsPanel.Children.Count > 0)
            {
                this.Activate();
            }
        }

        public void ClearContextCommands()
        {
            this.ContextCommandsPanel.Children.Clear(); 
        }

        private void SetCommands(Panel container, IEnumerable<CommandMetadata> commands)
        {
            foreach (var commandMetadata in commands)
            {
                var button = new Button()
                {
                    Style = (Style)Application.Current.Resources[commandMetadata.IconName],
                    Command = commandMetadata.Command
                };

                if (commandMetadata.Title != null)
                {
                    AutomationProperties.SetName(button, commandMetadata.Title);
                }

                container.Children.Add(button);
            }
        }
    }
}
