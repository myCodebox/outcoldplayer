// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class RadioStationPageViewPresenter : PlaylistPageViewPresenterBase<IPlaylistPageViewBase>
    {
        private readonly IPlaylistsService playlistsService;
        private readonly IPlayQueueService playQueueService;

        public RadioStationPageViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistsService playlistsService,
            IPlayQueueService playQueueService)
            : base(container)
        {
            this.playlistsService = playlistsService;
            this.playQueueService = playQueueService;
            this.RefreshRadioCommand = new DelegateCommand(RefreshRadio);
        }

        public DelegateCommand RefreshRadioCommand { get; private set; }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandIcon.Radio, "Refresh radio", this.RefreshRadioCommand);
        }

        private async void RefreshRadio()
        {
            if (!this.IsDataLoading)
            {
                await this.Dispatcher.RunAsync(() =>
                {
                    this.IsDataLoading = true;
                });

                IPlaylist playlist = this.BindingModel.Playlist;
                IList<Song> songs =
                    await this.playlistsService.GetSongsAsync(playlist.PlaylistType, playlist.Id, playlist);

                this.Logger.LogTask(this.playQueueService.PlayAsync(playlist, songs, 0));

                await this.Dispatcher.RunAsync(() =>
                {
                    this.BindingModel.Songs = songs;
                    this.IsDataLoading = false;
                });
            }
        }
    }
}
