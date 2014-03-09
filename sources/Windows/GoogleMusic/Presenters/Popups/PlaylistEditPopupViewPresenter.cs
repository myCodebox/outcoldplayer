// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class PlaylistEditCompletedEventArgs : EventArgs
    {
        
    }

    public interface IPlaylistEditPopupViewPresenter
    {
        string Title { get; set; }

        DelegateCommand SaveCommand { get; }

        DelegateCommand CancelCommand { get; }
    }

    public class PlaylistEditPopupViewPresenter : DisposableViewPresenterBase<IPlaylistEditPopupView>, IPlaylistEditPopupViewPresenter
    {
        private readonly IUserPlaylistsService userPlaylistsService;
        private readonly UserPlaylist userPlaylist;

        private string title;

        public PlaylistEditPopupViewPresenter(
            IUserPlaylistsService userPlaylistsService,
            UserPlaylist userPlaylist)
        {
            this.userPlaylistsService = userPlaylistsService;
            this.userPlaylist = userPlaylist;
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
            this.CancelCommand = new DelegateCommand(this.Cancel);
            this.Title = !string.IsNullOrEmpty(this.userPlaylist.PlaylistId)
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

        private void Save()
        {
            if (!string.IsNullOrEmpty(this.userPlaylist.PlaylistId))
            {
                this.Logger.LogTask(this.userPlaylistsService.ChangeNameAsync(this.userPlaylist, this.Title));
            }
            else
            {
                this.Logger.LogTask(this.userPlaylistsService.CreateAsync(this.Title));
            }
            
            this.View.Close(new PlaylistEditCompletedEventArgs());
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
