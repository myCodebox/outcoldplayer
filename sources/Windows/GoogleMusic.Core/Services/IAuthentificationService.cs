﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface IAuthentificationService
    {
        Task<AuthentificationService.AuthentificationResult> CheckAuthentificationAsync(UserInfo userInfo = null, string captchaToken = null, string captcha = null, bool forceCaptcha = false);
    }
}