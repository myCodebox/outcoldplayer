// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistNavigationRequest
    {
        public PlaylistNavigationRequest(IPlaylist playlist, string songId = null)
        {
            this.PlaylistType = playlist.PlaylistType;
            this.PlaylistId = playlist.Id;
            this.Playlist = playlist;
            this.SongId = songId;
        }

        public PlaylistNavigationRequest(IPlaylist playlist, string title, string subtitle, IList<Song> songs)
        {
            this.PlaylistType = playlist.PlaylistType;
            this.PlaylistId = playlist.Id;
            this.Playlist = playlist;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Songs = songs;
        }

        public PlaylistNavigationRequest(IPlaylist playlist, string title, string subtitle, IList<IPlaylist> playlists)
        {
            this.PlaylistType = playlist.PlaylistType;
            this.PlaylistId = playlist.Id;
            this.Playlist = playlist;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Playlists = playlists;
        }

        public PlaylistNavigationRequest(string title, string subtitle, IList<Song> songs)
        {
            this.Title = title;
            this.Subtitle = subtitle;
            this.Songs = songs;
        }

        public PlaylistNavigationRequest(string title, string subtitle, IList<IPlaylist> playlists)
        {
            this.Title = title;
            this.Subtitle = subtitle;
            this.Playlists = playlists;
        }

        public IPlaylist Playlist { get; set; }

        public PlaylistType PlaylistType { get; set; }

        public string PlaylistId { get; set; }

        public string SongId { get; set; }

        public IList<Song> Songs { get; set; }

        public IList<IPlaylist> Playlists { get; set; } 

        public string Title { get; set; }

        public string Subtitle { get; set; }
    }
}