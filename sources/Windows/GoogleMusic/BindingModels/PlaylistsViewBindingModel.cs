// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    public class PlaylistsViewBindingModel : BindingModelBase
    {
        private bool isLoading;

        public PlaylistsViewBindingModel()
        {
            this.IsLoading = true;
            this.Playlists = new ObservableCollection<PlaylistBindingModel>();
        }

        public ObservableCollection<PlaylistBindingModel> Playlists { get; private set; }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }

            set
            {
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }
    }
}