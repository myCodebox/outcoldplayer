// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels.Popups;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;

    public class PlayerMorePopupViewPresenter : DisposableViewPresenterBase<IPlayerMorePopupView>, IDisposable
    {
        private readonly IPlayQueueService queueService;

        public PlayerMorePopupViewPresenter(
            IPlayQueueService queueService,
            PlayerMorePopupViewBindingModel bindingModel)
        {
            this.queueService = queueService;

            this.BindingModel = bindingModel;
            this.BindingModel.IsRepeatAllEnabled = this.queueService.IsRepeatAll;
            this.BindingModel.IsShuffleEnabled = this.queueService.IsShuffled;

            this.RepeatAllCommand = new DelegateCommand(async () => await this.queueService.SetRepeatAllAsync(!this.queueService.IsRepeatAll));
            this.ShuffleCommand = new DelegateCommand(async () => await this.queueService.SetShuffledAsync(!this.queueService.IsShuffled));
        }

        public PlayerMorePopupViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ShuffleCommand { get; set; }

        public DelegateCommand RepeatAllCommand { get; set; }

        public DelegateCommand LockScreenCommand { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.RegisterForDispose(this.EventAggregator.GetEvent<QueueChangeEvent>().Subscribe(
                async (e) =>
                    {
                        await this.Dispatcher.RunAsync(
                            () =>
                                {
                                    this.BindingModel.IsRepeatAllEnabled = e.IsRepeatAllEnabled;
                                    this.BindingModel.IsShuffleEnabled = e.IsShuffleEnabled;
                                });
                    }));
        }
    }
}