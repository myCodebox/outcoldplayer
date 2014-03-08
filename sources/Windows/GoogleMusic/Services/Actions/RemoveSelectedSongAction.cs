// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Views;

    public class RemoveSelectedSongAction: ISelectedObjectAction
    {
        private readonly IApplicationResources applicationResources;
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;

        private readonly IDispatcher dispatcher;

        public RemoveSelectedSongAction(
            IApplicationResources applicationResources,
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IDispatcher dispatcher)
        {
            this.applicationResources = applicationResources;
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
            this.dispatcher = dispatcher;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Remove;
            }
        }

        public string Title
        {
            get
            {
                return this.applicationResources.GetString("Toolbar_QueueButton");
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            if (!(this.navigationService.GetCurrentView() is ICurrentPlaylistPageView))
            {
                return false;
            }

            return selectedObjects.Count > 0;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            ICurrentPlaylistPageView currentView = this.navigationService.GetCurrentView() as ICurrentPlaylistPageView;

            if (currentView != null)
            {
                var bindingModel = currentView.GetPresenter<CurrentPlaylistPageViewPresenter>().View.GetSongsListView().GetPresenter<SongsListViewPresenter>();
                IList<int> selectedIndexes = bindingModel.GetSelectedIndexes().ToList();

                await this.playQueueService.RemoveAsync(selectedIndexes);

                await this.dispatcher.RunAsync(
                    () =>
                    {
                        if (selectedIndexes.Count == 1)
                        {
                            int selectedSongIndex = selectedIndexes.First();
                            if (selectedSongIndex < bindingModel.Songs.Count)
                            {
                                bindingModel.SelectSongByIndex(selectedSongIndex);
                            }
                            else if (bindingModel.Songs.Count > 0)
                            {
                                bindingModel.SelectSongByIndex(selectedSongIndex - 1);
                            }
                        }
                    });
            }

            return true;
        }
    }
}
