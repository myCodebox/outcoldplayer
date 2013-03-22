// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ICurrentSongPublisher
    {
        PublisherType PublisherType { get; }

        Task PublishAsync(Song song, IPlaylist currentPlaylist, Uri imageUri, CancellationToken cancellationToken);
    }
}