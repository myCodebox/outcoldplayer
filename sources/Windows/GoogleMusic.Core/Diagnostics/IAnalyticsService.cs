// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    public interface IAnalyticsService
    {
        void SendEvent(string category, string action, string label, int? value = null);

        void SendTiming(TimeSpan timeSpan, string category, string action, string label);
    }
}