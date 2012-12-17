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

            this.authentificationService.CheckAuthentificationAsync().ContinueWith(
               task =>
                   {
                       if (task.Result.Succeed)
                       {
                           this.NavigateTo<IStartView>();
                       }
                       else
                       {
                           this.ShowView<IAuthentificationView>().Succeed += this.AuthentificationViewOnSucceed;
                       }
                   },
               TaskScheduler.FromCurrentSynchronizationContext());
        }

        public MainViewBindingModel BindingModel { get; private set; }

        public void NavigateTo<TView>(object parameter = null) where TView : IView
        {
            if (this.viewsHistory.Count > 0)
            {
                var value = this.viewsHistory.Last.Value;
                if (object.Equals(value.Parameter, parameter)
                    && typeof(TView) == value.View.GetType())
                {
                    this.Logger.Warning("Double click found. Ignoring...");
                    return;
                }
            }

            var view = this.ShowView<TView>();
            view.OnNavigatedTo(parameter);

            var historyItem = new HistoryItem(view, parameter);
            this.viewsHistory.AddLast(historyItem);

            this.UpdateCanGoBack();
        }

        public void GoBack()
        {
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
            this.View.HideView();
            ((IAuthentificationView)sender).Succeed -= this.AuthentificationViewOnSucceed;

            this.NavigateTo<IStartView>();
        }

        private TView ShowView<TView>() where TView : IView
        {
            return (TView)this.ShowView(this.container.Resolve<TView>());
        }

        private object ShowView(IView view)
        {
            this.View.HideView();
            this.BindingModel.Message = null;
            this.BindingModel.IsProgressRingActive = false;
            this.View.ShowView(view);
            return view;
        }

        private class HistoryItem
        {
            public HistoryItem(IView view, object parameter)
            {
                this.View = view;
                this.Parameter = parameter;
            }

            public IView View { get; private set; }

            public object Parameter { get; private set; }
        }
    }
}