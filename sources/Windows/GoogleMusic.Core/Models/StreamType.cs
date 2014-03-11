// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    [Flags]
    public enum StreamType : byte
    {
        Uploaded = 0,

        EphemeralSubscription = 1,

        OwnLibrary = 2,

        Purchased = 4,

        Free = 6,

        AllAccess = 7,

        AllAccessLibrary = 8
    }
}