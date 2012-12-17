namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IAuthentificationService
    {
        Task<AuthentificationService.AuthentificationResult> CheckAuthentificationAsync(UserInfo userInfo = null);
    }
}