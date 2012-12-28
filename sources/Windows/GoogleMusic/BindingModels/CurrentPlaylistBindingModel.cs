// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    public class CurrentPlaylistBindingModel : SongsBindingModelBase
    {
        public DelegateCommand PlaySelectedSong { get; set; }

        public DelegateCommand RemoveSelectedSong { get; set; }
    }
}