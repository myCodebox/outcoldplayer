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

        protected void RaiseCurrenntPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.RaisePropertyChanged(propertyName);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
