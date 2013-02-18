// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Collections.Generic;

    public interface IApplicationToolbar
    {
        void SetViewCommands(IEnumerable<CommandMetadata> commands);

        void ClearViewCommands();

        void SetContextCommands(IEnumerable<CommandMetadata> commands);

        void ClearContextCommands();
    }
}