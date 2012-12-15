//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml.Controls;

    public sealed partial class MainPage : Page
    {
        public MainPage(IDependencyResolverContainer container)
        {
            this.InitializeComponent();

            this.Content.Children.Add(container.Resolve<AuthentificationView>());
        }
    }
}
