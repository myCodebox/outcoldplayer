// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.Presenters;

    public class RadioEditPopupViewPresenter : DisposableViewPresenterBase<IPlaylistEditPopupView>, IPlaylistEditPopupViewPresenter
    {
        private readonly ISearchService searchService;
        private readonly IRadioWebService radioWebService;
        private readonly RadioPlaylist radioPlaylist;

        private string title;

        private bool isRenaming = false;

        public RadioEditPopupViewPresenter(
            ISearchService searchService,
            IRadioWebService radioWebService,
            RadioPlaylist radioPlaylist)
        {
            this.searchService = searchService;
            this.radioWebService = radioWebService;
            this.radioPlaylist = radioPlaylist;
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
            this.CancelCommand = new DelegateCommand(this.Cancel);
            this.Title = radioPlaylist.Title;
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.SetValue(ref this.title, value))
                {
                    this.SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand SaveCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.searchService.SetShowOnKeyboardInput(false);
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();
            this.searchService.SetShowOnKeyboardInput(true);
        }

        private async void Save()
        {
            this.isRenaming = true;
            this.SaveCommand.RaiseCanExecuteChanged();

            try
            {
                await this.radioWebService.RenameStationAsync(this.radioPlaylist, this.Title);
                this.EventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.Radio));

                this.View.Close();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot rename radio station");
            }
            
            this.isRenaming = false;
            this.SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !string.IsNullOrEmpty(this.Title) && !this.isRenaming;
        }

        private void Cancel()
        {
            this.View.Close();
        }
    }
}