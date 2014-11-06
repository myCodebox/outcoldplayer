// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;
    using System.Diagnostics;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;

    /// <summary>
    /// The app bar toggle button.
    /// </summary>
    /// <remarks>
    /// This class just fixes the Visual State "Checked" / "Unchecked" for layout update.
    /// </remarks>
    public class AppBarToggleButton : ToggleButton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppBarToggleButton"/> class.
        /// </summary>
        public AppBarToggleButton()
        {
        }

        protected override void OnToggle()
        {
            base.OnToggle();

            VisualStateManager.GoToState(this, this.IsChecked.HasValue && this.IsChecked.Value ? "Checked" : "Unchecked", false);
            if (this.IsPointerOver)
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
            }
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
            if (this.IsChecked.HasValue && this.IsChecked.Value)
            {
                VisualStateManager.GoToState(this, "Checked", false);
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);

            VisualStateManager.GoToState(this, "Normal", false);
            if (this.IsPointerOver)
            {
                VisualStateManager.GoToState(this, "PointerOver", false);
            }
            if (this.IsChecked.HasValue && this.IsChecked.Value)
            {
                VisualStateManager.GoToState(this, "Checked", false);
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
            if (this.IsChecked.HasValue && this.IsChecked.Value)
            {
                VisualStateManager.GoToState(this, "Checked", false);
            }

            if (!this.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            VisualStateManager.GoToState(this, "Normal", false);
            if (this.IsChecked.HasValue && this.IsChecked.Value)
            {
                VisualStateManager.GoToState(this, "Checked", false);
            }

            if (!this.IsEnabled)
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }
        }
    }
}