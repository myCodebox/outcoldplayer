//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.Presenters;

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
        public MainView()
        {
            this.InitializePresenter<MainViewPresenter>();
            this.InitializeComponent();

            this.PlayerView.SetMediaElement(this.MediaElement);

            Debug.Assert(this.BottomAppBar != null, "this.BottomAppBar != null");

            this.BottomAppBar.Opened += (sender, o) =>
                {
                    this.BottomBorder.Visibility = Visibility.Visible;
                };

            this.BottomAppBar.Closed += (sender, o) =>
                {
                    this.BottomBorder.Visibility = Visibility.Collapsed;
                };
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
    }
}
