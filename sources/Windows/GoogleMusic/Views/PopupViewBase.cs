// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldmansolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Diagnostics;

    using Windows.UI.Xaml.Controls.Primitives;

    /// <summary>
    /// The popup view base.
    /// </summary>
    public class PopupViewBase : ViewBase, IPopupView
    {
        /// <inheritdoc />
        public event EventHandler<EventArgs> Closed;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.Loaded += (sender, args) =>
                {
                    var popup = this.Parent as Popup;
                    Debug.Assert(popup != null, "popup != null");
                    if (popup != null)
                    {
                        popup.Closed += this.PopupOnClosed;
                    }
                };
        }

        /// <inheritdoc />
        public void Close()
        {
            this.Close(EventArgs.Empty);
        }

        /// <inheritdoc />
        public void Close(EventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException("eventArgs");
            }

            var popup = this.Parent as Popup;
            Debug.Assert(popup != null, "popup != null");
            if (popup != null && popup.IsOpen)
            {
                popup.IsOpen = false;
                this.RaiseClosed(eventArgs);
            }
        }

        private void PopupOnClosed(object sender, object o)
        {
            var popup = (Popup)sender;
            popup.Closed -= this.PopupOnClosed;
            this.RaiseClosed(EventArgs.Empty);
        }

        private void RaiseClosed(EventArgs eventArgs)
        {
            var handler = this.Closed;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }
    }
}