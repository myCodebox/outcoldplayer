// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Diagnostics;

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

        private TBindingModel bindingModel;

        private bool isDataLoading;

        protected PagePresenterBase(IDependencyResolverContainer container)
            : base(container)
        {
            this.container = container;
            this.Toolbar = container.Resolve<IApplicationToolbar>();
        }

        public bool IsDataLoading
        {
            get
            {
                return this.isDataLoading;
            }

            protected set
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

        protected IApplicationToolbar Toolbar { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            base.OnNavigatedTo(parameter);

            this.IsDataLoading = true;
            this.View.OnDataLoading(parameter);
            this.BindingModel.FreezeNotifications();

            this.Logger.LogTask(Task.Factory.StartNew(
                async () =>
                    {
                        await this.LoadDataAsync(parameter);

                        await this.Dispatcher.RunAsync(
                            () =>
                                {
                                    this.Toolbar.SetViewCommands(this.GetViewCommands());
                                    this.View.OnUnfreeze(parameter);
                                    this.BindingModel.UnfreezeNotifications();
                                    this.IsDataLoading = false;
                                });
                        
                        await Task.Delay(10);
                        await this.Dispatcher.RunAsync(() => this.View.OnDataLoaded(parameter));
                    }));
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel = this.container.Resolve<TBindingModel>();
        }

        protected abstract Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs);

        protected virtual IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield break;
        }
    }
}