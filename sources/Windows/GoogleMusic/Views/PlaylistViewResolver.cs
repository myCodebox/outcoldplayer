// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;

    public class PlaylistViewResolver : IPageViewResolver
    {
        public Type GetViewType(object parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (parameter is Album)
            {
                return typeof(IAlbumPageView);
            }
            
            if (parameter is Artist)
            {
                return typeof(IArtistPageView);
            }

            return typeof(IPlaylistPageView);
        }
    }
}