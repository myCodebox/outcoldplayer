//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;

    using Windows.UI.Xaml.Controls;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Presenters;

    public sealed partial class MainFrame : Page, IMainFrame, IMainFrameRegionProvider
    {
        private IDependencyResolverContainer container;
        private MainFramePresenter presenter;
        private ILogger logger;

        private IView currentView;

        private IAnalyticsService analyticsService;

        public MainFrame()
        {
            this.InitializeComponent();
        }

        public TPresenter GetPresenter<TPresenter>()
        {
            return (TPresenter)(object)this.presenter;
        }

        public string Title
        {
            set
            {
                this.presenter.Title = value;
            }

            get
            {
                return this.presenter.Title;
            }
        }

        public string Subtitle
        {
            set
            {
                this.presenter.Subtitle = value;
            }

            get
            {
                return this.presenter.Subtitle;
            }
        }

        public bool IsCurretView(IPageView view)
        {
            return this.currentView == view;
        }

        public void SetViewCommands(IEnumerable<CommandMetadata> commands)
        {
        }

        public void ClearViewCommands()
        {
        }

        public void SetContextCommands(IEnumerable<CommandMetadata> commands)
        {
        }

        public void ClearContextCommands()
        {
        }

        public TPopup ShowPopup<TPopup>(PopupRegion popupRegion, params object[] injections) where TPopup : IPopupView
        {
            return default(TPopup);
        }

        public void ShowMessage(string text)
        {
        }

        public void SetContent(MainFrameRegion region, object content)
        {
        }

        public void SetContent<TView>(MainFrameRegion region, params object[] injections)
        {
        }

        public void SetVisibility(MainFrameRegion region, bool isVisible)
        {
        }

        [Inject]
        internal void Initialize(
            IDependencyResolverContainer containerObject,
            ILogManager logManager,
            IAnalyticsService analyticsService,
            MainFramePresenter presenterObject)
        {
            this.container = containerObject;
            this.presenter = presenterObject;
            this.logger = logManager.CreateLogger("MainFrame");
            this.DataContext = this.presenter;
            this.analyticsService = analyticsService;
        }
    }
}
