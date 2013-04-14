// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class ArtistPageViewBindingModel : BindingModelBase
    {
        private readonly ObservableCollection<PlaylistBindingModel> selectedItems;
        private Artist artist;
        private IList<PlaylistBindingModel> albums;

        public ArtistPageViewBindingModel()
        {
            this.selectedItems = new ObservableCollection<PlaylistBindingModel>();
        }

        public Artist Artist
        {
            get
            {
                return this.artist;
            }

            set
            {
                this.SetValue(ref this.artist, value);
            }
        }

        public IList<PlaylistBindingModel> Albums
        {
            get
            {
                return this.albums;
            }

            set
            {
                this.SetValue(ref this.albums, value);
            }
        }

        public ObservableCollection<PlaylistBindingModel> SelectedItems
        {
            get
            {
                return this.selectedItems;
            }
        }

        public void ClearSelectedItems()
        {
            if (this.selectedItems.Count > 0)
            {
                this.selectedItems.Clear();
            }
        }
    }
}