// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    public class SettingsValueChangedEventArgs : EventArgs
    {
        public SettingsValueChangedEventArgs(string key)
        {
            this.Key = key;
        }

        public string Key { get; set; }
    }
}