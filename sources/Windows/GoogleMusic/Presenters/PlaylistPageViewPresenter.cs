// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistPageViewPresenter : PlaylistPageViewPresenterBase<IPlaylistPageView, IPlaylist>
    {
        private readonly IUserPlaylistsRepository userPlaylistsRepository;

        public PlaylistPageViewPresenter(
            IDependencyResolverContainer container, 
            IUserPlaylistsRepository userPlaylistsRepository)
            : base(container)
        {
            this.userPlaylistsRepository = userPlaylistsRepository;
            this.RemoveFromPlaylistCommand = new DelegateCommand(this.RemoveFromPlaylist);
        }

        public DelegateCommand RemoveFromPlaylistCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetContextCommands()
        {
            var commandMetadatas = base.GetContextCommands();
            if (this.BindingModel.Playlist is UserPlaylist)
            {
                commandMetadatas = new List<CommandMetadata>(commandMetadatas)
                                       {
                                           new CommandMetadata(CommandIcon.Remove, "Playlist", this.RemoveFromPlaylistCommand)
                                       };
            }

            return commandMetadatas;
        }

        private void RemoveFromPlaylist()
        {
            var selectedSongIndex = this.BindingModel.SelectedSongIndex;
            if (selectedSongIndex >= 0 && !this.IsDataLoading && this.BindingModel.Playlist is UserPlaylist)
            {
                this.IsDataLoading = true;
                var musicPlaylist = (UserPlaylist)this.BindingModel.Playlist;

                this.userPlaylistsRepository.RemoveEntry(
                    musicPlaylist, this.BindingModel.Songs[selectedSongIndex].Metadata.ProviderSongId, this.BindingModel.Songs[selectedSongIndex].Metadata.UserPlaylistEntry.ProviderEntryId).ContinueWith(
                        t =>
                            {
                                this.IsDataLoading = false;
                                if (this.BindingModel.Songs.Count > 0)
                                {
                                    if (this.BindingModel.Songs.Count <= selectedSongIndex)
                                    {
                                        selectedSongIndex--;
                                    }

                                    this.BindingModel.SelectedSongIndex = selectedSongIndex;
                                }
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
}