// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Controls
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public class ButtonWithHover : Button
    {
        public ButtonWithHover()
        {
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            VisualStateManager.GoToState(this, "Pressed", false);
        }

        protected override void OnPointerCanceled(PointerRoutedEventArgs e)
        {
            base.OnPointerCanceled(e);

            VisualStateManager.GoToState(this, "Normal", false);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            VisualStateManager.GoToState(this, "Normal", false);
            if (this.IsPointerOver)
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
            }
            if (!this.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);

            VisualStateManager.GoToState(this, "PointerOver", false);

            if (!this.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            VisualStateManager.GoToState(this, "Normal", false);

            if (!this.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }
        }
    }
}
