// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.ObjectModel;

    public abstract class SongsBindingModelBase : BindingModelBase
    {
        protected SongsBindingModelBase()
        {
            this.Songs = new ObservableCollection<SongBindingModel>();
            this.Songs.CollectionChanged += (sender, args) =>
                {
                    for (int index = 0; index < this.Songs.Count; index++)
                    {
                        var songBindingModel = this.Songs[index];
                        songBindingModel.Index = index + 1;
                    }
                };
        }

        public ObservableCollection<SongBindingModel> Songs { get; private set; }
    }
}