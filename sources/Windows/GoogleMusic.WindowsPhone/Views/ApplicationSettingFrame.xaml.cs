// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    using OutcoldSolutions.GoogleMusic.Presenters;

    public sealed partial class ApplicationSettingFrame : PopupViewBase, IApplicationSettingFrame, IDisposable
    {
        private ApplicationSettingFramePresenter presenter;

        public ApplicationSettingFrame()
        {
            this.InitializeComponent();
        }

        ~ApplicationSettingFrame()
        {
            this.Dispose(disposing: false);
        }

        public void SetContent(string title, object content)
        {
            this.presenter.SetContent(title, content);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.presenter = this.GetPresenter<ApplicationSettingFramePresenter>();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.presenter.DisposeIfDisposable();
            }
        }
    }
}
