// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleMusicSituations
    {
        public string PrimaryHeader { get; set; }

        public GoogleMusicSituation[] Situations { get; set; }
    }

    public class GoogleMusicSituation
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public string WideImageUrl { get; set; }

        public GoogleMusicSituation[] Situations { get; set; }

        public GoogleMusicRadio[] Stations { get; set; }
    }
}
