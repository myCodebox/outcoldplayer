// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Threading.Tasks;

    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml;

    public interface ICurrentPlaylistPageView : IPageView
    {
        ISongsListView GetSongsListView();
    }

    public sealed partial class CurrentPlaylistPageView : PageViewBase, ICurrentPlaylistPageView
    {
        private CurrentPlaylistPageViewPresenter presenter;

        private ISongsListView songsListView;

        public CurrentPlaylistPageView()
        {
            this.InitializeComponent();
        }

        public ISongsListView GetSongsListView()
        {
            return this.songsListView;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<CurrentPlaylistPageViewPresenter>();

            this.ContentPresenter.Content = this.songsListView = this.Container.Resolve<ISongsListView>();

            var frameworkElement = this.songsListView as SongsListView;

            if (frameworkElement != null)
            {
                frameworkElement.SetBinding(
                    SongsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = this.presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(
                            PropertyNameExtractor.GetPropertyName(() => this.presenter.Songs))
                    });

                frameworkElement.SetBinding(
                    SongsListView.ViewPlaylistProperty,
                    new Binding()
                    {
                        Source = this.presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(
                            PropertyNameExtractor.GetPropertyName(() => this.presenter.ViewPlaylist))
                    });

                this.TrackScrollViewer(frameworkElement.GetListView());
            }
        }
    }
}
