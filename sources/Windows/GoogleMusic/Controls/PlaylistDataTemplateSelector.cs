// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class PlaylistDataTemplateSelector : DataTemplateSelector 
    {
        public DataTemplate ArtistDataTemplate { get; set; }

        public DataTemplate AlbumDataTemplate { get; set; }

        public DataTemplate GenreDataTemplate { get; set; }

        public DataTemplate SystemPlaylistDataTemplate { get; set; }

        public DataTemplate UserPlaylistDataTemplate { get; set; }

        public DataTemplate PlaylistDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var groupPlaylistBindingModel = item as GroupPlaylistBindingModel;
            object playlist = null;
            if (groupPlaylistBindingModel != null)
            {
                playlist = groupPlaylistBindingModel.Playlist;
            }

            if (playlist is ArtistEntity)
            {
                Debug.Assert(this.ArtistDataTemplate != null, "this.ArtistDataTemplate != null");
                return this.ArtistDataTemplate;
            }

            if (playlist is AlbumEntity)
            {
                Debug.Assert(this.AlbumDataTemplate != null, "this.AlbumDataTemplate != null");
                return this.AlbumDataTemplate;
            }

            if (playlist is GenreEntity)
            {
                Debug.Assert(this.GenreDataTemplate != null, "this.GenreDataTemplate != null");
                return this.GenreDataTemplate;
            }

            if (playlist is SystemPlaylist)
            {
                Debug.Assert(this.SystemPlaylistDataTemplate != null, "this.SystemPlaylistDataTemplate != null");
                return this.SystemPlaylistDataTemplate;
            }

            if (playlist is UserPlaylistEntity)
            {
                Debug.Assert(this.UserPlaylistDataTemplate != null, "this.UserPlaylistDataTemplate != null");
                return this.UserPlaylistDataTemplate;
            }

            Debug.Assert(this.PlaylistDataTemplate != null, "this.PlaylistDataTemplate != null");
            return this.PlaylistDataTemplate;
            // return base.SelectTemplateCore(item, container);
        }
    }
}
