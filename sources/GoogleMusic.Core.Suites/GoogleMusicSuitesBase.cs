// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Suites
{
    using DebugLogWriter = OutcoldSolutions.GoogleMusic.Diagnostics.DebugLogWriter;

    public class GoogleMusicSuitesBase : SuitesBase
    {
        public override void SetUp()
        {
            base.SetUp();

            this.LogManager.Writers.AddOrUpdate(typeof(DebugLogWriter), type => new DebugLogWriter(), (type, writer) => writer);
        }
    }
}