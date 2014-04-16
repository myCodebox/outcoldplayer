// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class RadioEditPopupViewPresenter : DisposableViewPresenterBase<IPlaylistEditPopupView>, IPlaylistEditPopupViewPresenter
    {
        private readonly IRadioStationsService radioStationsService;
        private readonly Radio radio;

        private string title;

        private bool isRenaming = false;

        public RadioEditPopupViewPresenter(
            IRadioStationsService radioStationsService,
            Radio radio)
        {
            this.radioStationsService = radioStationsService;
            this.radio = radio;
            this.SaveCommand = new DelegateCommand(this.Save, this.CanSave);
            this.CancelCommand = new DelegateCommand(this.Cancel);
            this.Title = radio.Title;
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

        private async void Save()
        {
            this.isRenaming = true;
            this.SaveCommand.RaiseCanExecuteChanged();

            try
            {
                if (await this.radioStationsService.RenameStationAsync(this.radio, this.Title))
                {
                    this.EventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.Radio));
                    this.View.Close();
                }
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