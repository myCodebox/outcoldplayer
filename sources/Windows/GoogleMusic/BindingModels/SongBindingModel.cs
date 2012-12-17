// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class SongBindingModel
    {
        private readonly GoogleMusicSong song;

        public SongBindingModel(GoogleMusicSong song)
        {
            this.song = song;
        }

        public string Title
        {
            get
            {
                return this.song.Title;
            }
        }
    }
}