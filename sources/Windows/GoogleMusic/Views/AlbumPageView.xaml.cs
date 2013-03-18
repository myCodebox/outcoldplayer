// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public interface IAlbumPageView : IDataPageView
    {
    }

    public sealed partial class AlbumPageView : DataPageViewBase, IAlbumPageView
    {
        private AlbumPageViewPresenter presenter;

        public AlbumPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.ListView);
        }

        public override void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
            base.OnDataLoading(eventArgs);

            this.ListView.ItemsSource = null;
        }

        public override void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
            base.OnDataLoaded(eventArgs);

            if (this.presenter.BindingModel.Playlist != null)
            {
                this.ListView.ItemsSource = this.presenter.BindingModel.Songs;
                this.UpdateSelectedSong();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<AlbumPageViewPresenter>();
            this.presenter.BindingModel.Subscribe(
                () => this.presenter.BindingModel.SelectedSongIndex,
                (sender, args) => this.UpdateSelectedSong());
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (this.presenter.PlaySongCommand.CanExecute())
            {
                this.presenter.PlaySongCommand.Execute();
            }
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                this.presenter.BindingModel.SelectedSongIndex = -1;
            }
            else
            {
                this.presenter.BindingModel.SelectedSongIndex =
                    this.presenter.BindingModel.Songs.IndexOf((SongBindingModel)e.AddedItems.First());
            }
        }

        private void UpdateSelectedSong()
        {
            var selectedSongIndex = this.presenter.BindingModel.SelectedSongIndex;
            if (this.ListView.SelectedIndex != selectedSongIndex && this.ListView.Items != null && this.ListView.Items.Count > selectedSongIndex)
            {
                this.ListView.ScrollIntoView(this.presenter.BindingModel.SelectedSong);
                this.ListView.SelectedIndex = selectedSongIndex;
            }
        }
    }
}
