// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class MainViewPresenter : ViewPresenterBase<IMainView>
    {
        private readonly IDependencyResolverContainer container;
        private readonly IAuthentificationService authentificationService;


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
                           this.ShowView<IStartView>();
                       }
                       else
                       {
                           this.ShowView<IAuthentificationView>().Succeed += this.AuthentificationViewOnSucceed;
                       }
                   },
               TaskScheduler.FromCurrentSynchronizationContext());
        }

        public MainViewBindingModel BindingModel { get; private set; }

        private void AuthentificationViewOnSucceed(object sender, EventArgs eventArgs)
        {
            this.View.HideView();
            ((IAuthentificationView)sender).Succeed -= this.AuthentificationViewOnSucceed;

            this.ShowView<IStartView>();
        }

        private TView ShowView<TView>() where TView : IView
        {
            this.View.HideView();
            this.BindingModel.Message = null;
            this.BindingModel.IsProgressRingActive = false;
            var view = this.container.Resolve<TView>();
            this.View.ShowView(view);
            return view;
        }
    }
}