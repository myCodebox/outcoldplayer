// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.UI.Popups;

    public class RadioPageViewPresenter : PlaylistsPageViewPresenterBase<IRadioPageView, PlaylistsPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;
        private readonly IRadioStationsService radioStationsService;

        public RadioPageViewPresenter(
            IApplicationResources resources, 
            IPlaylistsService playlistsService, 
            INavigationService navigationService, 
            IPlayQueueService playQueueService, 
            IRadioStationsService radioStationsService,
            ISelectedObjectsService selectedObjectsService)
            : base(resources, playlistsService, navigationService, playQueueService, selectedObjectsService)
        {
            this.resources = resources;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.radioStationsService = radioStationsService;
            this.DeleteRadioCommand = new DelegateCommand(this.DeleteRadio, () => this.BindingModel.SelectedItems.Count > 0 && this.BindingModel.SelectedItems.All(x => !string.IsNullOrEmpty(x.Playlist.Id)));
        }

        public DelegateCommand DeleteRadioCommand { get; private set; }

        public void PlayRadio(IPlaylist playlist)
        {
            if (this.PlayCommand.CanExecute(playlist))
            {
                var currentPlaylist = this.playQueueService.CurrentPlaylist;

                if (currentPlaylist != null 
                    && currentPlaylist.PlaylistType == PlaylistType.Radio
                    && string.Equals(currentPlaylist.Id, playlist.Id, StringComparison.Ordinal))
                {
                    this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
                }
                else
                {
                    this.PlayCommand.Execute(playlist);
                }
            }
        }

        protected async override Task LoadPlaylistsAsync()
        {
            await base.LoadPlaylistsAsync();
            await this.Dispatcher.RunAsync(() => this.BindingModel.ClearSelectedItems());
        }

        private async void DeleteRadio()
        {
            if (this.DeleteRadioCommand.CanExecute())
            {
                try
                {
                    var yesUiCommand = new UICommand(this.resources.GetString("MessageBox_DeletePlaylistYes"));
                    var noUiCommand = new UICommand(this.resources.GetString("MessageBox_DeletePlaylistNo"));

                    var playlists = this.BindingModel.SelectedItems.Select(bm => bm.Playlist).ToList();

                    MessageDialog dialog = new MessageDialog(this.resources.GetString("MessageBox_DeleteRadioMessage"));
                    dialog.Commands.Add(yesUiCommand);
                    dialog.Commands.Add(noUiCommand);
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;
                    var command = await dialog.ShowAsync();

                    if (command == yesUiCommand)
                    {
                        this.IsDataLoading = true;

                        await this.radioStationsService.DeleteAsync(playlists.Cast<Radio>().ToList());

                        this.IsDataLoading = false;

                        this.EventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.Radio));
                    }
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, "DeleteRadio failed");
                }
            }
        }
    }
}
