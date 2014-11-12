// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;

    public interface ITimer
    {
        event EventHandler<object> Tick;

        TimeSpan Interval { get; set; }

        void Start();

        void Stop();
    }
}
