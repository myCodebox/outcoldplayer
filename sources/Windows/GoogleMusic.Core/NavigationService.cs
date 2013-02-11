// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using System.Collections.Generic;

    using OutcoldSolutions.Diagnostics;

    public class NavigationService : INavigationService
    {
        private readonly LinkedList<HistoryItem> viewsHistory = new LinkedList<HistoryItem>();

        private readonly ILogger logger;
        private readonly IDependencyResolverContainer container;

        private IViewRegionProvider viewRegionProvider;

        public NavigationService(
            IDependencyResolverContainer container,
            ILogManager logManager)
        {
            this.container = container;
            this.logger = logManager.CreateLogger("NavigationService");
        }

        public event EventHandler<NavigatedToEventArgs> NavigatedTo;

        public void RegisterRegionProvider(IViewRegionProvider regionProvider)
        {
            if (regionProvider == null)
            {
                throw new ArgumentNullException("regionProvider");
            }

            this.viewRegionProvider = regionProvider;
        }

        public TView NavigateTo<TView>(object parameter = null, bool keepInHistory = true) where TView : IView
        {
            if (this.viewRegionProvider == null)
            {
                throw new NotSupportedException("Register region provider first.");
            }

            var viewType = typeof(TView);
            this.logger.Debug("Navigating to {0}. Parameter {1}.", viewType, parameter);

            IView currentView = null;

            if (this.viewsHistory.Count > 0)
            {
                var value = this.viewsHistory.Last.Value;
                if (object.Equals(value.Parameter, parameter)
                    && value.ViewType == viewType)
                {
                    this.logger.Warning("Double click found. Ignoring...");
                    return (TView)value.View;
                }

                currentView = this.viewsHistory.Last.Value.View;

                this.viewsHistory.Last.Value.View.OnNavigatingFrom(new NavigatingFromEventArgs(this.viewsHistory.Last.Value.State));
            }

            var view = this.container.Resolve<TView>();

            HistoryItem historyItem = null;
            if (keepInHistory)
            {
                historyItem = new HistoryItem(view, viewType, parameter);
                this.viewsHistory.AddLast(historyItem);
            }

            if (currentView == null || !currentView.Equals(view))
            {
                this.viewRegionProvider.Show(view);
            }
            else
            {
                this.logger.Debug("View the same: {0}.", typeof(TView));
            }

            var navigatedToEventArgs = new NavigatedToEventArgs(view, historyItem == null ? null : historyItem.State, parameter, isBack: false);
            view.OnNavigatedTo(navigatedToEventArgs);
            this.RaiseNavigatedTo(navigatedToEventArgs);

            return view;
        }

        public void GoBack()
        {
            if (this.viewRegionProvider == null)
            {
                throw new NotSupportedException("Register region provider first.");
            }

            this.logger.Debug("Go back requested");

            if (this.CanGoBack())
            {
                this.viewsHistory.RemoveLast();
                var item = this.viewsHistory.Last.Value;

                this.viewRegionProvider.Show(item.View);
                var navigatedToEventArgs = new NavigatedToEventArgs(item.View, item.State, item.Parameter, isBack: true);
                item.View.OnNavigatedTo(navigatedToEventArgs);
                this.RaiseNavigatedTo(navigatedToEventArgs);
            }
        }

        public bool CanGoBack()
        {
            return this.viewsHistory.Count > 1;
        }

        public bool HasHistory()
        {
            return this.viewsHistory.Count > 0;
        }

        public void ClearHistory()
        {
            this.viewsHistory.Clear();
        }

        private void RaiseNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            var handler = this.NavigatedTo;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        private class HistoryItem
        {
            public HistoryItem(IView view, Type viewType, object parameter)
            {
                this.View = view;
                this.ViewType = viewType;
                this.Parameter = parameter;
                this.State = new Dictionary<string, object>();
            }

            public IView View { get; private set; }

            public Type ViewType { get; private set; }

            public object Parameter { get; private set; }

            public IDictionary<string, object> State { get; private set; }
        }
    }
}