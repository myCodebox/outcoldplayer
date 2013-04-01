// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;

    public class PlaylistsChangeEvent
    {
        private PlaylistsChangeEvent(PlaylistType playlistType)
        {
            PlaylistType = playlistType;
        }

        public PlaylistType PlaylistType { get; private set; }

        public IList<IPlaylist> AddedPlaylists { get; private set; }

        public IList<IPlaylist> RemovedPlaylists { get; private set; }

        public IList<IPlaylist> UpdatedPlaylists { get; private set; }

        public static PlaylistsChangeEvent New(PlaylistType playlistType)
        {
            return new PlaylistsChangeEvent(playlistType);
        }

        public PlaylistsChangeEvent AddAddedPlaylists(params IPlaylist[] playlists)
        {
            if (playlists == null)
            {
                throw new ArgumentNullException("playlists");
            }

            if (this.AddedPlaylists == null)
            {
                this.AddedPlaylists = new List<IPlaylist>(playlists);
            }

            return this;
        }

        public PlaylistsChangeEvent AddRemovedPlaylists(params IPlaylist[] playlists)
        {
            if (playlists == null)
            {
                throw new ArgumentNullException("playlists");
            }

            if (this.RemovedPlaylists == null)
            {
                this.RemovedPlaylists = new List<IPlaylist>(playlists);
            }

            return this;
        }

        public PlaylistsChangeEvent AddUpdatedPlaylists(params IPlaylist[] playlists)
        {
            if (playlists == null)
            {
                throw new ArgumentNullException("playlists");
            }

            if (this.UpdatedPlaylists == null)
            {
                this.UpdatedPlaylists = new List<IPlaylist>(playlists);
            }

            return this;
        }

        public bool HasAddedPlaylists()
        {
            return this.AddedPlaylists != null && this.AddedPlaylists.Count > 0;
        }

        public bool HasRemovedPlaylists()
        {
            return this.RemovedPlaylists != null && this.RemovedPlaylists.Count > 0;
        }

        public bool HasUpdatedPlaylists()
        {
            return this.UpdatedPlaylists != null && this.UpdatedPlaylists.Count > 0;
        }
    }
}
