//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface IStartView : IPageView
    {
        void SetGroups(List<PlaylistsGroupBindingModel> groups);
    }

    public sealed partial class StartPageView : PageViewBase, IStartView
    {
        private StartViewPresenter presenter;

        public StartPageView()
        {
            this.InitializeComponent();
        }

        public void SetGroups(List<PlaylistsGroupBindingModel> groups)
        {
            this.Groups.Source = groups;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<StartViewPresenter>();
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            this.presenter.ItemClick(e.ClickedItem as PlaylistBindingModel);
        }

        private void NavigateTo(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var groupBindingModel = frameworkElement.DataContext as PlaylistsGroupBindingModel;
                if (groupBindingModel != null)
                {
                    this.NavigationService.NavigateTo<IPlaylistsView>(groupBindingModel.Request);
                }
            }
        }
    }
}
