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
    using Windows.UI.Xaml.Controls;

    public interface IPlaylistView : IView
    {
        int SelectedIndex { get; set; }
    }

    public sealed partial class PlaylistView : ViewBase, IPlaylistView
    {
        private readonly ICurrentContextCommands currentContextCommands;

        private readonly Button playButton;
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
                if (value > 0 && this.ListView.SelectedItem != null)
                {
                    this.ListView.ScrollIntoView(this.ListView.SelectedItem);
                }
            }
        }

        private void ListOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedIndex >= 0)
            {
                List<UIElement> elements = new List<UIElement>() { this.playButton };

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
    }
}
