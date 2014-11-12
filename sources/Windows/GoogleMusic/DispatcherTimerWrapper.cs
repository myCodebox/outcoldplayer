// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic
{
    using System;
    using Windows.UI.Xaml;

    public class DispatcherTimerWrapper : ITimer
    {
        private readonly DispatcherTimer timer;

        public DispatcherTimerWrapper()
        {
            this.timer = new DispatcherTimer();
        }

        public event EventHandler<object> Tick
        {
            add { this.timer.Tick += value; }
            remove { this.timer.Tick -= value; }
        }

        public TimeSpan Interval
        {
            get { return this.timer.Interval; }
            set { this.timer.Interval = value; }
        }

        public void Start()
        {
            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
        }
    }
}
