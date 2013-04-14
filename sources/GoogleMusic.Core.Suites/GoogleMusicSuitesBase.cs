// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites
{
    using System.Diagnostics;

    using OutcoldSolutions.Diagnostics;

    public class GoogleMusicSuitesBase : SuitesBase
    {
        public GoogleMusicSuitesBase()
            : base(new DebugConsole())
        {
        }

        private class DebugConsole : IDebugConsole
        {
            public void WriteLine(string message)
            {
                Debug.WriteLine(message);
            }
        }
    }
}