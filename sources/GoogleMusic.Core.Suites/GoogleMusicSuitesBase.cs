// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites
{
    using OutcoldSolutions.GoogleMusic.Diagnostics;

    public class GoogleMusicSuitesBase : SuitesBase
    {
        public override void SetUp()
        {
            base.SetUp();

            this.LogManager.Writers.AddOrUpdate(typeof(DebugLogWriter), type => new DebugLogWriter(), (type, writer) => writer);
        }
    }
}