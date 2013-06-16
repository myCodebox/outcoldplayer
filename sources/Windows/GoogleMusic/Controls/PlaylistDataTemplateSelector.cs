// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class PlaylistDataTemplateSelector : DataTemplateSelector 
    {
        public DataTemplate ArtistDataTemplate { get; set; }

        public DataTemplate AlbumDataTemplate { get; set; }

        public DataTemplate GenreDataTemplate { get; set; }

        public DataTemplate SystemPlaylistDataTemplate { get; set; }

        public DataTemplate UserPlaylistDataTemplate { get; set; }

        public DataTemplate RadioDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var groupPlaylistBindingModel = item as PlaylistBindingModel;
            object playlist = null;
            if (groupPlaylistBindingModel != null)
            {
                playlist = groupPlaylistBindingModel.Playlist;
            }

            if (playlist is Artist)
            {
                Debug.Assert(this.ArtistDataTemplate != null, "this.ArtistDataTemplate != null");
                return this.ArtistDataTemplate;
            }

            if (playlist is Album)
            {
                Debug.Assert(this.AlbumDataTemplate != null, "this.AlbumDataTemplate != null");
                return this.AlbumDataTemplate;
            }

            if (playlist is Genre)
            {
                Debug.Assert(this.GenreDataTemplate != null, "this.GenreDataTemplate != null");
                return this.GenreDataTemplate;
            }

            if (playlist is SystemPlaylist)
            {
                Debug.Assert(this.SystemPlaylistDataTemplate != null, "this.SystemPlaylistDataTemplate != null");
                return this.SystemPlaylistDataTemplate;
            }

            if (playlist is UserPlaylist)
            {
                Debug.Assert(this.UserPlaylistDataTemplate != null, "this.UserPlaylistDataTemplate != null");
                return this.UserPlaylistDataTemplate;
            }

            if (playlist is RadioPlaylist)
            {
                Debug.Assert(this.RadioDataTemplate != null, "this.UserPlaylistDataTemplate != null");
                return this.RadioDataTemplate;
            }

            Debug.Assert(false, "Uknown playlist type.");
            return base.SelectTemplateCore(item, container);
        }
    }
}
