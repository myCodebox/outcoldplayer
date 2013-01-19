// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class MainViewPresenter : ViewPresenterBase<IMainView>, INavigationService
    {
        private readonly IDependencyResolverContainer container;
        private readonly IAuthentificationService authentificationService;
        private readonly IGoogleMusicSessionService sessionService;

        private readonly LinkedList<HistoryItem> viewsHistory = new LinkedList<HistoryItem>();

        public MainViewPresenter(
            IDependencyResolverContainer container, 
            IMainView view,
            IAuthentificationService authentificationService,
            IGoogleMusicSessionService sessionService)
            : base(container, view)
        {
            this.container = container;
            this.authentificationService = authentificationService;
            this.sessionService = sessionService;
            this.BindingModel = new MainViewBindingModel
                                    {
                                        Message = "Signing in...", 
                                        IsProgressRingActive = true
                                    };

            this.Logger.Debug("Checking authentification.");
            this.authentificationService.CheckAuthentificationAsync().ContinueWith(
               task =>
                   {
                       if (task.IsCompleted && !task.IsFaulted && task.Result.Succeed)
                       {
                           this.BindingModel.IsAuthenticated = true;
                           this.Logger.Debug("User is logged in. Going to start view and showing player.");
                           this.NavigateTo<IProgressLoadingView>(keepInHistory: false);
                       }
                       else
                       {
                           this.Logger.Debug("User is not logged in. Showing authentification view.");
                           this.NavigateTo<IAuthentificationView>(keepInHistory: false).Succeed += this.AuthentificationViewOnSucceed;
                       }
                   },
               TaskScheduler.FromCurrentSynchronizationContext());

            this.PlayerViewPresenter = this.container.Resolve<PlayerViewPresenter>(new object[] { view });

            this.sessionService.SessionCleared += (sender, args) => this.Dispatcher.RunAsync(
                () =>
                    {
                        App.Container.Resolve<ISearchService>().Unregister();
                        if (this.BindingModel.IsAuthenticated)
                        {
                            this.BindingModel.IsAuthenticated = false;
                            this.viewsHistory.Clear();
                            this.NavigateTo<IAuthentificationView>(keepInHistory: false).Succeed += this.AuthentificationViewOnSucceed;
                        }
                    });
        }

        public MainViewBindingModel BindingModel { get; private set; }

        public PlayerViewPresenter PlayerViewPresenter { get; private set; }

        public TView NavigateTo<TView>(object parameter = null, bool keepInHistory = true) where TView : IView
        {
            var viewType = typeof(TView);
            this.Logger.Debug("Navigating to {0}. Parameter {1}.", viewType, parameter);

            IView currentView = null;

            if (this.viewsHistory.Count > 0)
            {
                var value = this.viewsHistory.Last.Value;
                if (object.Equals(value.Parameter, parameter)
                    && value.ViewType == viewType)
                {
                    this.Logger.Warning("Double click found. Ignoring...");
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
                this.ShowView(view);
            }
            else
            {
                this.Logger.Debug("View the same: {0}.", typeof(TView));
            }

            view.OnNavigatedTo(new NavigatedToEventArgs(historyItem == null ? null : historyItem.State, parameter, isBack: false));
            this.UpdateCanGoBack();

            return view;
        }

        public void GoBack()
        {
            this.Logger.Debug("Going back");

            if (this.CanGoBack())
            {
                this.viewsHistory.RemoveLast();
                var item = this.viewsHistory.Last.Value;

                this.ShowView(item.View);
                item.View.OnNavigatedTo(new NavigatedToEventArgs(item.State, item.Parameter, isBack: true));

                this.UpdateCanGoBack();
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

        private void UpdateCanGoBack()
        {
            this.BindingModel.CanGoBack = this.CanGoBack();
        }

        private void AuthentificationViewOnSucceed(object sender, EventArgs eventArgs)
        {
            this.Logger.Debug("Authentification view on succed.");

            this.View.HideView();
            ((IAuthentificationView)sender).Succeed -= this.AuthentificationViewOnSucceed;

            this.BindingModel.IsAuthenticated = true;
            this.NavigateTo<IProgressLoadingView>(keepInHistory: false);
        }
        
        private void ShowView(IView view)
        {
            this.Logger.Debug("Showing view {0}. Instance.", view.GetType());

            this.View.HideView();
            this.BindingModel.Message = null;
            this.BindingModel.IsProgressRingActive = false;
            this.View.ShowView(view);
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