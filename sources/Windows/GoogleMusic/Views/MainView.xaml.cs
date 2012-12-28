//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public interface IMediaElemenetContainerView : IView
    {
        MediaElement GetMediaElement();

        void Activate();
    }

    public interface IMainView : IView, IMediaElemenetContainerView
    {
        void ShowView(IView view);

        void HideView();
    }

    public sealed partial class MainView : PageBase, IMainView, IMediaElemenetContainerView, ICurrentContextCommands
    {
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
        }

        public void ShowView(IView view)
        {
            Debug.Assert(this.BottomAppBar != null, "this.BottomAppBar != null");
            this.BottomAppBar.IsEnabled = this.Presenter<MainViewPresenter>().BindingModel.IsAuthenticated
                && ApplicationView.Value != ApplicationViewState.Snapped;

            Debug.Assert(this.TopAppBar != null, "this.TopAppBar != null");
            this.TopAppBar.IsEnabled = this.Presenter<MainViewPresenter>().BindingModel.IsAuthenticated
                && ApplicationView.Value != ApplicationViewState.Snapped;

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

        public void SetCommands(IEnumerable<ButtonBase> buttons)
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
                this.Content.Visibility = Visibility.Collapsed;
                this.BackButton.Visibility = Visibility.Collapsed;
                this.SnappedPlayerView.Visibility = Visibility.Visible;
                this.BottomAppBar.IsEnabled = false;
                this.TopAppBar.IsEnabled = false;
                this.BottomAppBar.IsOpen = false;
                this.TopAppBar.IsOpen = false;
                this.BottomAppBar.Visibility = Visibility.Collapsed;
                this.TopAppBar.Visibility = Visibility.Collapsed; 
            }
            else
            {
                this.Content.Visibility = Visibility.Visible;
                this.BackButton.Visibility = Visibility.Visible;
                this.SnappedPlayerView.Visibility = Visibility.Collapsed;
                this.BottomAppBar.IsEnabled = true;
                this.TopAppBar.IsEnabled = true;
                this.BottomAppBar.Visibility = Visibility.Visible;
                this.TopAppBar.Visibility = Visibility.Visible; 
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

        private void Navigate<TView>() where TView : IView
        {
            Debug.Assert(this.TopAppBar != null, "this.TopAppBar != null");
            this.TopAppBar.IsOpen = false;
            App.Container.Resolve<INavigationService>().NavigateTo<TView>();
        }
    }
}
