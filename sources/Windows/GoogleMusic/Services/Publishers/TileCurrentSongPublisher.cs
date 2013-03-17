// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;

    internal class TileCurrentSongPublisher : ICurrentSongPublisher
    {
        private const string CurrentSongTileTag = "CurrentSong";

        public TileCurrentSongPublisher()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        public PublisherType PublisherType
        {
            get { return PublisherType.ImmediatelyWithAlbumArt; }
        }

        public Task PublishAsync(SongBindingModel song, PlaylistBaseBindingModel currentPlaylist, Uri albumArtUri, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
                {
                    XmlDocument wideTileTemplate = this.GenerateWideTile(song, albumArtUri);
                    XmlDocument squareTileTemplate = this.GenerateSquareTile(song);

                    IXmlNode squareBindingNode = squareTileTemplate.GetElementsByTagName("binding").Item(0);
                    IXmlNode visualNode = wideTileTemplate.GetElementsByTagName("visual").Item(0);
                    if (visualNode != null && squareBindingNode != null)
                    {
                        visualNode.AppendChild(wideTileTemplate.ImportNode(squareBindingNode, true));
                    }

                    var tileNotification = new TileNotification(wideTileTemplate)
                                               {
                                                   ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(song.Duration),
                                                   Tag = CurrentSongTileTag
                                               };

                    TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
                });
        }

        private XmlDocument GenerateWideTile(SongBindingModel song, Uri albumArtUri)
        {
            XmlDocument templateContent = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideSmallImageAndText02);
            XmlNodeList images = templateContent.GetElementsByTagName("image");
            ((XmlElement)images[0]).SetAttribute("src", albumArtUri.ToString());
            ((XmlElement)images[0]).SetAttribute("alt", "Album Art");

            XmlNodeList textElements = templateContent.GetElementsByTagName("text");
            this.SetTextElements(textElements, templateContent, song);

            return templateContent;
        }

        private XmlDocument GenerateSquareTile(SongBindingModel song)
        {
            XmlDocument templateContent = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText03);

            XmlNodeList textElements = templateContent.GetElementsByTagName("text");
            this.SetTextElements(textElements, templateContent, song);

            return templateContent;
        }

        private void SetTextElements(XmlNodeList textElements, XmlDocument templateContent, SongBindingModel song)
        {
            textElements[0].AppendChild(templateContent.CreateTextNode(song.Title));
            textElements[1].AppendChild(templateContent.CreateTextNode(song.Artist));
            textElements[2].AppendChild(templateContent.CreateTextNode(song.Album));
            textElements[3].AppendChild(templateContent.CreateTextNode(TimeSpan.FromSeconds(song.Duration).ToPresentString()));
        }
    }
}
