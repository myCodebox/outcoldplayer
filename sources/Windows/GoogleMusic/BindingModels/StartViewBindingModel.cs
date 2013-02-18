// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    public class StartViewBindingModel : BindingModelBase
    {
        public List<PlaylistsGroupBindingModel> Groups { get; set; }
    }
}