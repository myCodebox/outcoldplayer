//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml;

    public interface IMainView : IView
    {
        void ShowView(IView view);

        void HideView();
    }

    public sealed partial class MainView : ViewBase, IMainView
    {
        public MainView(
            IDependencyResolverContainer container)
            : base(container)
        {
            this.InitializePresenter<MainViewPresenter>();
            this.InitializeComponent();
        }

        public void ShowView(IView view)
        {
            this.Content.Children.Add((UIElement)view);
        }

        public void HideView()
        {
            this.Content.Children.Clear();
        }
    }
}
