// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public interface ICurrentSongPublisher
    {
        PublisherType PublisherType { get; }

        Task PublishAsync(Song song, ISongsContainer currentPlaylist, Uri imageUri, CancellationToken cancellationToken);
    }
}