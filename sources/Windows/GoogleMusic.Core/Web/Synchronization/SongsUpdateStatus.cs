// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    public class SongsUpdateStatus
    {
        public SongsUpdateStatus(int newSongs, int updatedSongs, int deletedSongs)
        {
            this.NewSongs = newSongs;
            this.UpdatedSongs = updatedSongs;
            this.DeletedSongs = deletedSongs;
        }

        public int NewSongs { get; private set; }

        public int UpdatedSongs { get; private set; }

        public int DeletedSongs { get; private set; }
    }
}
