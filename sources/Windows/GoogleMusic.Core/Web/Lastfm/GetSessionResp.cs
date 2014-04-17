// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Lastfm
{
    using System.Diagnostics.CodeAnalysis;

    public class GetSessionResp : ErrorResp
    {
        public Session Session { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class Session
    {
        public string Name { get; set; }

        public string Key { get; set; }
    }
}