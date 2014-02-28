// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using System.Collections.Generic;

    public class GoogleListResponse<TDataType> : CommonResponse
    {
        public string Kind { get; set; }

        public string NextPageToken { get; set; }

        public GoogleData<TDataType> Data { get; set; }
    }

    public class GoogleData<TDataType>
    {
        public List<TDataType> Items { get; set; }
    }
}