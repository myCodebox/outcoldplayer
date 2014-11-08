// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System;

    public class FakeAnalyticsService : IAnalyticsService
    {
        public void SendEvent(string category, string action, string label, int? value = null)
        {   
        }

        public void SendTiming(TimeSpan timeSpan, string category, string action, string label)
        {
        }
    }
}