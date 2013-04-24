// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistResultBindingModel : SearchResultBindingModel
    {
        private readonly IPlaylist result;
        private readonly string description;
        private readonly string subtitle;

        public PlaylistResultBindingModel(
            string search, 
            IPlaylist result,
            string description,
            string subtitle)
            : base(search, result.Title)
        {
            this.result = result;
            this.description = description;
            this.subtitle = subtitle;
        }

        public IPlaylist Result
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
                return this.subtitle;
            }
        }

        public override string Description
        {
            get
            {
                return this.description;
            }
        }

        public override Uri ImageUrl
        {
            get
            {
                return this.result.ArtUrl;
            }
        }
    }
}