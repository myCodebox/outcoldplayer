// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    public class QueueChangeEvent
    {
        public QueueChangeEvent(
            bool isShuffleEnabled, 
            bool isRepeatAllEnabled, 
            bool isRadio,
            IEnumerable<Song> songsQueue)
        {
            this.IsShuffleEnabled = isShuffleEnabled;
            this.IsRepeatAllEnabled = isRepeatAllEnabled;
            this.IsRadio = isRadio;
            this.SongsQueue = songsQueue;
        }

        public bool IsShuffleEnabled { get; set; }

        public bool IsRepeatAllEnabled { get; set; }

        public bool IsRadio { get; set; }

        public IEnumerable<Song> SongsQueue { get; set; }
    }
}