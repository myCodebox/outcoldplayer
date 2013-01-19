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
            var addSeconds = dateTime.AddMilliseconds(1358228284699000 / 1000).ToLocalTime();
            // 1355715674069770l
            // 1355715674069770
            // 1353471887402476
            var time = new DateTime(1970, 1, 1);
            time = time.AddMilliseconds(1355715674069);
        }
    }
}
