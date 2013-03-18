// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface ICurrentSongPublisherService
    {
        void AddPublisher<TPublisherType>() where TPublisherType : ICurrentSongPublisher;

        void RemovePublishers<TPublisherType>() where TPublisherType : ICurrentSongPublisher;

        Task PublishAsync(Song song, ISongsContainer currentPlaylist);

        void CancelActiveTasks();
    }
}