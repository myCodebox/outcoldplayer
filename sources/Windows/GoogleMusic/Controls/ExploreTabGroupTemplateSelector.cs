// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using OutcoldSolutions.GoogleMusic.Services;

    public class ExploreTabGroupTemplateSelector : DataTemplateSelector 
    {
        public DataTemplate SongsGroupDataTemplate { get; set; }

        public DataTemplate PlaylistsGroupDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var group = item as ExploreTabGroup;

            if (group != null)
            {
                if (group.Songs != null)
                {
                    return this.SongsGroupDataTemplate;
                }

                if (group.Playlists != null)
                {
                    return this.PlaylistsGroupDataTemplate;
                }
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
