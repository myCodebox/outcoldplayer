// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System.Net.Http.Headers;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Web;

    public class MediaTypeHeaderValueExtensionsSuites
    {
        [Test]
        public void IsPlainText_NullHeader_False()
        {
            // Arrange
            MediaTypeHeaderValue value = null;

            // Act
            var isPlainText = value.IsPlainText();

            // Assert
            Assert.IsFalse(isPlainText);
        }

        [Test]
        public void IsPlainText_PlainTextHeader_True()
        {
            // Arrange
            MediaTypeHeaderValue value = MediaTypeHeaderValue.Parse("plain/text");

            // Act
            var isPlainText = value.IsPlainText();

            // Assert
            Assert.IsFalse(isPlainText);
        }
    }
}