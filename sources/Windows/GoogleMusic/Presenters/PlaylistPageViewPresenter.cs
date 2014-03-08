// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistPageViewPresenter : PlaylistPageViewPresenterBase<IPlaylistPageView, IPlaylist>
    {
        public PlaylistPageViewPresenter(
            IDependencyResolverContainer container)
            : base(container)
        {
        }
    }
}