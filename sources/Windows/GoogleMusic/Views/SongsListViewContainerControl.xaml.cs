// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;

    public sealed partial class SongsListViewContainerControl : UserControl
    {
        public SongsListViewContainerControl()
        {
            this.InitializeComponent();

            var songsListView = ApplicationContext.Container.Resolve<ISongsListView>();

            this.Content = (UIElement)songsListView;

            var listView = songsListView as SongsListView;
            if (listView != null)
            {
                listView.MaxItems = 5;
                listView.AllowSorting = false;
                listView.SetBinding(
                    SongsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = this,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("DataContext")
                    });
            }
        }
    }
}
