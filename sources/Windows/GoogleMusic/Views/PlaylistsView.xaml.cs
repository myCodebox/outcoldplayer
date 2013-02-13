// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Automation;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Input;

    public interface IPlaylistsView : IPageView
    {
        void EditPlaylist(PlaylistBindingModel selectedItem);

        void SetGroups(List<PlaylistsGroupBindingModel> playlistsGroupBindingModels);

        void ShowPlaylist(PlaylistBindingModel playlistBindingModel);

        double GetHorizontalOffset();

        void ScrollToHorizontalOffset(double horizontalOffset);
    }

    public sealed partial class PlaylistsPageView : PageViewBase, IPlaylistsView
    {
        private readonly Button addPlaylistButton;
        private readonly Button editPlaylistButton;
        private readonly Button deletePlaylistButton;
        private readonly Border separator;

        private readonly PlaylistsViewPresenter presenter;

        public PlaylistsPageView()
        {
            this.presenter = this.InitializePresenter<PlaylistsViewPresenter>();
            this.InitializeComponent();

            this.addPlaylistButton = new Button()
                                         {
                                             Style = (Style)Application.Current.Resources["AddAppBarButtonStyle"],
                                             Command = this.presenter.AddPlaylistCommand
                                         };

            this.editPlaylistButton = new Button()
                                         {
                                             Style = (Style)Application.Current.Resources["EditAppBarButtonStyle"],
                                             Command = this.presenter.EditPlaylistCommand
                                         };
            this.editPlaylistButton.SetValue(AutomationProperties.NameProperty, "Rename");

            this.deletePlaylistButton = new Button()
                                         {
                                             Style = (Style)Application.Current.Resources["DeleteAppBarButtonStyle"],
                                             Command = this.presenter.DeletePlaylistCommand
                                         };

            this.separator = new Border() { Style = (Style)Application.Current.Resources["AppBarSeparator"] };
        }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            this.ListView.SelectedIndex = -1;
            if (this.ListView.Items != null && this.ListView.Items.Count > 0)
            {
                this.ScrollToHorizontalOffset(0d);
            }

            base.OnNavigatedTo(eventArgs);

            var currentContextCommands = this.Container.Resolve<ICurrentContextCommands>();
            
            if (eventArgs.Parameter is PlaylistsRequest && ((PlaylistsRequest)eventArgs.Parameter) == PlaylistsRequest.Playlists)
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

        public void EditPlaylist(PlaylistBindingModel selectedItem)
        {
            this.Container.Resolve<ISearchService>().SetShowOnKeyboardInput(false);
            this.PlaylistNamePopup.VerticalOffset = this.ActualHeight - 240;
            this.TextBoxPlaylistName.Text = selectedItem.Playlist.Title;
            this.SaveNameButton.IsEnabled = !string.IsNullOrEmpty(this.TextBoxPlaylistName.Text);
            this.PlaylistNamePopup.IsOpen = true;
            this.TextBoxPlaylistName.Focus(FocusState.Keyboard);
        }

        public void SetGroups(List<PlaylistsGroupBindingModel> playlistsGroupBindingModels)
        {
            this.Groups.Source = playlistsGroupBindingModels;
            if (Groups.View == null)
            {
                ((ListViewBase)SemanticZoom.ZoomedOutView).ItemsSource = null;
            }
            else
            {
                ((ListViewBase)SemanticZoom.ZoomedOutView).ItemsSource = Groups.View.CollectionGroups;
            }
            
            this.ListView.SelectedIndex = -1;
            this.SemanticZoom.IsZoomedInViewActive = true;
        }

        public void ShowPlaylist(PlaylistBindingModel playlistBindingModel)
        {
            if (playlistBindingModel != null)
            {
                this.ListView.ScrollIntoView(playlistBindingModel);
            }
        }

        public double GetHorizontalOffset()
        {
            return VisualTreeHelperEx.GetVisualChild<ScrollViewer>(this.ListView).HorizontalOffset;
        }

        public void ScrollToHorizontalOffset(double horizontalOffset)
        {
            var scrollViewer = VisualTreeHelperEx.GetVisualChild<ScrollViewer>(this.ListView);
            scrollViewer.ScrollToHorizontalOffset(horizontalOffset);
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            this.presenter.ItemClick(e.ClickedItem as PlaylistBindingModel);
        }

        private void ListViewOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var uiElements = new List<UIElement>();
            var currentContextCommands = this.Container.Resolve<ICurrentContextCommands>();
            if (this.presenter.BindingModel.IsEditable)
            {
                uiElements.Add(this.addPlaylistButton);

                if (this.ListView.SelectedIndex >= 0)
                {
                    uiElements.Add(this.separator);
                    uiElements.Add(this.editPlaylistButton);
                    uiElements.Add(this.deletePlaylistButton);
                }
            }

            currentContextCommands.SetCommands(uiElements);
        }

        private void SaveNameClick(object sender, RoutedEventArgs e)
        {
            this.presenter.ChangePlaylistName(this.TextBoxPlaylistName.Text);
            this.PlaylistNamePopup.IsOpen = false;
        }

        private void CancelChangeNameClick(object sender, RoutedEventArgs e)
        {
            this.PlaylistNamePopup.IsOpen = false;
        }

        private void TextBoxPlaylistNameKeyUp(object sender, KeyRoutedEventArgs e)
        {
            this.SaveNameButton.IsEnabled = !string.IsNullOrEmpty(this.TextBoxPlaylistName.Text);
        }

        private void TextBoxPlaylistNameOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.SaveNameClick(sender, e);
                e.Handled = true;
            }
        }

        private void PlaylistNamePopupOnClosed(object sender, object e)
        {
            this.Container.Resolve<ISearchService>().SetShowOnKeyboardInput(true);
        }

        private void ZoomOutClick(object sender, RoutedEventArgs e)
        {
            this.SemanticZoom.IsZoomedInViewActive = false;
        }
        
        //private void SemanticZoom_OnViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        //{
        //    if (e.IsSourceZoomedInView)
        //    {
        //        e.DestinationItem.Item = ((List<PlaylistsGroupBindingModel>)this.ListViewGroups.ItemsSource)
        //            .FirstOrDefault(x => x.Playlists.Contains(e.SourceItem.Item));
        //    }
        //    else
        //    {
        //        e.DestinationItem.Item = ((PlaylistsGroupBindingModel)e.SourceItem.Item).Playlists.FirstOrDefault();
        //    }
        //}
    }
}
