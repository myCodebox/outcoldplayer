// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    public class GoogleConfigResponse
    {
        public string Kind { get; set; }

        public GoogleConfigData Data { get; set; } 
    }

    public class GoogleConfigData
    {
        public GoogleConfigEntity[] Entries { get; set; } 
    }

    public class GoogleConfigEntity
    {
        public string Kind { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
