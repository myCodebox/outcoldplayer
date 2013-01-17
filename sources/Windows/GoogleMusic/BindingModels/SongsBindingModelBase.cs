// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presentation;

    public abstract class SongsBindingModelBase : BindingModelBase
    {
        protected SongsBindingModelBase()
        {
            this.Songs = new ObservableCollection<Song>();
        }

        public ObservableCollection<Song> Songs { get; private set; }
    }
}