// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    public class SelectSongEvent
    {
    }

    public class SelectCurrentPlaylistSongEvent : SelectSongEvent
    {
    }

    public class SelectSongByIdEvent : SelectSongEvent
    {
        public SelectSongByIdEvent(string songId)
        {
            this.SongId = songId;
        }

        public string SongId { get; private set; }
    }
}
