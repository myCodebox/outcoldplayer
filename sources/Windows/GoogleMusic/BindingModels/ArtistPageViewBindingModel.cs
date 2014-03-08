// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class ArtistPageViewBindingModel : BindingModelBase
    {
        private Artist artist;
        private IList<IPlaylist> albums;
        private IList<IPlaylist> collections;

        public ArtistPageViewBindingModel()
        {
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

        public IList<IPlaylist> Albums
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

        public IList<IPlaylist> Collections
        {
            get
            {
                return this.collections;
            }

            set
            {
                this.SetValue(ref this.collections, value);
            }
        }
    }
}