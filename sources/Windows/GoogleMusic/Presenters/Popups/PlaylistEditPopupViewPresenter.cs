// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;

    public class PlaylistEditPopupViewPresenter : DisposableViewPresenterBase<IPlaylistEditPopupView>
    {
        private readonly ISearchService searchService;
        private readonly IUserPlaylistsService userPlaylistsService;
        private readonly UserPlaylist userPlaylist;

        private string title;

        public PlaylistEditPopupViewPresenter(
            ISearchService searchService,
            IUserPlaylistsService userPlaylistsService,
            UserPlaylist userPlaylist)
        {
            this.searchService = searchService;
            this.userPlaylistsService = userPlaylistsService;
            this.userPlaylist = userPlaylist;
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
            this.CancelCommand = new DelegateCommand(this.Cancel);
            this.Title = this.userPlaylist.Id > 0
                       ? this.userPlaylist.Title
                       : DateTime.Now.ToString(CultureInfo.CurrentCulture);
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

        private void Save()
        {
            if (this.userPlaylist.Id > 0)
            {
                this.Logger.LogTask(this.userPlaylistsService.ChangeNameAsync(this.userPlaylist, this.Title));
            }
            else
            {
                this.Logger.LogTask(this.userPlaylistsService.CreateAsync(this.Title));
            }
            
            this.View.Close();
        }

        private bool CanSave()
        {
            return !string.IsNullOrEmpty(this.Title);
        }

        private void Cancel()
        {
            this.View.Close();
        }
    }
}
