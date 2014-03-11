// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SQLite;

    [Table("Radio")]
    public class Radio : IPlaylist, IMixedPlaylist
    {
        [PrimaryKey, Column("RadioId")]
        public string RadioId { get; set; }

        [Ignore]
        public string Id
        {
            get
            {
                return this.RadioId;
            }
        }

        [Ignore]
        public PlaylistType PlaylistType
        {
            get
            {
                return PlaylistType.Radio;
            }
        }

        public string Title { get; set; }

        [Indexed]
        public string TitleNorm { get; set; }

        [Ignore]
        public int SongsCount { get; set; }

        [Ignore]
        public int OfflineSongsCount { get; set; }

        [Ignore]
        public TimeSpan Duration { get; set; }

        [Ignore]
        public TimeSpan OfflineDuration { get; set; }

        public Uri ArtUrl { get; set; }

        public Uri ArtUrl1 { get; set; }
        
        public Uri ArtUrl2 { get; set; }

        public Uri ArtUrl3 { get; set; }

        [Ignore]
        public Uri[] ArtUrls
        {
            get
            {
                var urls = new List<Uri>();

                if (this.ArtUrl != null)
                {
                    urls.Add(this.ArtUrl);
                }

                if (this.ArtUrl1 != null)
                {
                    urls.Add(this.ArtUrl1);
                }

                if (this.ArtUrl2 != null)
                {
                    urls.Add(this.ArtUrl2);
                }

                if (this.ArtUrl3 != null)
                {
                    urls.Add(this.ArtUrl3);
                }

                return urls.ToArray();
            }

            set
            {
                
            }
        }

        [Indexed]
        public DateTime Recent { get; set; }

        public string ClientId { get; set; }

        public DateTime LastModified { get; set; }

        public int SeedType { get; set; }

        public string SongId { get; set; }

        public string TrackLockerId { get; set; }

        public string GoogleArtistId { get; set; }

        public string GoogleAlbumId { get; set; }
    }
}
