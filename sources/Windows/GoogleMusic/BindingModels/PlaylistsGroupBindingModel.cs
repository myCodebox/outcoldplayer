// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistsGroupBindingModel
    {
        public PlaylistsGroupBindingModel(string title, int playlistsCount, IEnumerable<PlaylistBindingModel> playlists, PlaylistsRequest request)
        {
            this.Title = title;
            this.Playlists = new ObservableCollection<PlaylistBindingModel>(playlists);
            this.PlaylistsCount = playlistsCount;
            this.Request = request;
        }

        public PlaylistsGroupBindingModel(string title, int playlistsCount, IEnumerable<PlaylistBindingModel> playlists)
        {
            this.Title = title;
            this.PlaylistsCount = playlistsCount;
            this.Playlists = new ObservableCollection<PlaylistBindingModel>(playlists);
        }

        public string Title { get; private set; }

        public int PlaylistsCount { get; private set; }

        public ObservableCollection<PlaylistBindingModel> Playlists { get; private set; }

        public PlaylistsRequest Request { get; set; }
    }
}