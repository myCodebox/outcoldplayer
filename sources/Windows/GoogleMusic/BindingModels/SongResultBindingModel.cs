// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;

    public class SongResultBindingModel : SearchResultBindingModel
    {
        private readonly Song result;

        public SongResultBindingModel(string search, Song result)
            : base(search, result.Title)
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