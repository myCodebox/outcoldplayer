// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites
{
    using System;

    using NUnit.Framework;
    
    public class JsonTest
    {
        [Test]
        public void TestSessionId()
        {
            // var fromMilliseconds = TimeSpan.FromMilliseconds(1355715674069770 / 1000);

            DateTime dateTime = new DateTime(1970, 1, 1);
            var addSeconds = dateTime.AddMilliseconds(1353472689288941 / 1000).ToLocalTime();
        }
    }
}
