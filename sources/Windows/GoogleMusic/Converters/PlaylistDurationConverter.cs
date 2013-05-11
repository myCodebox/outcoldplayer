// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public class PlaylistDurationConverter : IValueConverter
    {
        private readonly Lazy<IApplicationStateService> stateService = new Lazy<IApplicationStateService>(() => ApplicationBase.Container.Resolve<IApplicationStateService>());

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var playlist = value as IPlaylist;
            if (playlist != null)
            {
                if (stateService.Value.IsOnline())
                {
                    return playlist.Duration.ToPresentString();
                }
                else
                {
                    return playlist.OfflineDuration.ToPresentString();
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}