// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    public sealed partial class SituationStationsPageView : PageViewBase, ISituationStationsPageView
    {
        private IPlaylistsListView playlistsListView;

        public SituationStationsPageView()
        {
            this.InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var presenter = this.GetPresenter<BindingModelBase>();

            this.PlaylistsContentPresenter.Content = (this.playlistsListView = this.Container.Resolve<IPlaylistsListView>()) as PlaylistsListView;

            var listView = this.playlistsListView as PlaylistsListView;
            if (listView != null)
            {
                listView.ItemClicked += ListViewOnItemClicked;
                listView.SetBinding(
                    PlaylistsListView.ItemsSourceProperty,
                    new Binding()
                    {
                        Source = presenter,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath("BindingModel.Playlists")
                    });
            }

            this.TrackScrollViewer(this);
        }

        private void ListViewOnItemClicked(object sender, ItemClickEventArgs e)
        {
            var playlistBindingModel = e.ClickedItem as PlaylistBindingModel;
            if (playlistBindingModel != null)
            {
                var situationRadio = playlistBindingModel.Playlist as SituationRadio;
                if (situationRadio != null)
                {
                    this.GetPresenter<SituationStationsPageViewPresenter>().NavigateToRadio(situationRadio);
                }
            }
        }
    }
}
