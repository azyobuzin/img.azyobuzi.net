using Newtonsoft.Json;

namespace ImgAzyobuziV3.Core.DataModels
{
    [JsonObject]
    public class ImageInfo
    {
        public ImageInfo() { }

        public ImageInfo(string full, string large, string thumb, string video = null)
        {
            this.Full = full;
            this.Large = large;
            this.Thumb = thumb;
            this.Video = video;
        }

        [JsonProperty("full")]
        public string Full { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("video")]
        public string Video { get; set; }
    }
}
