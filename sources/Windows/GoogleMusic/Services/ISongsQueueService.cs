// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface ISongsQueueService
    {
        Task PlayAsync(IPlaylist playlist);
    }
}
