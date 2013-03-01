// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Telerik.UI.Xaml.Controls.Grid;

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
            //this.TrackListViewBase(this.ListView);
        }

        public override void OnDataLoading(NavigatedToEventArgs eventArgs)
        {
            base.OnDataLoading(eventArgs);

            this.DataGrid.ItemsSource = null;
        }

        public override void OnDataLoaded(NavigatedToEventArgs eventArgs)
        {
            base.OnDataLoaded(eventArgs);

            if (this.presenter.BindingModel.Playlist != null)
            {
                this.DataGrid.ItemsSource = this.presenter.BindingModel.Playlist.Songs;
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

        private void UpdateSelectedSong()
        {
            var selectedSong = this.presenter.BindingModel.SelectedSong;

            if (this.DataGrid.SelectedItem != selectedSong && this.DataGrid.ItemsSource != null)
            {
                this.DataGrid.ScrollItemIntoView(selectedSong);
                this.DataGrid.SelectedItem = selectedSong;
            }
        }

        private void DataGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            var addedItems = e.AddedItems.ToList();
            if (addedItems.Count == 0)
            {
                this.presenter.BindingModel.SelectedSongIndex = -1;
            }
            else
            {
                this.presenter.BindingModel.SelectedSongIndex =
                    this.presenter.BindingModel.Playlist.Songs.IndexOf((Song)addedItems.First());
            }
        }

        private void DataGrid_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (this.presenter.PlaySongCommand.CanExecute())
            {
                this.presenter.PlaySongCommand.Execute();
            }
        }
    }
}
