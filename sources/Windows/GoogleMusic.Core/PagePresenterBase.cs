// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;

    using Windows.UI.Core;

    public class PagePresenterBase<TView> : ViewPresenterBase<TView>, IPagePresenterBase
        where TView : IPageView
    {
        public PagePresenterBase(
            IDependencyResolverContainer container)
            : base(container)
        {
        }

        public virtual void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
        }

        public virtual void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Class with the same name.")]
    public abstract class PagePresenterBase<TView, TBindingModel> : PagePresenterBase<TView>
        where TView : IDataPageView 
        where TBindingModel : BindingModelBase
    {
        private readonly IDependencyResolverContainer container;
        private readonly IApplicationToolbar toolBar;

        private TBindingModel bindingModel;

        private bool isDataLoading;

        protected PagePresenterBase(IDependencyResolverContainer container)
            : base(container)
        {
            this.container = container;
            this.toolBar = container.Resolve<IApplicationToolbar>();
        }

        public bool IsDataLoading
        {
            get
            {
                return this.isDataLoading;
            }

            private set
            {
                this.isDataLoading = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public TBindingModel BindingModel
        {
            get
            {
                return this.bindingModel;
            }

            private set
            {
                this.bindingModel = value;
                this.RaiseCurrentPropertyChanged();
            }
        }

        public override void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            base.OnNavigatedTo(parameter);

            this.IsDataLoading = true;
            this.View.OnDataLoading(parameter);
            this.BindingModel.FreezeNotifications();

            Task.Factory.StartNew(() => this.LoadData(parameter)).ContinueWith(
                    async t =>
                    {
                        if (t.IsCanceled)
                        {
                            this.Logger.Warning("Task is cancelled.");
                        }
                        else if (t.IsFaulted)
                        {
                            this.Logger.Error("Task is fauled");
                            this.Logger.LogErrorException(t.Exception);
                        }
                        else
                        {
                            this.Logger.Debug("Data loaded.");
                            await this.Dispatcher.RunAsync(() => this.toolBar.SetViewCommands(this.GetViewCommands()));
                        }

                        // TODO: We need to show some error message here if error happens
                        await this.Dispatcher.RunAsync(() =>
                            { 
                                this.BindingModel.UnfreezeNotifications();
                                this.IsDataLoading = false;
                            });
                        // TODO: We need to find better way to do this
                        await Task.Delay(100);
                        await this.Dispatcher.RunAsync(() => this.View.OnDataLoaded(parameter));
                    });
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel = this.container.Resolve<TBindingModel>();
        }

        protected abstract void LoadData(NavigatedToEventArgs navigatedToEventArgs);

        protected virtual IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield break;
        }
    }
}