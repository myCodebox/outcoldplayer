// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ICurrentSongPublisherService
    {
        void AddPublisher(Lazy<ICurrentSongPublisher> publisher);

        void AddPublisher(ICurrentSongPublisher publisher);

        Task PublishAsync(Song song, Playlist currentPlaylist);

        void CancelActiveTasks();
    }
}