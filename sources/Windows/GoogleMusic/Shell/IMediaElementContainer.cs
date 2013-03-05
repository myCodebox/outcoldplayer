//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Threading.Tasks;

    using Windows.Storage.Streams;

    public interface IMediaElementContainer
    {
        event EventHandler<MediaEndedEventArgs> MediaEnded;

        event EventHandler<PlayProgressEventArgs> PlayProgress;

        double Volume { get; set; }

        Task PlayAsync(IRandomAccessStream stream, string mimeType);

        Task PlayAsync();

        Task PauseAsync();

        Task StopAsync();

        Task SetPositionAsync(TimeSpan position);
    }
}