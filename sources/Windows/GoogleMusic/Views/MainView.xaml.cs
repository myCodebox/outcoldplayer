//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.System.Display;
    using Windows.UI.Xaml;

    public interface IMainView : IView
    {
        void ShowView(IView view);

        void HideView();

        void ShowPlayer();

        void HidePlayer();
    }

    public sealed partial class MainView : PageBase, IMainView
    {
        private DisplayRequest request;

        public MainView()
        {
            this.InitializePresenter<MainViewPresenter>();
            this.InitializeComponent();

            this.PlayerView.SetMediaElement(this.MediaElement);
        }

        public void ShowView(IView view)
        {
            this.Content.Children.Add((UIElement)view);
        }

        public void HideView()
        {
            this.Content.Children.Clear();
        }

        public void ShowPlayer()
        {
            this.PlayerView.Visibility = Visibility.Visible;
        }

        public void HidePlayer()
        {
            this.PlayerView.Visibility = Visibility.Collapsed;
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            var mainViewPresenter = this.Presenter<MainViewPresenter>();
            if (mainViewPresenter.CanGoBack())
            {
                mainViewPresenter.GoBack();
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (this.request == null)
            {
                this.request = new DisplayRequest();
                this.request.RequestActive();
            }
            else
            {
                this.request.RequestRelease();
                this.request = null;
            }
        }

        private void MoreClick(object sender, RoutedEventArgs e)
        {
            this.MorePopup.IsOpen = true;
        }
    }
}
