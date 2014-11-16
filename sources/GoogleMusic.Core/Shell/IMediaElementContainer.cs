//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Threading.Tasks;

    public interface IMediaElementContainer
    {
        event EventHandler<MediaEndedEventArgs> MediaEnded;

        event EventHandler<PlayProgressEventArgs> PlayProgress;

        double Volume { get; set; }

        Task<bool> IsBeginning();

        Task PlayAsync(IStream stream, string mimeType);

        Task PlayAsync();

        Task PauseAsync();

        Task StopAsync();

        Task SetPositionAsync(TimeSpan position);
    }
}