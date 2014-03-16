// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using Windows.UI.Core;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Views;

    /// <summary>
    /// The page presenter base.
    /// </summary>
    /// <typeparam name="TView">
    /// The type of the view.
    /// </typeparam>
    public abstract class PagePresenterBase<TView> : ViewPresenterBase<TView>, IPagePresenterBase
        where TView : IPageView 
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        private bool isDataLoading;
        private CancellationTokenSource dataLoadingCancellationTokenSource;

        /// <summary>
        /// Gets or sets a value indicating whether is data loading.
        /// </summary>
        public bool IsDataLoading
        {
            get
            {
                return this.isDataLoading;
            }

            protected set
            {
                this.SetValue(ref this.isDataLoading, value);
            }
        }

        /// <inheritdoc />
        public virtual void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            this.IsDataLoading = true;
            this.View.OnDataLoading(parameter);
            this.Freeze();

            CancellationTokenSource source = this.dataLoadingCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = source.Token;

            this.Logger.LogTask(Task.Factory.StartNew(
                async () =>
                {
                    await this.LoadDataAsync(parameter, cancellationToken);

                    await this.Dispatcher.RunAsync(
                        () =>
                        {
                            this.MainFrame.SetViewCommands(this.GetViewCommands());
                            this.View.OnUnfreeze(parameter);
                            this.Unfreeze();
                            this.IsDataLoading = false;
                        });

                    await Task.Delay(1, cancellationToken);
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => this.View.OnDataLoaded(parameter));
                }, cancellationToken));
        }

        /// <inheritdoc />
        public virtual void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            try
            {
                if (this.dataLoadingCancellationTokenSource != null)
                {
                    this.dataLoadingCancellationTokenSource.Cancel();
                    this.dataLoadingCancellationTokenSource = null;
                }
            }
            catch (Exception e)
            {
                this.Logger.Debug(e, "Could not cancel");
            }
        }

        internal virtual void Freeze()
        {
        }

        internal virtual void Unfreeze()
        {
        }

        /// <summary>
        /// The load data async.
        /// </summary>
        /// <param name="navigatedToEventArgs">
        /// The navigated to event args.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        protected abstract Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken);

        /// <summary>
        /// The get view commands.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{IconCommandMetadata}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield break;
        }
    }

    /// <summary>
    /// The page presenter base.
    /// </summary>
    /// <typeparam name="TView">
    /// The type of view.
    /// </typeparam>
    /// <typeparam name="TBindingModel">
    /// The type of binding model.
    /// </typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Same logic.")]
    public abstract class PagePresenterBase<TView, TBindingModel> : PagePresenterBase<TView>, IPagePresenterBase
        where TView : IPageView 
        where TBindingModel : BindingModelBase
    {
        private TBindingModel bindingModel;

        /// <summary>
        /// Gets the binding model.
        /// </summary>
        public TBindingModel BindingModel
        {
            get
            {
                return this.bindingModel;
            }

            private set
            {
                this.SetValue(ref this.bindingModel, value);
            }
        }

        internal override void Freeze()
        {
            base.Freeze();
            this.BindingModel.FreezeNotifications();
        }

        internal override void Unfreeze()
        {
            base.Unfreeze();
            this.BindingModel.UnfreezeNotifications();
        }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel = this.Container.Resolve<TBindingModel>();
        }
    }
}