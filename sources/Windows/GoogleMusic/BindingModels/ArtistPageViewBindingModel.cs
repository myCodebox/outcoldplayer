// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class ArtistPageViewBindingModel : BindingModelBase
    {
        private ArtistBindingModel artist;

        private List<PlaylistBindingModel> albums;

        public ArtistBindingModel Artist
        {
            get
            {
                return this.artist;
            }

            set
            {
                this.artist = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public List<PlaylistBindingModel> Albums
        {
            get
            {
                return this.albums;
            }

            set
            {
                this.albums = value;
                this.RaiseCurrentPropertyChanged();
            }
        }
    }
}