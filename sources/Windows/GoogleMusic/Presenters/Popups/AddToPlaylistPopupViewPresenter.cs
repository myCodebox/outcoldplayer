// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class AddToPlaylistPopupViewPresenter : ViewPresenterBase<IAddToPlaylistPopupView>
    {
        private readonly Song song;
        private readonly IPlaylistCollectionsService collectionsService;
        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        private bool isLoading;

        private List<MusicPlaylist> playlists;

        public AddToPlaylistPopupViewPresenter(
            Song song,
            IDependencyResolverContainer container,
            IPlaylistCollectionsService collectionsService,
            IMusicPlaylistRepository musicPlaylistRepository)
            : base(container)
        {
            this.song = song;
            this.collectionsService = collectionsService;
            this.musicPlaylistRepository = musicPlaylistRepository;
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                this.isLoading = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public List<MusicPlaylist> Playlists
        {
            get
            {
                return this.playlists;
            }

            set
            {
                this.playlists = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public void AddToPlaylist(MusicPlaylist playlist)
        {
            this.Logger.LogTask(this.musicPlaylistRepository.AddEntryAsync(playlist.Id, this.song));
            this.View.Close();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.IsLoading = true;

            this.collectionsService
                .GetCollection<MusicPlaylist>()
                .GetAllAsync(Order.Name)
                .ContinueWith(async (t) =>
                {
                    if (t.IsCompleted && !t.IsFaulted && !t.IsCanceled)
                    {
                        await this.Dispatcher.RunAsync(() => this.Playlists = new List<MusicPlaylist>(t.Result));
                    }

                    this.Logger.LogTask(t);

                    await this.Dispatcher.RunAsync(() => this.IsLoading = false);
                });
        }
    }
}