// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services.Publishers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;

    internal class TileCurrentSongPublisher : ICurrentSongPublisher
    {
        private const string CurrentSongTileTag = "CurrentSong";
        private bool isInitialized = false;

        public PublisherType PublisherType
        {
            get { return PublisherType.ImmediatelyWithAlbumArt; }
        }

        public Task PublishAsync(Song song, IPlaylist currentPlaylist, Uri albumArtUri, CancellationToken cancellationToken)
        {
            this.TilesInitialization();

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
                                                   ExpirationTime = DateTimeOffset.UtcNow.Add(song.Duration),
                                                   Tag = CurrentSongTileTag
                                               };

                    TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
                }, cancellationToken);
        }

        private void TilesInitialization()
        {
            if (!this.isInitialized)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                this.isInitialized = true;
            }
        }

        private XmlDocument GenerateWideTile(Song song, Uri albumArtUri)
        {
            XmlDocument templateContent = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideSmallImageAndText02);
            XmlNodeList images = templateContent.GetElementsByTagName("image");
            ((XmlElement)images[0]).SetAttribute("src", albumArtUri.ToString());
            ((XmlElement)images[0]).SetAttribute("alt", "Album Art");

            XmlNodeList textElements = templateContent.GetElementsByTagName("text");
            this.SetTextElements(textElements, templateContent, song);

            return templateContent;
        }

        private XmlDocument GenerateSquareTile(Song song)
        {
            XmlDocument templateContent = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText03);

            XmlNodeList textElements = templateContent.GetElementsByTagName("text");
            this.SetTextElements(textElements, templateContent, song);

            return templateContent;
        }

        private void SetTextElements(XmlNodeList textElements, XmlDocument templateContent, Song song)
        {
            textElements[0].AppendChild(templateContent.CreateTextNode(song.Title));
            textElements[1].AppendChild(templateContent.CreateTextNode(song.GetSongArtist()));
            textElements[2].AppendChild(templateContent.CreateTextNode(song.AlbumTitle));
            textElements[3].AppendChild(templateContent.CreateTextNode(song.Duration.ToPresentString()));
        }
    }
}
