// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Services;

    public class QueueChangeEvent
    {
        public QueueChangeEvent(
            bool isShuffleEnabled,
            RepeatType repeat, 
            bool isRadio,
            IEnumerable<Song> songsQueue)
        {
            this.IsShuffleEnabled = isShuffleEnabled;
            this.Repeat = repeat;
            this.IsRadio = isRadio;
            this.SongsQueue = songsQueue;
        }

        public bool IsShuffleEnabled { get; set; }

        public RepeatType Repeat { get; set; }

        public bool IsRadio { get; set; }

        public IEnumerable<Song> SongsQueue { get; set; }
    }
}