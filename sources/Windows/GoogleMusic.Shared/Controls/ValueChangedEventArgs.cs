// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(int newValue)
        {
            this.NewValue = newValue;
        }

        public int NewValue { get; private set; }
    }
}