// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public class BindingModelBase : INotifyPropertyChanged
    {
        private readonly Dictionary<string, List<EventHandler<PropertyChangedEventArgs>>> subscriptions =
            new Dictionary<string, List<EventHandler<PropertyChangedEventArgs>>>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Subscribe(
            Expression<Func<object>> expression,
            EventHandler<PropertyChangedEventArgs> action)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.Subscribe(PropertyNameExtractor.GetPropertyName(expression), action);
        }

        public void Unsubscribe(
            Expression<Func<object>> expression,
            EventHandler<PropertyChangedEventArgs> action)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.Unsubscribe(PropertyNameExtractor.GetPropertyName(expression), action);
        }

        public void Subscribe(
            string propertyName,
            EventHandler<PropertyChangedEventArgs> action)
        {
            lock (this.subscriptions)
            {
                List<EventHandler<PropertyChangedEventArgs>> propertySubscriptions;
                if (this.subscriptions.TryGetValue(propertyName, out propertySubscriptions))
                {
                    propertySubscriptions.Add(action);
                }
                else
                {
                    this.subscriptions.Add(propertyName, new List<EventHandler<PropertyChangedEventArgs>> { action });
                }
            }
        }

        public void Unsubscribe(
            string propertyName,
            EventHandler<PropertyChangedEventArgs> action)
        {
            lock (this.subscriptions)
            {
                List<EventHandler<PropertyChangedEventArgs>> propertySubscriptions;
                if (this.subscriptions.TryGetValue(propertyName, out propertySubscriptions))
                {
                    propertySubscriptions.Remove(action);
                }
            }
        }

        public void ClearPropertyChangedSubscriptions()
        {
            this.PropertyChanged = null;

            lock (this.subscriptions)
            {
                this.subscriptions.Clear();
            }
        }

        protected void RaisePropertyChanged(Expression<Func<object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this.RaisePropertyChanged(PropertyNameExtractor.GetPropertyName(expression));
        }

        protected void RaiseCurrentPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.RaisePropertyChanged(propertyName);
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var eventArgs = new PropertyChangedEventArgs(propertyName);

            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

            List<EventHandler<PropertyChangedEventArgs>> propertySubscriptions = null;

            lock (this.subscriptions)
            {
                List<EventHandler<PropertyChangedEventArgs>> result;
                if (this.subscriptions.TryGetValue(propertyName, out result))
                {
                    propertySubscriptions = result.ToList();
                }
            }

            if (propertySubscriptions != null)
            {
                foreach (var propertySubscription in propertySubscriptions)
                {
                    propertySubscription(this, eventArgs);
                }
            }
        }
    }
}
