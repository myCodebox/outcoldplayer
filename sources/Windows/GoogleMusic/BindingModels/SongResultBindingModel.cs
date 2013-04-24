// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    public class SongResultBindingModel : SearchResultBindingModel
    {
        private readonly SongBindingModel result;
        private readonly string subtitle;

        public SongResultBindingModel(
            string search, 
            SongBindingModel result,
            string subtitle)
            : base(search, result.Title)
        {
            this.result = result;
            this.subtitle = subtitle;
        }

        public SongBindingModel Result
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
                return this.subtitle;
            }
        }

        public override Uri ImageUrl
        {
            get
            {
                return this.result.Metadata.AlbumArtUrl;
            }
        }
    }
}