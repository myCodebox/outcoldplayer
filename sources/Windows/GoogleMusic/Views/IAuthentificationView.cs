// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    public interface IAuthentificationView : IView
    {
        void ShowError(string errorMessage);

        void ShowCaptcha(string captchaUrl);
    }
}