// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.Collections.Generic;

    using OutcoldSolutions.BindingModels;

    public class StartViewBindingModel : BindingModelBase
    {
        private List<PlaylistsGroupBindingModel> groups;

        public List<PlaylistsGroupBindingModel> Groups
        {
            get
            {
                return this.groups;
            }

            set
            {
                this.SetValue(ref this.groups, value);
            }
        }
    }
}