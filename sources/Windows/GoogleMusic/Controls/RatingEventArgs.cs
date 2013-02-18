// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;

    public class RatingEventArgs : EventArgs
    {
        public RatingEventArgs(object commandParameter, int value)
        {
            this.CommandParameter = commandParameter;
            this.Value = value;
        }

        public object CommandParameter { get; private set; }

        public int Value { get; private set; }
    }
}