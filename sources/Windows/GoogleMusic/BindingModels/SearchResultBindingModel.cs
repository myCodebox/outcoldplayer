// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using OutcoldSolutions.GoogleMusic.Models;

    public abstract class SearchResultBindingModel
    {
        protected SearchResultBindingModel(string searchString, string title)
        {
            int indexOf = Search.IndexOf(title, searchString);
            if (indexOf < 0)
            {
                this.PreTitle = title;
            }
            else
            {
                if (indexOf > 0)
                {
                    this.PreTitle = title.Substring(0, indexOf);
                }

                this.HighlightedTitle = title.Substring(indexOf, searchString.Length);

                var postTitleLength = title.Length - (indexOf + searchString.Length);
                if (postTitleLength > 0)
                {
                    this.PostTitle = title.Substring(indexOf + searchString.Length, postTitleLength);
                }
            }
        }

        public string PreTitle { get; private set; }

        public string HighlightedTitle { get; private set; }

        public string PostTitle { get; private set; }

        public abstract string Subtitle { get; }

        public abstract string Description { get; }

        public abstract string ImageUrl { get; }
    }
}