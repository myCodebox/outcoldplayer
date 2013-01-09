// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites.WebServices
{
    using System.IO;

    using NUnit.Framework;

    public class PlainLinesBodyReaderSuites
    {
        [Test]
        public void GetValues_SimpleInput_AllValuesRead()
        {
            // Arrange
            using (var memoryStreamBody = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStreamBody))
                {
                    streamWriter.Write(@"SID=DQAAAGgA7Zg8CTN
LSID=DQAAAGsAlk8BBbG
Auth=DQAAAGgAdk3fA5N");
                    streamWriter.Flush();


                    memoryStreamBody.Seek(0, SeekOrigin.Begin);

                    //using (var reader = new PlainLinesBodyReader(memoryStreamBody))
                    //{
                    //    // Act
                    //    var dictionary = reader.GetValues();

                    //    // Assert
                    //    Assert.AreEqual("DQAAAGgA7Zg8CTN", dictionary["SID"]);
                    //    Assert.AreEqual("DQAAAGsAlk8BBbG", dictionary["LSID"]);
                    //    Assert.AreEqual("DQAAAGgAdk3fA5N", dictionary["Auth"]);
                    //}
                }
            }
        }
    }
}