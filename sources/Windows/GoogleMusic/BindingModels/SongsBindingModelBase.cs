// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    using OutcoldSolutions.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    
    public abstract class SongsBindingModelBase : BindingModelBase
    {
        protected SongsBindingModelBase()
        {
            this.Songs = new ObservableCollection<SongBindingModel>();
        }

        public ObservableCollection<SongBindingModel> Songs { get; private set; }
    }
}