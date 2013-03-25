// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    public class QueueChangeEvent
    {
        public QueueChangeEvent(
            bool isShuffleEnabled, 
            bool isRepeatAllEnabled)
        {
            this.IsShuffleEnabled = isShuffleEnabled;
            this.IsRepeatAllEnabled = isRepeatAllEnabled;
        }

        public bool IsShuffleEnabled { get; set; }

        public bool IsRepeatAllEnabled { get; set; }
    }
}