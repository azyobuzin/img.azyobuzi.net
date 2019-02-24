namespace ImgAzyobuziNet.Core
{
    public class ImgAzyobuziNetOptions
    {
        public ApiKeys ApiKeys { get; set; }
        public string FallbackV2ApiUri { get; set; }
    }

    public class ApiKeys
    {
        public string FlickrApiKey { get; set; }
        public string InstagramAccessToken { get; set; }
        public string MobypictureDeveloperKey { get; set; }
        public string TinamiApiKey { get; set; }
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterAccessToken { get; set; }
    }
}
