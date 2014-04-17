// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public class MenuItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SeparatorDataTemplate { get; set; }

        public DataTemplate MenuItemDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var commandMetadata = item as CommandMetadata;

            if (commandMetadata != null)
            {
                if (commandMetadata.Command == null)
                {
                    return this.SeparatorDataTemplate;
                }

                return this.MenuItemDataTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
