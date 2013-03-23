// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    public class UserPlaylistsUpdateStatus
    {
        public UserPlaylistsUpdateStatus(int newPlaylists, int updatedPlaylists, int deletedPlaylists)
        {
            this.NewPlaylists = newPlaylists;
            this.UpdatedPlaylists = updatedPlaylists;
            this.DeletedPlaylists = deletedPlaylists;
        }

        public int NewPlaylists { get; private set; }

        public int UpdatedPlaylists { get; private set; }

        public int DeletedPlaylists { get; private set; }
    }
}
