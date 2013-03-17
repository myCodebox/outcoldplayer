// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistPageViewPresenter : PlaylistPageViewPresenterBase<IPlaylistPageView, PlaylistBaseBindingModel>
    {
        private readonly IUserPlaylistRepository userPlaylistRepository;

        public PlaylistPageViewPresenter(
            IDependencyResolverContainer container, 
            IUserPlaylistRepository userPlaylistRepository)
            : base(container)
        {
            this.userPlaylistRepository = userPlaylistRepository;
            this.RemoveFromPlaylistCommand = new DelegateCommand(this.RemoveFromPlaylist);
        }

        public DelegateCommand RemoveFromPlaylistCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetContextCommands()
        {
            var commandMetadatas = base.GetContextCommands();
            if (this.BindingModel.Playlist is UserPlaylistBindingModel)
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
            if (selectedSongIndex >= 0 && !this.IsDataLoading && this.BindingModel.Playlist is UserPlaylistBindingModel)
            {
                this.IsDataLoading = true;
                var musicPlaylist = (UserPlaylistBindingModel)this.BindingModel.Playlist;

                this.userPlaylistRepository.RemoveEntry(
                    musicPlaylist.Metadata, musicPlaylist.Songs[selectedSongIndex].Metadata.ProviderSongId, musicPlaylist.EntriesIds[selectedSongIndex]).ContinueWith(
                        t =>
                            {
                                this.IsDataLoading = false;
                                if (this.BindingModel.Playlist.Songs.Count > 0)
                                {
                                    if (this.BindingModel.Playlist.Songs.Count <= selectedSongIndex)
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