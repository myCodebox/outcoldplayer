// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistViewResolver : IPageViewResolver
    {
        public Type GetViewType(object parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (parameter is AlbumBindingModel)
            {
                return typeof(IAlbumPageView);
            }
            
            if (parameter is ArtistBindingModel)
            {
                return typeof(IArtistPageView);
            }

            return typeof(IPlaylistPageView);
        }
    }
}