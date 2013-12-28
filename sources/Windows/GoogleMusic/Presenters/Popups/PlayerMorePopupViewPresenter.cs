// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels.Popups;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Shell;

    public class PlayerMorePopupViewPresenter : DisposableViewPresenterBase<IPlayerMorePopupView>, IDisposable
    {
        private readonly IPlayQueueService queueService;
        private readonly IApplicationSettingViewsService applicationSettingViewsService;

        public PlayerMorePopupViewPresenter(
            IPlayQueueService queueService,
            IApplicationSettingViewsService applicationSettingViewsService,
            PlayerMorePopupViewBindingModel bindingModel)
        {
            this.queueService = queueService;
            this.applicationSettingViewsService = applicationSettingViewsService;

            this.BindingModel = bindingModel;

            this.RepeatAllCommand = new DelegateCommand(() => { }, () => this.queueService.State != QueueState.Busy && !queueService.IsRadio);
            this.ShuffleCommand = new DelegateCommand(() => { }, () => this.queueService.State != QueueState.Busy && !queueService.IsRadio);
            this.ShowApplicationSettingsCommand = new DelegateCommand(async () =>
                {
                    await this.Dispatcher.RunAsync(
                        () => this.applicationSettingViewsService.Show());
                });

            this.queueService.StateChanged += this.QueueServiceOnStateChanged;
        }

        public PlayerMorePopupViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatAllCommand { get; set; }

        public DelegateCommand ShowApplicationSettingsCommand { get; set; }

        protected override void OnDisposing()
        {
            base.OnDisposing();

            this.BindingModel.Dispose();
            this.queueService.StateChanged -= this.QueueServiceOnStateChanged;
        }

        private void QueueServiceOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            this.ShuffleCommand.RaiseCanExecuteChanged();
            this.RepeatAllCommand.RaiseCanExecuteChanged();
        }
    }
}