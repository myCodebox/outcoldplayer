// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System;
    using System.Globalization;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistResultBindingModel : SearchResultBindingModel
    {
        private readonly IPlaylist result;

        public PlaylistResultBindingModel(string search, IPlaylist result)
            : base(search, result.Title)
        {
            this.result = result;
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
                return string.Format(CultureInfo.CurrentCulture, "{0} songs", this.result.SongsCount);
            }
        }

        public override string Description
        {
            get
            {
                return this.Result.PlaylistType.ToTitle();
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