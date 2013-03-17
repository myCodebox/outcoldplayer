// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    public interface ICurrentSongPublisherService
    {
        void AddPublisher<TPublisherType>() where TPublisherType : ICurrentSongPublisher;

        void RemovePublishers<TPublisherType>() where TPublisherType : ICurrentSongPublisher;

        Task PublishAsync(SongBindingModel song, PlaylistBaseBindingModel currentPlaylist);

        void CancelActiveTasks();
    }
}