// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    public interface IPlaylistsView : IView
    {
    }

    public sealed partial class PlaylistsView : ViewBase, IPlaylistsView
    {
        private readonly Button addPlaylistButton;
        private readonly Button editPlaylistButton;
        private readonly Button deletePlaylistButton;
        private readonly Border separator;

        public PlaylistsView()
        {
            this.InitializePresenter<PlaylistsViewPresenter>();
            this.InitializeComponent();

            this.addPlaylistButton = new Button()
                                         {
                                             Style = (Style)Application.Current.Resources["AddAppBarButtonStyle"],
                                             Command = this.Presenter<PlaylistsViewPresenter>().AddPlaylistCommand
                                         };

            this.editPlaylistButton = new Button()
                                         {
                                             Style = (Style)Application.Current.Resources["EditAppBarButtonStyle"]
                                         };

            this.deletePlaylistButton = new Button()
                                         {
                                             Style = (Style)Application.Current.Resources["DeleteAppBarButtonStyle"],
                                             Command = this.Presenter<PlaylistsViewPresenter>().DeletePlaylistCommand
                                         };

            this.separator = new Border() { Style = (Style)Application.Current.Resources["AppBarSeparator"] };
        }

        public override void OnNavigatedTo(object parameter)
        {
            if (this.ListView.Items != null && this.ListView.Items.Count > 0)
            {
                this.ListView.ScrollIntoView(this.ListView.Items[0]);
            }

            base.OnNavigatedTo(parameter);

            var currentContextCommands = App.Container.Resolve<ICurrentContextCommands>();

            if (parameter is PlaylistsRequest && ((PlaylistsRequest)parameter) == PlaylistsRequest.Playlists)
            {
                this.ListView.SelectionMode = ListViewSelectionMode.Single;
                currentContextCommands.SetCommands(new List<ButtonBase>() { this.addPlaylistButton });
            }
            else
            {
                this.ListView.SelectionMode = ListViewSelectionMode.None;
                currentContextCommands.SetCommands(null);
            }
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            this.Presenter<PlaylistsViewPresenter>().ItemClick(e.ClickedItem as PlaylistBindingModel);
        }

        private void ListViewOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Presenter<PlaylistsViewPresenter>().BindingModel.IsEditable)
            {
                var uiElements = new List<UIElement>() { this.addPlaylistButton };

                if (this.ListView.SelectedIndex >= 0)
                {
                    uiElements.Add(this.separator);
                    uiElements.Add(this.editPlaylistButton);
                    uiElements.Add(this.deletePlaylistButton);
                }

                var currentContextCommands = App.Container.Resolve<ICurrentContextCommands>();
                currentContextCommands.SetCommands(uiElements);
            }
        }
    }
}
