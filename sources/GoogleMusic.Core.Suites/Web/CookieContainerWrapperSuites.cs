// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using System;
    using System.Linq;
    using System.Net;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Web;

    public class CookieContainerWrapperSuites : GoogleMusicSuitesBase
    {
        private static readonly Uri BaseUri = new Uri("https://play.google.com/music/");

        private CookieContainerWrapper cookieContainer;

        public override void SetUp()
        {
            base.SetUp();

            this.cookieContainer = new CookieContainerWrapper(BaseUri);
        }

        [Test]
        public void AddCookies_PathIsTooLong_PathCorrectedToOriginal()
        {
            // Arrange
            const string CookieName = "CookieX";

            // Act
            this.cookieContainer.AddCookies(new[] { new Cookie(CookieName, "X", "/music/refresh", "play.google.com") });
            var cookie = this.cookieContainer.FindCookie(CookieName);

            // Assert
            Assert.AreEqual("/music", cookie.Path);
        }

        [Test]
        public void FindCookie_SearchExistingCookie_ReturnCookie()
        {
            // Arrange
            var originalCookie = new Cookie("CookieName", "X", "/music", "play.google.com");
            this.cookieContainer.AddCookies(new[] { originalCookie });

            // Act
            var cookie = this.cookieContainer.FindCookie(originalCookie.Name);

            // Assert
            Assert.IsNotNull(cookie);
            Assert.AreEqual(originalCookie.Name, cookie.Name);
            Assert.AreEqual(originalCookie.Value, cookie.Value);
        }

        [Test]
        public void FindCookie_CookieIsNotExist_ReturnNull()
        {
            // Arrange
            var originalCookie = new Cookie("CookieName", "X", "/music", "play.google.com");
            this.cookieContainer.AddCookies(new[] { originalCookie });

            // Act
            var cookie = this.cookieContainer.FindCookie("UnknownName");

            // Assert
            Assert.IsNull(cookie);
        }

        [Test]
        public void GetCookies_ContainerIsNotEmpty_ReturnAllCookies()
        {   
            // Arrange
            var originalCookie1 = new Cookie("CookieNameA", "A", "/music", "play.google.com");
            var originalCookie2 = new Cookie("CookieNameB", "B", "/", ".google.com");
            this.cookieContainer.AddCookies(new[] { originalCookie1, originalCookie2 });

            // Act
            var cookies = this.cookieContainer.GetCookies();

            // Assert
            Assert.IsNotNull(cookies);
            var array = cookies.ToList();
            Assert.AreEqual(2, array.Count);
            Assert.IsTrue(array.Any(x => string.Equals(originalCookie1.Name, x.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(originalCookie1.Value, x.Value, StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(array.Any(x => string.Equals(originalCookie2.Name, x.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(originalCookie2.Value, x.Value, StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void GetCookieHeader_ContainerIsNotEmpty_ReturnHeader()
        {
            // Arrange
            var originalCookie1 = new Cookie("CookieNameA", "A", "/music", "play.google.com");
            var originalCookie2 = new Cookie("CookieNameB", "B", "/", ".google.com");
            this.cookieContainer.AddCookies(new[] { originalCookie1, originalCookie2 });

            // Act
            var header = this.cookieContainer.GetCookieHeader();

            // Assert
            Assert.IsNotNull(header);
            Assert.IsTrue(header.Contains(originalCookie1.Name));
            Assert.IsTrue(header.Contains(originalCookie2.Name));
        }

        [Test]
        public void SetCookies_CookieWithEmptyDomainAndPath_DomainAndPathAreCorrected()
        {
            // Act
            this.cookieContainer.SetCookies(new[] { "CookieX=X" });

            // Assert
            var cookie = this.cookieContainer.FindCookie("CookieX");
            Assert.AreEqual("/music", cookie.Path);
            Assert.AreEqual("play.google.com", cookie.Domain);
        }

        [Test]
        public void SetCookies_CookieWithEmptyDomain_DomainIsCorrected()
        {
            // Act
            this.cookieContainer.SetCookies(new[] { "CookieX=X;Path=/" });

            // Assert
            var cookie = this.cookieContainer.FindCookie("CookieX");
            Assert.AreEqual("/", cookie.Path);
            Assert.AreEqual("play.google.com", cookie.Domain);
        }

        [Test]
        public void SetCookies_CookieWithEmptyDomainAndPathAndSecureString_DomainIsCorrected()
        {
            // Act
            this.cookieContainer.SetCookies(new[] { "CookieX=X; Secure" });

            // Assert
            var cookie = this.cookieContainer.FindCookie("CookieX");
            Assert.AreEqual("/music", cookie.Path);
            Assert.AreEqual("play.google.com", cookie.Domain);
        }
    }
}