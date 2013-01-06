// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Automation;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public interface IPlaylistView : IView
    {
        int SelectedIndex { get; set; }

        void ShowPlaylists(List<MusicPlaylist> playlists);

        void SetIsLoading(bool value);
    }

    public sealed partial class PlaylistView : ViewBase, IPlaylistView
    {
        private readonly ICurrentContextCommands currentContextCommands;

        private readonly Button playButton;
        private readonly Button addToPlaylistButton;
        private readonly Button removeButton;
        private readonly Border borderSeparator;

        public PlaylistView()
        {
            this.InitializePresenter<PlaylistViewPresenter>();
            this.InitializeComponent();

            this.currentContextCommands = App.Container.Resolve<ICurrentContextCommands>();

            this.playButton = new Button()
                                  {
                                      Style = (Style)Application.Current.Resources["PlayAppBarButtonStyle"],
                                      Command = this.Presenter<PlaylistViewPresenter>().PlaySelectedSongCommand
                                  };

            this.removeButton = new Button()
                                  {
                                      Style = (Style)Application.Current.Resources["RemoveAppBarButtonStyle"],
                                      Command = this.Presenter<PlaylistViewPresenter>().RemoveFromPlaylistCommand
                                  };

            this.addToPlaylistButton = new Button()
                                {
                                    Style = (Style)Application.Current.Resources["AddAppBarButtonStyle"],
                                    Command = this.Presenter<PlaylistViewPresenter>().AddToPlaylistCommand
                                };
            this.addToPlaylistButton.SetValue(AutomationProperties.NameProperty, "Playlist");

            this.borderSeparator = new Border() { Style = (Style)Application.Current.Resources["AppBarSeparator"] };
        }

        public int SelectedIndex
        {
            get
            {
                return this.ListView.SelectedIndex;
            }

            set
            {
                this.ListView.SelectedIndex = value;
                if (value >= 0 && this.ListView.SelectedItem != null)
                {
                    this.ListView.ScrollIntoView(this.ListView.SelectedItem);
                }
            }
        }

        public override void OnNavigatedTo(object parameter)
        {
            this.ListView.SelectedIndex = -1;
            if (this.ListView.Items != null && this.ListView.Items.Count > 0)
            {
                this.ListView.ScrollIntoView(this.ListView.Items[0]);
            }

            base.OnNavigatedTo(parameter);
        }

        public void ShowPlaylists(List<MusicPlaylist> playlists)
        {
            this.PlaylistsView.ItemsSource = playlists;
            this.PlaylistsPopup.VerticalOffset = this.ActualHeight - 400;
            this.PlaylistsPopup.IsOpen = true;
        }

        public void SetIsLoading(bool value)
        {
            this.ProgressRing.IsActive = value;
            this.ListView.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ListOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedIndex >= 0)
            {
                List<UIElement> elements = new List<UIElement>
                                               {
                                                   this.playButton,
                                                   this.addToPlaylistButton
                                               };

                if (this.Presenter<PlaylistViewPresenter>().BindingModel.Playlist is MusicPlaylist)
                {
                    elements.Add(this.borderSeparator);
                    elements.Add(this.removeButton);
                }

                this.currentContextCommands.SetCommands(elements);
            }
            else
            {
                this.currentContextCommands.ClearContext();
            }
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.PlaylistsPopup.IsOpen)
            {
                this.Presenter<PlaylistViewPresenter>().AddSelectedSongToPlaylist((MusicPlaylist)e.ClickedItem);
                this.PlaylistsPopup.IsOpen = false;
            }
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.Presenter<PlaylistViewPresenter>().PlaySelectedSongCommand.Execute(null);
        }
    }
}
