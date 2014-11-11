// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;

    public sealed partial class PlaylistsListViewContainerControl : UserControl
    {
        public PlaylistsListViewContainerControl()
        {
            this.InitializeComponent();

            var playlistsListView = ApplicationContext.Container.Resolve<IPlaylistsListView>();

            this.Content = (UIElement)playlistsListView;

            var listView = playlistsListView as PlaylistsListView;
            if (listView != null)
            {
                listView.MaxItems = 5;
                listView.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
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
