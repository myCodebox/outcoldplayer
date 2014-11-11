// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    public class UserInfo
    {
        public UserInfo(string email, string password)
        {
            this.Email = email;
            this.Password = password;
        }

        public string Email { get; private set; }

        public string Password { get; private set; }

        public bool RememberAccount { get; set; }
    }
}