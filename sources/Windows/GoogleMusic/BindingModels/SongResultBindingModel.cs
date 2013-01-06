// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class SongResultBindingModel : SearchResultBindingModel
    {
        private readonly Song result;

        public SongResultBindingModel(Song result)
        {
            this.result = result;
        }

        public Song Result
        {
            get
            {
                return this.result;
            }
        }

        public override string Title
        {
            get
            {
                return this.result.Title;
            }
        }

        public override string Subtitle
        {
            get
            {
                return this.result.Artist;
            }
        }

        public override string Description
        {
            get
            {
                return "Song";
            }
        }

        public override string ImageUrl
        {
            get
            {
                return this.result.GoogleMusicMetadata.AlbumArtUrl;
            }
        }
    }
}