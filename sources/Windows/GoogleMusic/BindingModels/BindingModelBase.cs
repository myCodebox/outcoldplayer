// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.BindingModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class BindingModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // TODO: Implement expressions
        public void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void RaiseCurrentPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.RaisePropertyChanged(propertyName);
        }
    }
}
