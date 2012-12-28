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
        SongBindingModel SelectedSong { get; set; }
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

        public SongBindingModel SelectedSong
        {
            get
            {
                return this.ListView.SelectedItem as SongBindingModel;
            }

            set
            {
                this.ListView.SelectedItem = value;
            }
        }

        private void ListOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedSong != null)
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
