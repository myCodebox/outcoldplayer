// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Web.Models
{
    using System;

    public class AddPlaylistResp : CommonResponse
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
    }
}