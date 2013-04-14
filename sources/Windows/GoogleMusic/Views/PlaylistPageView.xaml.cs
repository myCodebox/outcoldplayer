// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Input;

    public interface IPlaylistPageView : IPageView
    {
    }

    public sealed partial class PlaylistPageView : PageViewBase, IPlaylistPageView
    {
        private PlaylistPageViewPresenter presenter;

        public PlaylistPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<PlaylistPageViewPresenter>();
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var songBindingModel = frameworkElement.DataContext as SongBindingModel;
                if (songBindingModel != null)
                {
                    this.presenter.PlaySong(songBindingModel);
                }
            }
        }
    }
}
