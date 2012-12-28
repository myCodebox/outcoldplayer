//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

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
        private readonly PlayerView playerView;
        private UIElement currentView;

        private bool? smallView = null;

        public MainView()
        {
            this.InitializeComponent();
            this.InitializePresenter<MainViewPresenter>();

            this.playerView = new PlayerView() { DataContext = this.Presenter<MainViewPresenter>().PlayerViewPresenter };
            Grid.SetColumn(this.playerView, 1);

            Debug.Assert(this.BottomAppBar != null, "this.BottomAppBar != null");
            this.BottomAppBar.Opened += (sender, o) => { this.BottomBorder.Visibility = Visibility.Visible; };
            this.BottomAppBar.Closed += (sender, o) => { this.BottomBorder.Visibility = Visibility.Collapsed; };

            this.Loaded += this.OnLoaded;
        }

        public void ShowView(IView view)
        {
            Debug.Assert(this.BottomAppBar != null, "this.BottomAppBar != null");
            this.BottomAppBar.IsEnabled = this.Presenter<MainViewPresenter>().BindingModel.IsAuthentificated;

            Debug.Assert(this.TopAppBar != null, "this.TopAppBar != null");
            this.TopAppBar.IsEnabled = this.Presenter<MainViewPresenter>().BindingModel.IsAuthentificated;

            this.ClearContext();
            this.Content.Children.Add((UIElement)view);
        }

        public void HideView()
        {
            this.ClearContext();
            this.Content.Children.Clear();
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
            var isSmallView = this.ActualWidth < 320.1;
            if (isSmallView)
            {
                if (!this.smallView.HasValue || !this.smallView.Value)
                {
                    
                    this.AppBarContent.Children.Remove(this.playerView);
                    this.currentView = this.Content.Children[0];
                    this.HideView();
                    this.Content.Children.Add(this.playerView);
                    this.Content.Margin = new Thickness(0);
                    this.BackButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (!this.smallView.HasValue || this.smallView.Value)
                {
                    if (this.smallView.HasValue)
                    {
                        this.HideView();
                        if (this.currentView != null)
                        {
                            this.Content.Children.Add(this.currentView);
                            this.currentView = null;
                        }
                    }

                    this.AppBarContent.Children.Add(this.playerView);
                    this.BackButton.Visibility = Visibility.Visible;
                }
            }

            this.smallView = isSmallView;
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            var mainViewPresenter = this.Presenter<MainViewPresenter>();
            if (mainViewPresenter.CanGoBack())
            {
                mainViewPresenter.GoBack();
            }
        }

        private void QueueNavigate(object sender, RoutedEventArgs e)
        {
            Debug.Assert(this.TopAppBar != null, "this.TopAppBar != null");
            this.TopAppBar.IsOpen = false;
            App.Container.Resolve<INavigationService>().NavigateTo<ICurrentPlaylistView>();
        }
    }
}
