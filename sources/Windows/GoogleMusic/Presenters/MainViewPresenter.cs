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

        private readonly LinkedList<HistoryItem> viewsHistory = new LinkedList<HistoryItem>();

        public MainViewPresenter(
            IDependencyResolverContainer container, 
            IMainView view,
            IAuthentificationService authentificationService)
            : base(container, view)
        {
            this.container = container;
            this.authentificationService = authentificationService;
            this.BindingModel = new MainViewBindingModel
                                    {
                                        Message = "Signing in...", 
                                        IsProgressRingActive = true
                                    };

            this.Logger.Debug("Checking authentification.");
            this.authentificationService.CheckAuthentificationAsync().ContinueWith(
               task =>
                   {
                       if (task.Result.Succeed)
                       {
                           this.BindingModel.IsAuthenticated = true;
                           this.Logger.Debug("User is logged in. Going to start view and showing player.");
                           this.NavigateTo<IStartView>();
                       }
                       else
                       {
                           this.Logger.Debug("User is not logged in. Showing authentification view.");
                           this.ShowView<IAuthentificationView>().Succeed += this.AuthentificationViewOnSucceed;
                       }
                   },
               TaskScheduler.FromCurrentSynchronizationContext());

            this.PlayerViewPresenter = this.container.Resolve<PlayerViewPresenter>(new object[] { view });
        }

        public MainViewBindingModel BindingModel { get; private set; }

        public PlayerViewPresenter PlayerViewPresenter { get; private set; }

        public void NavigateTo<TView>(object parameter = null) where TView : IView
        {
            var viewType = typeof(TView);
            this.Logger.Debug("Navigating to {0}. Parameter {1}.", viewType, parameter);

            if (this.viewsHistory.Count > 0)
            {
                var value = this.viewsHistory.Last.Value;
                if (object.Equals(value.Parameter, parameter)
                    && value.ViewType == viewType)
                {
                    this.Logger.Warning("Double click found. Ignoring...");
                    return;
                }
            }

            var view = this.ShowView<TView>();
            view.OnNavigatedTo(parameter);

            var historyItem = new HistoryItem(view, viewType, parameter);
            this.viewsHistory.AddLast(historyItem);

            this.UpdateCanGoBack();
        }

        public void GoBack()
        {
            this.Logger.Debug("Going back");

            if (this.CanGoBack())
            {
                this.viewsHistory.RemoveLast();
                var item = this.viewsHistory.Last.Value;

                this.ShowView(item.View);
                item.View.OnNavigatedTo(item.Parameter);

                this.UpdateCanGoBack();
            }
        }

        public bool CanGoBack()
        {
            return this.viewsHistory.Count > 1;
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
            this.NavigateTo<IStartView>();
        }

        private TView ShowView<TView>() where TView : IView
        {
            this.Logger.Debug("Showing view {0}. Creating.", typeof(TView));
            return (TView)this.ShowView(this.container.Resolve<TView>());
        }

        private object ShowView(IView view)
        {
            this.Logger.Debug("Showing view {0}. Instance.", view.GetType());

            this.View.HideView();
            this.BindingModel.Message = null;
            this.BindingModel.IsProgressRingActive = false;
            this.View.ShowView(view);
            return view;
        }

        private class HistoryItem
        {
            public HistoryItem(IView view, Type viewType, object parameter)
            {
                this.View = view;
                this.ViewType = viewType;
                this.Parameter = parameter;
            }

            public IView View { get; private set; }

            public Type ViewType { get; private set; }

            public object Parameter { get; private set; }
        }
    }
}