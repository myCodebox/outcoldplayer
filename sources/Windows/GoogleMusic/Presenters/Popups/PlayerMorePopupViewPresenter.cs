// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels.Popups;
    using OutcoldSolutions.GoogleMusic.Shell;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class PlayerMorePopupViewPresenter : DisposableViewPresenterBase<IPlayerMorePopupView>, IDisposable
    {
        private readonly IApplicationSettingViewsService applicationSettingViewsService;

        public PlayerMorePopupViewPresenter(
            IApplicationSettingViewsService applicationSettingViewsService,
            PlayerMorePopupViewBindingModel bindingModel)
        {
            this.applicationSettingViewsService = applicationSettingViewsService;

            this.BindingModel = bindingModel;
            
            this.ShowApplicationSettingsCommand = new DelegateCommand(async () =>
                {
                    await this.Dispatcher.RunAsync(
                        () => this.applicationSettingViewsService.Show());
                });
        }

        public PlayerMorePopupViewBindingModel BindingModel { get; private set; }

        public DelegateCommand ShowApplicationSettingsCommand { get; set; }

    }
}