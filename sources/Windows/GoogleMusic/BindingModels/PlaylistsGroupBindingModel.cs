// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    
    public class PlaylistsGroupBindingModel
    {
        public PlaylistsGroupBindingModel(
            string title,
            int itemsCount,
            IList<PlaylistBindingModel> playlists,
            PlaylistType type)
        {
            this.Title = title;
            this.Playlists = playlists;
            this.ItemsCount = itemsCount;
            this.Request = type;
        }

        public PlaylistsGroupBindingModel(
            string title,
            int itemsCount,
            IList<PlaylistBindingModel> playlists)
        {
            this.Title = title;
            this.ItemsCount = itemsCount;
            this.Playlists = playlists;
        }

        public string Title { get; private set; }

        public int ItemsCount { get; private set; }

        public IList<PlaylistBindingModel> Playlists { get; private set; }

        public PlaylistType Request { get; set; }
    }
}