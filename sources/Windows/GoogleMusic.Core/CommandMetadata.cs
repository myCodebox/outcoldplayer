// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System;

    public class CommandMetadata
    {
        public CommandMetadata(string iconName, DelegateCommand command)
        {
            if (iconName == null)
            {
                throw new ArgumentNullException("iconName");
            }

            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            this.IconName = iconName;
            this.Command = command;
        }

        public CommandMetadata(string iconName, string title, DelegateCommand command)
            : this(iconName, command)
        {
            if (title == null)
            {
                throw new ArgumentNullException("title");
            }

            this.Title = title;
        }

        public string IconName { get; set; }

        public string Title { get; set; }

        public DelegateCommand Command { get; set; }
    }
}