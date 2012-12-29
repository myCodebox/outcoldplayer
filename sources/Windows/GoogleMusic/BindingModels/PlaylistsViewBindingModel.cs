// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    public class PlaylistsViewBindingModel : BindingModelBase
    {
        private int count;
        private string title;
        private bool isLoading;

        public PlaylistsViewBindingModel()
        {
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

        public int Count
        {
            get
            {
                return this.count;
            }

            set
            {
                if (this.count != value)
                {
                    this.count = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.RaiseCurrentPropertyChanged();
                }
            }
        }
    }
}