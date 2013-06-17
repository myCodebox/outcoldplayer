// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleRadio : CommonResponse
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string[] ImageUrl { get; set; }

        public double RecentTimestamp { get; set; }

        public RadioSeedId RadioSeedId { get; set; }
    }
}