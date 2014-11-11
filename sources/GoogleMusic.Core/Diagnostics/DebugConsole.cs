//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Diagnostics
{
    using System.Diagnostics;

    public class DebugConsole : IDebugConsole
    {
        public void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
