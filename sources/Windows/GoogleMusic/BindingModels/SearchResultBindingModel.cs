// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    public abstract class SearchResultBindingModel
    {
        public abstract string Title { get; }

        public abstract string Subtitle { get; }

        public abstract string Description { get; }

        public abstract string ImageUrl { get; }
    }
}