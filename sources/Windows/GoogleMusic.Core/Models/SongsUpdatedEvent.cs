// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class SongsUpdatedEvent
    {
        public SongsUpdatedEvent(IEnumerable<Song> updatedSongs)
        {
            this.UpdatedSongs = new ReadOnlyCollection<Song>(new List<Song>(updatedSongs));
        }

        public ICollection<Song> UpdatedSongs { get; private set; }
    }
}
