// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    public class SelectedItems
    {
        public SelectedItems(IList<Song> songs)
        {
            this.Songs = songs;
        }

        public SelectedItems(IList<IPlaylist> playlists)
        {
            this.Playlists = playlists;
        }

        public IList<Song> Songs { get; set; }

        public IList<IPlaylist> Playlists { get; set; }
    }
}