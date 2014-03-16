// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicTabs
    {
        public string Kind { get; set; }

        public GoogleMusicTab[] Tabs { get; set; }
    }

    public class GoogleMusicTab
    {
        public string Kind { get; set; }

        public string Tab_Type { get; set; }

        public GoogleMusicTabGroup[] Groups { get; set; }
    }

    public class GoogleMusicTabGroup
    {
        public string Kind { get; set; }

        public string Title { get; set; }

        public GoogleMusicTabGroupEntity[] Entities { get; set; }

        public string Continuation_Token { get; set; }

        public int Start_Position { get; set; }

        public string Group_Type { get; set; }
    }

    public class GoogleMusicTabGroupEntity
    {
        public string Kind { get; set; }

        public GoogleMusicAlbum Album { get; set; }

        public GoogleMusicPlaylist Playlist { get; set; }

        public GoogleMusicSong Track { get; set; }
    }
}
