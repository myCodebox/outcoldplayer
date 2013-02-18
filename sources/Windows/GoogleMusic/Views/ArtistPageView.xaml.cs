// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public interface IArtistPageView : IDataPageView
    {
    }

    public sealed partial class ArtistPageView : DataPageViewBase, IArtistPageView
    {
        private const string HorizontalScrollOffset = "ListView_HorizontalScrollOffset";
        private ArtistPageViewPresenter presenter;

        public ArtistPageView()
        {
            this.InitializeComponent();
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            eventArgs.State[HorizontalScrollOffset] = this.GridView.GetScrollViewerHorizontalOffset();
        }

        public override void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
            this.GridView.ScrollToHorizontalZero();
        }

        public override void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
            if (eventArgs.IsNavigationBack)
            {
                object offset;
                if (eventArgs.State.TryGetValue(HorizontalScrollOffset, out offset))
                {
                    this.GridView.ScrollToHorizontalOffset((double)offset);
                }
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<ArtistPageViewPresenter>();
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as PlaylistBindingModel;

            Debug.Assert(album != null, "album != null");
            if (album != null)
            {
                this.NavigationService.NavigateToView<PlaylistViewResolver>(album.Playlist);
            }
        }
    }
}
