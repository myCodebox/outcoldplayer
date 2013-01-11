// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System.Net.Http;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Web;

    public class HttpContentExtensionsSuites
    {
        [Test]
        public async void ReadAsDictionaryAsync_SimpleContent_AllValuesRead()
        {
            // Arrange
            HttpContent content = new StringContent(
@"SID=DQAAAGgA7Zg8CTN
LSID=DQAAAGsAlk8BBbG
Auth=DQAAAGgAdk3fA5N");

            // Act
            var dictionary = await content.ReadAsDictionaryAsync();

            // Assert
            Assert.AreEqual("DQAAAGgA7Zg8CTN", dictionary["SID"]);
            Assert.AreEqual("DQAAAGsAlk8BBbG", dictionary["LSID"]);
            Assert.AreEqual("DQAAAGgAdk3fA5N", dictionary["Auth"]);
        }
    }
}