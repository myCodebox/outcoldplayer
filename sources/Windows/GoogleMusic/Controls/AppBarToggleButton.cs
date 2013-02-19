// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls.Primitives;

    public class AppBarToggleButton : ToggleButton
    {
        public AppBarToggleButton()
        {
            this.Click += (sender, args) => this.UpdateState();
            this.LayoutUpdated += (sender, o) => this.UpdateState();
        }

        private void UpdateState()
        {
            VisualStateManager.GoToState(
                this, this.IsChecked.HasValue && this.IsChecked.Value ? "Checked" : "Unchecked", false);
        }
    }
}