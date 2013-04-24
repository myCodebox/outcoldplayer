// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters.Popups
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels.Popups;
    using OutcoldSolutions.GoogleMusic.Services.Publishers;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web.Lastfm;
    using OutcoldSolutions.Presenters;

    using Windows.System;

    public class LastfmAuthentificationPresenter : ViewPresenterBase<ILastfmAuthentificationView>
    {
        private readonly IApplicationResources resources;
        private readonly ILastfmAccountWebService accountWebService;
        private readonly ICurrentSongPublisherService publisherService;

        private string token;

        public LastfmAuthentificationPresenter(
            IApplicationResources resources,
            ILastfmAccountWebService accountWebService,
            ICurrentSongPublisherService publisherService)
        {
            this.resources = resources;
            this.accountWebService = accountWebService;
            this.publisherService = publisherService;
            this.BindingModel = new LastfmAuthentificationBindingModel
                                    {
                                        IsLoading = true,
                                        Message = this.resources.GetString("LastFmAuthentification_Instruction")
                                    };

            this.accountWebService.GetTokenAsync().ContinueWith(
                t =>
                    {
                        if (t.IsCompleted && !t.IsFaulted && string.IsNullOrEmpty(t.Result.Error)
                            && !string.IsNullOrEmpty(t.Result.Token))
                        {
                            this.BindingModel.IsLoading = false;
                            var launchTask = Launcher.LaunchUriAsync(new Uri(this.BindingModel.LinkUrl = this.accountWebService.GetAuthUrl(this.token = t.Result.Token)));
                        }
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public LastfmAuthentificationBindingModel BindingModel { get; private set; }

        public void GetSession()
        {
            this.BindingModel.IsLoading = true;
            this.BindingModel.Message = this.resources.GetString("LastFmAuthentification_Linking");
            this.BindingModel.IsLinkVisible = false;

            this.accountWebService.GetSessionAsync(this.token).ContinueWith(
                (t) =>
                    {
                        if (t.IsCompleted && !t.IsFaulted && t.Result.Session != null)
                        {
                            this.publisherService.AddPublisher<LastFmCurrentSongPublisher>();
                            this.View.Close();
                        }
                        else
                        {
                            this.BindingModel.Message = this.resources.GetString("LastFmAuthentification_Unsuccess");
                            this.BindingModel.IsLinkVisible = true;
                            this.BindingModel.IsLoading = false;
                        }
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}