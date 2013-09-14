// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.Views;

    using Windows.UI.Popups;

    public class RadioPageViewPresenter : PlaylistsPageViewPresenterBase<IRadioPageView, PlaylistsPageViewBindingModel>
    {
        private readonly IApplicationResources resources;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;
        private readonly IRadioWebService radioWebService;

        public RadioPageViewPresenter(
            IApplicationResources resources, 
            IPlaylistsService playlistsService, 
            INavigationService navigationService, 
            IPlayQueueService playQueueService, 
            ISongsCachingService cachingService, 
            IApplicationStateService stateService,
            IRadioWebService radioWebService)
            : base(resources, playlistsService, navigationService, playQueueService, cachingService, stateService)
        {
            this.resources = resources;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.radioWebService = radioWebService;
            this.EditRadioNameCommand = new DelegateCommand(this.EditRadioName, () => this.BindingModel.SelectedItems.Count == 1);
            this.DeleteRadioCommand = new DelegateCommand(this.DeleteRadio, () => this.BindingModel.SelectedItems.Count > 0);
        }

        public DelegateCommand DeleteRadioCommand { get; private set; }

        public DelegateCommand EditRadioNameCommand { get; private set; }

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

        protected override void OnSelectedItemsChanged()
        {
            base.OnSelectedItemsChanged();

            this.EditRadioNameCommand.RaiseCanExecuteChanged();
            this.DeleteRadioCommand.RaiseCanExecuteChanged();
        }

        protected override IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.Edit, this.resources.GetString("Toolbar_RenameButton"), this.EditRadioNameCommand);
            yield return new CommandMetadata(CommandIcon.Delete, this.resources.GetString("Toolbar_DeleteButton"), this.DeleteRadioCommand);
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

                        foreach (RadioPlaylist playlist in playlists)
                        {
                            await this.radioWebService.DeleteStationAsync(playlist.Id);
                        }

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

        private void EditRadioName()
        {
            if (this.EditRadioNameCommand.CanExecute())
            {
                this.MainFrame.ShowPopup<IRadioEditPopupView>(PopupRegion.AppToolBarLeft, this.BindingModel.SelectedItems[0].Playlist);
            }
        }
    }
}
