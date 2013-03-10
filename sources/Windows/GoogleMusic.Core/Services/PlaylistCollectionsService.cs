// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public class PlaylistCollectionsService : IPlaylistCollectionsService
    {
        private readonly IDependencyResolverContainer container;

        public PlaylistCollectionsService(
            IDependencyResolverContainer container)
        {
            this.container = container;
        }

        public IPlaylistCollection<TPlaylist> GetCollection<TPlaylist>() where TPlaylist : Playlist
        {
            return this.container.Resolve<IPlaylistCollection<TPlaylist>>();
        }
    }
}