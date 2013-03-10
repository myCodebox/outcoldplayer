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

    public class PlaylistPageViewPresenter : PlaylistPageViewPresenterBase<IPlaylistPageView, Playlist>
    {
        private readonly IMusicPlaylistRepository musicPlaylistRepository;

        public PlaylistPageViewPresenter(
            IDependencyResolverContainer container, 
            IMusicPlaylistRepository musicPlaylistRepository)
            : base(container)
        {
            this.musicPlaylistRepository = musicPlaylistRepository;
            this.RemoveFromPlaylistCommand = new DelegateCommand(this.RemoveFromPlaylist);
        }

        public DelegateCommand RemoveFromPlaylistCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetContextCommands()
        {
            var commandMetadatas = base.GetContextCommands();
            if (this.BindingModel.Playlist is MusicPlaylist)
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
            if (selectedSongIndex >= 0 && !this.IsDataLoading && this.BindingModel.Playlist is MusicPlaylist)
            {
                this.IsDataLoading = true;
                var musicPlaylist = (MusicPlaylist)this.BindingModel.Playlist;

                this.musicPlaylistRepository.RemoveEntry(
                    musicPlaylist.Id, musicPlaylist.EntriesIds[selectedSongIndex]).ContinueWith(
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