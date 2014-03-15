﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    /// <summary>
    /// The PageView interface.
    /// </summary>
    public interface IPageView : IView
    {
        /// <summary>
        /// Gets the title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the subtitle.
        /// </summary>
        string Subtitle { get; }

        /// <summary>
        /// On navigated to.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        void OnNavigatedTo(NavigatedToEventArgs eventArgs);

        /// <summary>
        /// On navigating from.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        void OnNavigatingFrom(NavigatingFromEventArgs eventArgs);

        /// <summary>
        /// On data loading.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        void OnDataLoading(NavigatedToEventArgs eventArgs);

        /// <summary>
        /// On unfreeze.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        void OnUnfreeze(NavigatedToEventArgs eventArgs);

        /// <summary>
        /// On data loaded.
        /// </summary>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        void OnDataLoaded(NavigatedToEventArgs eventArgs);
    }
}