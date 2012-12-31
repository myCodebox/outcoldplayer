// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface ICurrentPlaylistView : IView
    {
        int SelectedSongIndex { get; set; }
    }

    public sealed partial class CurrentPlaylistView : ViewBase, ICurrentPlaylistView
    {
        private readonly List<Button> contextButtons = new List<Button>();
        private readonly ICurrentContextCommands currentContextCommands;

        public CurrentPlaylistView()
        {
            this.InitializePresenter<CurrentPlaylistViewPresenter>();
            this.InitializeComponent();

            this.currentContextCommands = App.Container.Resolve<ICurrentContextCommands>();

            this.contextButtons.Add(new Button()
                                        {
                                            Style = (Style)Application.Current.Resources["PlayAppBarButtonStyle"],
                                            Command = this.Presenter<CurrentPlaylistViewPresenter>().BindingModel.PlaySelectedSong
                                        });

            this.contextButtons.Add(new Button()
                                        {
                                            Style = (Style)Application.Current.Resources["RemoveAppBarButtonStyle"],
                                            Command = this.Presenter<CurrentPlaylistViewPresenter>().BindingModel.RemoveSelectedSong
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
            }
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
    }
}
