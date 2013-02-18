// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml.Controls;

    public interface IArtistPageView : IDataPageView
    {
    }

    public sealed partial class ArtistPagePageView : DataPageViewBase, IArtistPageView
    {
        private ArtistPageViewPresenter presenter;

        public ArtistPagePageView()
        {
            this.InitializeComponent();
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<ArtistPageViewPresenter>();
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
        }
    }
}
