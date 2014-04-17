// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Presenters;

    public sealed partial class PlaylistPageView : PageViewBase, IPlaylistPageView
    {
        private PlaylistPageViewPresenter presenter;

        private ISongsListView songsListView;

        public PlaylistPageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<PlaylistPageViewPresenter>();

            this.ContentPresenter.Content = this.songsListView = this.Container.Resolve<ISongsListView>();

            var frameworkElement = this.songsListView as SongsListView;

            if (frameworkElement != null)
            {
                frameworkElement.SetBinding(
                    SongsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = this.presenter.BindingModel,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(
                            PropertyNameExtractor.GetPropertyName(() => this.presenter.BindingModel.Songs))
                    });

                frameworkElement.SetBinding(
                    SongsListView.ViewPlaylistProperty,
                    new Binding()
                    {
                        Source = this.presenter.BindingModel,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(
                            PropertyNameExtractor.GetPropertyName(() => this.presenter.BindingModel.Playlist))
                    });

                this.TrackScrollViewer(frameworkElement.GetListView());
            }
        }

        public ISongsListView GetSongsListView()
        {
            return this.songsListView;
        }
    }
}
