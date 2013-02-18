// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistViewResolver : IViewResolver
    {
        public Type GetViewType(object parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (parameter is Artist)
            {
                return typeof(IArtistPageView);
            }

            return typeof(IPlaylistPageView);
        }
    }
}