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

    public interface ICurrentPlaylistPageView : IDataPageView
    {
        void SelectPlayingSong();
    }

    public sealed partial class CurrentPlaylistPageView : DataPageViewBase, ICurrentPlaylistPageView
    {
        private CurrentPlaylistPageViewPresenter presenter;

        public CurrentPlaylistPageView()
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

            this.ListView.ItemsSource = this.presenter.BindingModel.Songs;
            this.UpdateSelectedSong();
        }

        public void SelectPlayingSong()
        {
            this.presenter.SelectPlayingSong();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<CurrentPlaylistPageViewPresenter>();
            this.presenter.BindingModel.Subscribe(
                () => this.presenter.BindingModel.SelectedSongIndex,
                (sender, args) => this.UpdateSelectedSong());
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (this.presenter.PlaySelectedSongCommand.CanExecute())
            {
                this.presenter.PlaySelectedSongCommand.Execute();
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
            if (this.ListView.Items != null && this.ListView.Items.Count > selectedSongIndex)
            {
                this.ListView.ScrollIntoView(this.presenter.BindingModel.SelectedSong);
                this.ListView.SelectedIndex = selectedSongIndex;
            }
        }
    }
}
