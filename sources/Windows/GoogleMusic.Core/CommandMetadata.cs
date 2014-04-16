// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Services.Actions;

    /// <summary>
    /// The command metadata.
    /// </summary>
    public class CommandMetadata
    {
        public CommandMetadata(string iconName, string title, DelegateCommand command)
        {
            this.IconName = iconName;
            this.Command = command;
            this.Title = title;
        }

        public CommandMetadata(string iconName, string title, ActionGroup actionGroup, DelegateCommand command)
        {
            this.IconName = iconName;
            this.Command = command;
            this.Title = title;
            this.ActionGroup = actionGroup;
        }

        /// <summary>
        /// Gets or sets the icon name.
        /// </summary>
        public string IconName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public DelegateCommand Command { get; set; }

        public ActionGroup ActionGroup { get; set; }
    }
}