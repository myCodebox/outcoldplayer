namespace OutcoldSolutions.GoogleMusic.Converters
{
    using System;
    using System.Globalization;
    using System.Text;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;

    public class AlbumDetailsConverter : IValueConverter
    {
        private readonly Lazy<IApplicationStateService> stateService = new Lazy<IApplicationStateService>(() => ApplicationContext.Container.Resolve<IApplicationStateService>());

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var playlist = value as Album;

            if (playlist != null)
            {
                int count = 0;

                if (this.stateService.Value.IsOnline())
                {
                    count = playlist.SongsCount;
                }
                else
                {
                    count = playlist.OfflineSongsCount;
                }

                StringBuilder stringBuilder = new StringBuilder();

                if (count > 0)
                {
                    if (count == 1)
                    {
                        stringBuilder.Append("1 Song");
                    }
                    else
                    {
                        stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} Songs", count);
                    }
                }

                if (playlist.Year.HasValue)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(" - ");
                    }

                    stringBuilder.Append(playlist.Year.Value);
                }

                return stringBuilder.ToString();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
