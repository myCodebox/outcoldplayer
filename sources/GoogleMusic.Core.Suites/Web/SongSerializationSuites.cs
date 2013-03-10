// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Suites.Web
{
    using Newtonsoft.Json;

    using NUnit.Framework;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public class SongSerializationSuites
    {
        [Test]
        public void Deserialize_PlaylistEntryIdIsNull_ShouldDeserialize()
        {
            const string SerializedSong =
                @"{""genre"":""Easy Listening"",""beatsPerMinute"":0,""albumArtistNorm"":"""",""artistNorm"":""phil collins"",""album"":""...But Seriously"",""lastPlayed"":1352817005540132,""type"":6,""recentTimestamp"":1352817005539000,""disc"":0,""id"":""f47f1b0c-c2fa-38d8-b845-32c82a6f645d"",""composer"":"""",""title"":""All Of My Life"",""albumArtist"":"""",""totalTracks"":0,""subjectToCuration"":false,""name"":""All Of My Life"",""totalDiscs"":0,""year"":0,""titleNorm"":""all of my life"",""artist"":""Phil Collins"",""albumNorm"":""...but seriously"",""track"":0,""durationMillis"":335000,""matchedId"":""Ti7w4xbvr54o2y6li5a5ckmog5m"",""albumArtUrl"":""//lh5.googleusercontent.com/d1hmFkdykRJJi5_6UX5UFIgfVo7p0073zv0LBbRFkQODE_vXi8RQRsQdUNhW\u003ds130-c-e100"",""deleted"":false,""url"":"""",""creationDate"":1352817005540132,""playCount"":0,""playlistEntryId"":""null"",""bitrate"":320,""rating"":0,""comment"":"""",""storeId"":""Ti7w4xbvr54o2y6li5a5ckmog5m""}";

            var song = JsonConvert.DeserializeObject<GoogleMusicSong>(SerializedSong);

            Assert.NotNull(song);
        }

    }
}
