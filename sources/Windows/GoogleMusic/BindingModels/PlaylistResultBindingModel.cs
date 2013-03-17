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
        private readonly PlaylistBaseBindingModel result;

        public PlaylistResultBindingModel(string search, PlaylistBaseBindingModel result)
            : base(search, result.Title)
        {
            this.result = result;
        }

        public PlaylistBaseBindingModel Result
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
                return string.Format(CultureInfo.CurrentCulture, "{0} songs", this.result.Songs.Count);
            }
        }

        public override string Description
        {
            get
            {
                if (this.result is AlbumBindingModel)
                {
                    return "Album";
                }

                if (this.result is ArtistBindingModel)
                {
                    return "Artist";
                }

                if (this.result is GenreBindingModel)
                {
                    return "Genre";
                }

                if (this.result is UserPlaylistBindingModel)
                {
                    return "Playlist";
                }

                return null;
            }
        }

        public override Uri ImageUrl
        {
            get
            {
                return this.result.AlbumArtUrl;
            }
        }
    }
}