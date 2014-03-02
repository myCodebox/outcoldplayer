// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistPageViewPresenter : PlaylistPageViewPresenterBase<IPlaylistPageView, IPlaylist>
    {
        private readonly IApplicationResources resources;
        private readonly IUserPlaylistsService userPlaylistsService;
        private readonly IPlaylistsService playlistsService;

        public PlaylistPageViewPresenter(
            IDependencyResolverContainer container,
            IApplicationResources resources,
            IUserPlaylistsService userPlaylistsService,
            IPlaylistsService playlistsService)
            : base(container)
        {
            this.resources = resources;
            this.userPlaylistsService = userPlaylistsService;
            this.playlistsService = playlistsService;
            this.RemoveFromPlaylistCommand = new DelegateCommand(this.RemoveFromPlaylist);
        }

        public DelegateCommand RemoveFromPlaylistCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetContextCommands()
        {
            var commandMetadatas = base.GetContextCommands();
            if (this.BindingModel.Playlist is UserPlaylist && !((UserPlaylist)this.BindingModel.Playlist).IsShared)
            {
                commandMetadatas = new List<CommandMetadata>(commandMetadatas)
                                       {
                                           new CommandMetadata(CommandIcon.Remove, this.resources.GetString("Toolbar_PlaylistButton"), this.RemoveFromPlaylistCommand)
                                       };
            }

            return commandMetadatas;
        }

        private async void RemoveFromPlaylist()
        {
            var selectedSongs = this.BindingModel.SongsBindingModel.GetSelectedSongs().ToList();
            if (selectedSongs.Count > 0 && !this.IsDataLoading && this.BindingModel.Playlist is UserPlaylist)
            {
                await this.Dispatcher.RunAsync(() => { this.IsDataLoading = true; });
                var userPlaylist = (UserPlaylist)this.BindingModel.Playlist;

                IEnumerable<Song> songs = null;

                try
                {
                    await this.userPlaylistsService.RemoveSongsAsync(userPlaylist, selectedSongs);
                    songs = await this.playlistsService.GetSongsAsync(PlaylistType.UserPlaylist, userPlaylist.Id);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "Exception while tried to remove songs.");
                }

                await this.Dispatcher.RunAsync(
                        () =>
                        {
                            this.BindingModel.SongsBindingModel.SetCollection(songs);
                            this.IsDataLoading = false;
                        });
            }
        }
    }
}