// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    public class SettingsChangeEvent
    {
        public SettingsChangeEvent(string key, object newValue, object oldValue)
        {
            this.Key = key;
            this.NewValue = newValue;
            this.OldValue = oldValue;
        }

        public string Key { get; private set; }

        public object NewValue { get; private set; }

        public object OldValue { get; private set; }
    }
}