// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using Windows.ApplicationModel.Resources;

    public class ApplicationResources : IApplicationResources
    {
        private readonly ResourceLoader resourceLoader = new ResourceLoader();

        public string GetString(string name)
        {
            return this.resourceLoader.GetString(name);
        }
    }
}