// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System;

    public interface IAuthentificationView : IView
    {
        event EventHandler Succeed;

        void ShowCaptcha(string captchaUrl);
    }
}