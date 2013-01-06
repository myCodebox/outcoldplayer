// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using OutcoldSolutions.GoogleMusic.Models;

    public class GroupBindingModel
    {
        public GroupBindingModel(string title, int playlistsCount, PlaylistsRequest request, IEnumerable<PlaylistBindingModel> playlists)
        {
            this.Title = title;
            this.Playlists = new ObservableCollection<PlaylistBindingModel>(playlists);
            this.PlaylistsCount = playlistsCount;
            this.Request = request;
        }

        public string Title { get; private set; }

        public int PlaylistsCount { get; private set; }

        public ObservableCollection<PlaylistBindingModel> Playlists { get; private set; }

        public PlaylistsRequest Request { get; set; }
    }
}