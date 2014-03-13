// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistsListViewPresenter : ViewPresenterBase<IPlaylistsListView>
    {
        private readonly IPlayQueueService playQueueService;

        private readonly INavigationService navigationService;

        private readonly ISelectedObjectsService selectedObjectsService;

        private readonly ObservableCollection<PlaylistBindingModel> selectedItems;

        private ObservableCollection<PlaylistBindingModel> playlists;

        private int maxItems = int.MaxValue;

        private IList<IPlaylist> collection;

        private bool isMixedList = false;

        public PlaylistsListViewPresenter(
            IPlayQueueService playQueueService,
            INavigationService navigationService,
            ISelectedObjectsService selectedObjectsService)
        {
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.selectedObjectsService = selectedObjectsService;
            this.PlayCommand = new DelegateCommand(this.Play);

            this.selectedItems = new ObservableCollection<PlaylistBindingModel>();


            this.selectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
        }

        public DelegateCommand PlayCommand { get; private set; }

        public ObservableCollection<PlaylistBindingModel> Playlists
        {
            get
            {
                return this.playlists;
            }

            private set
            {
                this.SetValue(ref this.playlists, value);
            }
        }


        public ObservableCollection<PlaylistBindingModel> SelectedItems
        {
            get
            {
                return this.selectedItems;
            }
        }

        public int MaxItems
        {
            get
            {
                return this.maxItems;
            }

            set
            {
                this.maxItems = value;
            }
        }

        public bool IsMixedList
        {
            get
            {
                return this.isMixedList;
            }

            set
            {
                this.isMixedList = value;
            }
        }

        public void SetCollection(IEnumerable<IPlaylist> enumerable)
        {
            this.collection = enumerable == null ? null : enumerable.ToList();
            if (this.collection == null)
            {
                this.Playlists = null;
            }
            else
            {
                this.Playlists = new ObservableCollection<PlaylistBindingModel>(this.collection.Take(this.MaxItems).Select(x => new PlaylistBindingModel(x) { PlayCommand = this.PlayCommand, IsMixedList = this.IsMixedList }));
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.EventAggregator.GetEvent<SelectionClearedEvent>()
                .Subscribe<SelectionClearedEvent>(async (e) => await this.Dispatcher.RunAsync(this.ClearSelectedItems));
        }

        public void ClearSelectedItems()
        {
            if (this.selectedItems.Count > 0)
            {
                this.selectedItems.Clear();
            }
        }

        private void Play(object commandParameter)
        {
            IPlaylist playlist = commandParameter as IPlaylist;
            if (playlist != null)
            {
                this.Logger.LogTask(this.playQueueService.PlayAsync(playlist));
                this.navigationService.NavigateToPlaylist(playlist);
            }
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.selectedObjectsService.Update(
                e.NewItems == null ? null : e.NewItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist),
                e.OldItems == null ? null : e.OldItems.Cast<PlaylistBindingModel>().Select(x => x.Playlist));
        }
    }
}
