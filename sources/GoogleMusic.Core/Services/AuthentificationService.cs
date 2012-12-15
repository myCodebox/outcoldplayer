// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.WebServices;

    public class AuthentificationService
    {
        private readonly ILogManager logManager;
        private readonly IUserDataStorage userDataStorage;
        private readonly IClientLoginService clientLoginService;

        public AuthentificationService(
            ILogManager logManager,
            IUserDataStorage userDataStorage,
            IClientLoginService clientLoginService)
        {
            this.logManager = logManager;
            this.userDataStorage = userDataStorage;
            this.clientLoginService = clientLoginService;
        }

        public event EventHandler Failed;

        public Task<bool> CheckAuthentification()
        {
            
        }


    }
}
