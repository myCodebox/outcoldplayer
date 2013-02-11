// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Automation;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public interface ICurrentPlaylistView : IPageView
    {
        int SelectedSongIndex { get; set; }

        void ShowPlaylists(List<MusicPlaylist> playlists);

        void SelectCurrentSong();
    }

    public sealed partial class CurrentPlaylistPageView : PageViewBase, ICurrentPlaylistView
    {
        private readonly List<UIElement> contextButtons = new List<UIElement>();
        private readonly ICurrentContextCommands currentContextCommands;

        private CurrentPlaylistViewPresenter presenter;

        public CurrentPlaylistPageView()
        {
            this.InitializeComponent();
            this.presenter = this.InitializePresenter<CurrentPlaylistViewPresenter>();

            this.currentContextCommands = App.Container.Resolve<ICurrentContextCommands>();

            this.contextButtons.Add(new Button()
                                        {
                                            Style = (Style)Application.Current.Resources["PlayAppBarButtonStyle"],
                                            Command = this.presenter.BindingModel.PlaySelectedSong
                                        });

            var addToPlaylistButton = new Button()
            {
                Style = (Style)Application.Current.Resources["AddAppBarButtonStyle"],
                Command = this.presenter.AddToPlaylistCommand
            };

            addToPlaylistButton.SetValue(AutomationProperties.NameProperty, "Playlist");

            this.contextButtons.Add(addToPlaylistButton);

            this.contextButtons.Add(new Border() { Style = (Style)Application.Current.Resources["AppBarSeparator"] });

            this.contextButtons.Add(new Button()
                                        {
                                            Style = (Style)Application.Current.Resources["RemoveAppBarButtonStyle"],
                                            Command = this.presenter.BindingModel.RemoveSelectedSong
                                        });
        }

        public int SelectedSongIndex
        {
            get
            {
                return this.ListView.SelectedIndex;
            }

            set
            {
                this.ListView.SelectedIndex = value;
                this.ListView.ScrollIntoView(this.ListView.SelectedItem);
            }
        }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            if (this.ListView.Items != null && this.ListView.Items.Count > 0)
            {
                int currentSongIndex = App.Container.Resolve<ICurrentPlaylistService>().CurrentSongIndex;
                this.ListView.SelectedIndex = currentSongIndex;
                this.ListView.ScrollIntoView(this.ListView.SelectedItem);
            }

            base.OnNavigatedTo(eventArgs);
        }

        public void ShowPlaylists(List<MusicPlaylist> playlists)
        {
            this.PlaylistsView.ItemsSource = playlists;
            this.PlaylistsPopup.VerticalOffset = this.ActualHeight - 400;
            this.PlaylistsPopup.IsOpen = true;
        }

        public void SelectCurrentSong()
        {
            int currentSongIndex = App.Container.Resolve<ICurrentPlaylistService>().CurrentSongIndex;
            this.ListView.SelectedIndex = currentSongIndex;
            this.ListView.ScrollIntoView(this.ListView.SelectedItem);
        }

        private void ListOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedSongIndex >= 0)
            {
                this.currentContextCommands.SetCommands(this.contextButtons);
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
                this.presenter.AddSelectedSongToPlaylist((MusicPlaylist)e.ClickedItem);
                this.PlaylistsPopup.IsOpen = false;
            }
        }

        private void ListDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.presenter.BindingModel.PlaySelectedSong.Execute(null);
        }

        private void RatingOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.presenter.UpdateRating((Song)((Rating)sender).DataContext, (byte)e.NewValue);
        }
    }
}
