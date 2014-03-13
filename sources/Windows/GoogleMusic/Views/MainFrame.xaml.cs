// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media.Animation;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Services;

    /// <summary>
    /// The MainFrame interface.
    /// </summary>
    public interface IMainFrame : IView
    {
        /// <summary>
        /// Set view commands.
        /// </summary>
        /// <param name="commands">
        /// The commands.
        /// </param>
        void SetViewCommands(IEnumerable<CommandMetadata> commands);

        /// <summary>
        /// The clear view commands.
        /// </summary>
        void ClearViewCommands();

        /// <summary>
        /// Set context commands.
        /// </summary>
        /// <param name="commands">
        /// The commands.
        /// </param>
        void SetContextCommands(IEnumerable<CommandMetadata> commands);

        /// <summary>
        /// Clear context commands.
        /// </summary>
        void ClearContextCommands();

        /// <summary>
        /// Show popup.
        /// </summary>
        /// <param name="popupRegion">
        /// The popup region.
        /// </param>
        /// <param name="injections">
        /// The injections arguments.
        /// </param>
        /// <typeparam name="TPopup">
        /// The type of popup view.
        /// </typeparam>
        /// <returns>
        /// The <see cref="TPopup"/>.
        /// </returns>
        TPopup ShowPopup<TPopup>(PopupRegion popupRegion, params object[] injections) where TPopup : IPopupView;
    }

    /// <summary>
    /// The main frame.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "View/Interface implementation.")]
    public sealed partial class MainFrame : Page, IMainFrame, IMainFrameRegionProvider
    {
        private IDependencyResolverContainer container;
        private MainFramePresenter presenter;
        private ILogger logger;

        private IView currentView;

        private Popup fullScreenPopup;

        private IMainMenu mainMenu;

        private ISelectedObjectsService selectedObjectsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainFrame"/> class.
        /// </summary>
        public MainFrame()
        {
            this.InitializeComponent();

            Debug.Assert(this.BottomAppBar != null, "this.BottomAppBar != null");

            this.BottomAppBar.Closed += (sender, o) =>
                {
                    this.AppToolBarRightPopup.IsOpen = false;
                    this.AppToolBarLeftPopup.IsOpen = false;
                };

            this.SizeChanged += (sender, args) =>
                {
                    this.UpdateFullScreenPopupSize();
                    this.UpdateBottomAppBarVisibility();
                };

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.MainMenuContainer.Content = this.mainMenu = this.container.Resolve<IMainMenu>();
        }

        /// <inheritdoc />
        public void SetViewCommands(IEnumerable<CommandMetadata> commands)
        {
            if (this.mainMenu != null)
            {
                this.mainMenu.GetPresenter<MainMenuPresenter>().ViewCommands.Clear();
                foreach (var commandMetadata in commands)
                {
                    this.mainMenu.GetPresenter<MainMenuPresenter>().ViewCommands.Add(commandMetadata);
                }
            }

            this.UpdateBottomAppBar();
        }

        /// <inheritdoc />
        public void ClearViewCommands()
        {
            if (this.mainMenu != null)
            {
                this.mainMenu.GetPresenter<MainMenuPresenter>().ViewCommands.Clear();
            }

            this.UpdateBottomAppBar();
        }

        /// <inheritdoc />
        public void SetContextCommands(IEnumerable<CommandMetadata> commands)
        {
            var list = commands.ToList();
            int i = 1;
            while (i < list.Count)
            {
                if (list[i - 1].ActionGroup != list[i].ActionGroup)
                {
                    list.Insert(i, new CommandMetadata(null, null, null));
                    i++;
                }

                i++;
            }

            this.ContextButtonsItemsControl.ItemsSource = list;
            this.UpdateBottomAppBar();
            if (this.BottomAppBar.Visibility == Visibility.Visible 
                && !this.BottomAppBar.IsOpen
                && this.ContextButtonsItemsControl.Items != null
                && this.ContextButtonsItemsControl.Items.Count > 0)
            {
                this.BottomAppBar.IsOpen = true;
            }
        }

        /// <inheritdoc />
        public void ClearContextCommands()
        {
            this.ContextButtonsItemsControl.ItemsSource = null;
            this.UpdateBottomAppBar();
        }

        /// <inheritdoc />
        public TPopup ShowPopup<TPopup>(PopupRegion popupRegion, params object[] injections) where TPopup : IPopupView
        {
            TPopup popupView = this.container.Resolve<TPopup>(injections);
            var uiElement = (FrameworkElement)(object)popupView;
            this.ShowPopup(popupRegion, uiElement);
            return popupView;
        }
       
        /// <inheritdoc />
        public TPresenter GetPresenter<TPresenter>()
        {
            return (TPresenter)(object)this.presenter;
        }

        /// <inheritdoc />
        public void SetContent(MainFrameRegion region, object content)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Trying to set {0} to region {1}.", content, region);
            }

            switch (region)
            {
                case MainFrameRegion.Content:
                    this.SetContentRegion(content);
                    break;

                case MainFrameRegion.Right:
                    this.SetRightRegion(content);
                    break;

                case MainFrameRegion.BottomAppBarRightZone:
                    this.SetBottomAppBarRightZoneRegion(content);
                    break;

                case MainFrameRegion.Background:
                    this.SetBackgroundRegion(content);
                    break;

                case MainFrameRegion.Links:
                    this.SetLinksRegion(content);
                    break;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Region {0} is not supported.", region));
            }
        }

        /// <inheritdoc />
        public void SetContent<TView>(MainFrameRegion region, params object[] injections)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Trying to set {0} to region {1}.", typeof(TView), region);
            }

            object content = this.container.Resolve<TView>(injections);

            this.SetContent(region, content);
        }

        /// <inheritdoc />
        public void SetVisibility(MainFrameRegion region, bool isVisible)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Trying to set visibility '{0}' to region {1}.", isVisible, region);
            }

            switch (region)
            {
                case MainFrameRegion.Content:
                    this.ContentControl.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case MainFrameRegion.Right:
                    this.RightRegionContentControl.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case MainFrameRegion.BottomAppBarRightZone:
                    this.BottomAppBarRightZoneRegionContentControl.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case MainFrameRegion.Background:
                    this.BackgroundContentControl.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case MainFrameRegion.Links:
                    this.LinksContentControl.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Region {0} is not supported.", region));
            }
        }

        [Inject]
        internal void Initialize(
            IDependencyResolverContainer containerObject,
            ILogManager logManager,
            MainFramePresenter presenterObject)
        {
            this.container = containerObject;
            this.presenter = presenterObject;
            this.logger = logManager.CreateLogger("MainFrame");
            this.DataContext = this.presenter;
        }

        private void ShowPopup(PopupRegion region, FrameworkElement content)
        {
            switch (region)
            {
                case PopupRegion.AppToolBarRight:
                    this.DisposePopupContent(this.AppToolBarRightPopup);
                    this.AppToolBarRightPopup.VerticalOffset = 0;
                    this.AppToolBarRightPopup.Child = content;
                    this.AppToolBarRightPopup.Width = content.Width;
                    this.AppToolBarRightPopup.Height = content.Height;
                    this.AppToolBarRightPopup.IsOpen = true;
                    break;
                case PopupRegion.AppToolBarLeft:
                    this.DisposePopupContent(this.AppToolBarLeftPopup);
                    this.AppToolBarLeftPopup.VerticalOffset = 0;
                    this.AppToolBarLeftPopup.Child = content;
                    this.AppToolBarLeftPopup.Width = content.Width;
                    this.AppToolBarLeftPopup.Height = content.Height;
                    this.AppToolBarLeftPopup.IsOpen = true;
                    break;
                case PopupRegion.Full:
                    if (this.fullScreenPopup != null)
                    {
                        this.DisposePopupContent(this.fullScreenPopup);
                        this.fullScreenPopup.Closed -= this.FullScreenPopupViewClosed;
                        this.fullScreenPopup = null;
                        ((Storyboard)this.Resources["ActivateFullScreenPopup"]).Stop();
                    }

                    this.fullScreenPopup = new Popup()
                                               {
                                                   HorizontalAlignment = HorizontalAlignment.Stretch,
                                                   VerticalAlignment = VerticalAlignment.Stretch,
                                                   IsLightDismissEnabled = false
                                               };
                    this.fullScreenPopup.Closed += this.FullScreenPopupViewClosed;
                    this.fullScreenPopup.Child = content;
                    this.UpdateFullScreenPopupSize();
                    this.fullScreenPopup.IsOpen = true;
                    this.fullScreenPopup.Opacity = 1.0;
                    this.UpdateBottomAppBarVisibility();
                    Storyboard.SetTarget(((Storyboard)this.Resources["ActivateFullScreenPopup"]), this.fullScreenPopup);
                    ((Storyboard)this.Resources["ActivateFullScreenPopup"]).Begin();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("region");
            }
        }

        private void UpdateFullScreenPopupSize()
        {
            if (this.fullScreenPopup != null)
            {
                this.fullScreenPopup.Width = Window.Current.Bounds.Width;
                this.fullScreenPopup.Height = Window.Current.Bounds.Height;
                var frameworkElement = this.fullScreenPopup.Child as FrameworkElement;
                if (frameworkElement != null)
                {
                    frameworkElement.Height = this.fullScreenPopup.Height;
                    frameworkElement.Width = this.fullScreenPopup.Width;
                }
            }
        }

        private void OnIsDataLoadingChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var view = this.currentView;
            if (view is IPageView)
            {
                var viewPresenter = view.GetPresenter<IPagePresenterBase>();
                this.ProgressRing.IsActive = viewPresenter.IsDataLoading;
                if (!this.ProgressRing.IsActive)
                {
                    ((Storyboard)this.Resources["ActivateContent"]).Begin();
                }
            }
        }

        private void PopupViewClosed(object sender, object e)
        {
            var popup = sender as Popup;
            if (popup != null)
            {
                this.DisposePopupContent(popup);
            }
        }

        private void DisposePopupContent(Popup popup)
        {
            UIElement content = popup.Child;
            popup.Child = null;

            var popupViewBase = content as PopupViewBase;
            if (popupViewBase != null)
            {
                try
                {
                    popupViewBase.GetPresenter<BindingModelBase>().DisposeIfDisposable();
                }
                catch (Exception exp)
                {
                    this.logger.Error(exp, "Exception while tried to dispose presenter for popup view base.");
                }
            }

            try
            {
                content.DisposeIfDisposable();
            }
            catch (Exception exp)
            {
                this.logger.Error(exp, "Exception while tried to dispose content of popup view base.");
            }
        }

        private void FullScreenPopupViewClosed(object sender, object e)
        {
            this.PopupViewClosed(sender, e);
            this.UpdateBottomAppBarVisibility();
        }

        private void SetContentRegion(object content)
        {
            if (this.currentView != null)
            {
                this.currentView.GetPresenter<BindingModelBase>().Unsubscribe("IsDataLoading", this.OnIsDataLoadingChanged);

                try
                {
                    this.currentView.GetPresenter<BindingModelBase>().DisposeIfDisposable();
                }
                catch (Exception exp)
                {
                    this.logger.Error(exp, "Exception while tried to dispose presenter for curren view.");
                }

                try
                {
                    this.currentView.DisposeIfDisposable();
                }
                catch (Exception exp)
                {
                    this.logger.Error(exp, "Exception while tried to dispose current view.");
                }
                
                this.currentView = null;
            }

            this.ClearViewCommands();
            this.ClearContextCommands();

            this.ContentControl.Content = null;
            this.TitleTextBox.ClearValue(TextBlock.TextProperty);
            this.SubtitleTextBox.ClearValue(TextBlock.TextProperty);
            this.TitleGrid.ClearValue(UIElement.VisibilityProperty);

            this.currentView = content as IView;

            var pageView = this.currentView as IPageView;
            if (pageView != null)
            {
                this.TitleTextBox.SetBinding(
                    TextBlock.TextProperty,
                    new Binding()
                    {
                        Source = this.currentView,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(PropertyNameExtractor.GetPropertyName(() => pageView.Title))
                    });

                this.SubtitleTextBox.SetBinding(
                    TextBlock.TextProperty,
                    new Binding()
                    {
                        Source = this.currentView,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(PropertyNameExtractor.GetPropertyName(() => pageView.Subtitle))
                    });

                this.TitleGrid.SetBinding(
                    UIElement.VisibilityProperty,
                    new Binding()
                    {
                        Source = this.currentView,
                        Mode = BindingMode.OneWay,
                        Path = new PropertyPath(PropertyNameExtractor.GetPropertyName(() => pageView.IsTitleVisible)),
                        Converter = (IValueConverter)Application.Current.Resources["BooleanToVisibilityConverter"]
                    });
            }

            var dataPageView = this.currentView as IPageView;
            if (dataPageView != null)
            {
                this.ProgressRing.IsActive = true;
                this.ContentControl.Opacity = 0;
                this.currentView.GetPresenter<BindingModelBase>().Subscribe("IsDataLoading", this.OnIsDataLoadingChanged);
            }

            this.ContentControl.Content = this.currentView;
        }

        private void SetRightRegion(object content)
        {
            this.RightRegionContentControl.Content = content;
        }

        private void SetBackgroundRegion(object content)
        {
            this.BackgroundContentControl.Content = content;
        }

        private void SetLinksRegion(object content)
        {
            this.LinksContentControl.Content = content;
        }

        private void SetBottomAppBarRightZoneRegion(object content)
        {
            this.BottomAppBarRightZoneRegionContentControl.Content = content;
            this.UpdateBottomAppBar();
        }

        private void UpdateBottomAppBar()
        {
            this.UpdateBottomAppBarVisibility();
        }

        private void UpdateBottomAppBarVisibility()
        {
            bool isVisible = (this.ContextButtonsItemsControl.Items != null && this.ContextButtonsItemsControl.Items.Count > 0);
            this.UpdateToolBarVisibility(this.BottomAppBar, isVisible);
        }

        private void UpdateToolBarVisibility(AppBar appBar, bool isLogicalVisible)
        {
            if (appBar != null)
            {
                var currentVisibility = appBar.Visibility == Visibility.Visible && appBar.IsOpen;

                var isVisible = (this.fullScreenPopup == null || !this.fullScreenPopup.IsOpen) && isLogicalVisible;

                appBar.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

                if (!currentVisibility)
                {
                    appBar.IsOpen = false;
                }
            }
        }

        private void MainFrame_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                if (this.selectedObjectsService == null)
                {
                    this.selectedObjectsService = this.container.Resolve<ISelectedObjectsService>();
                }

                if (this.selectedObjectsService.HasSelectedObjects())
                {
                    this.selectedObjectsService.ClearSelection();
                    e.Handled = true;
                }
            }
        }
    }
}
