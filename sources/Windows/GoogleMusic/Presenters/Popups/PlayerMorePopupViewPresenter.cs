// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;

    using OutcoldSolutions.GoogleMusic.BindingModels.Popups;
    using OutcoldSolutions.GoogleMusic.Views.Popups;

    public class PlayerMorePopupViewPresenter : DisposableViewPresenterBase<IPlayerMorePopupView>, IDisposable
    {
        public PlayerMorePopupViewPresenter(
            PlayerMorePopupViewBindingModel bindingModel)
        {
            this.BindingModel = bindingModel;
        }

        public PlayerMorePopupViewBindingModel BindingModel { get; private set; }
    }
}