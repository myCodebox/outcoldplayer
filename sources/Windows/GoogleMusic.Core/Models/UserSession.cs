// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;

    public class UserSession
    {
        public UserSession()
        {
            this.SessionId = this.GenerateSessionId();
        }

        public string SessionId { get; private set; }

        private string GenerateSessionId()
        {
            char[] s = new char[12];
            var random = new Random();

            for (int i = 0; i < s.Length; i++)
            {
                if (random.Next(1, 4) == 1)
                {
                    s[i] = Convert.ToChar(random.Next(0x30, 0x39)); // 0 - 9
                }
                else
                {
                    s[i] = Convert.ToChar(random.Next(0x61, 0x7A)); // a - z
                }
            }

            return new string(s);
        }
    }
}