namespace ImgAzyobuziNet.Core
{
    public class ImgAzyobuziNetOptions
    {
        public ApiKeyOptions ApiKeys { get; set; }
        public ResolverCacheOptions ResolverCache { get; set; }
    }

    public class ApiKeyOptions
    {
        public string FlickrApiKey { get; set; }
        public string InstagramAccessToken { get; set; }
        public string MobypictureDeveloperKey { get; set; }
        public string TinamiApiKey { get; set; }
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterAccessToken { get; set; }
    }

    public class ResolverCacheOptions
    {
        public ResolverCacheType Type { get; set; } = ResolverCacheType.Memory;
        public double? ExpirationSeconds { get; set; } = 60 * 60 * 24; // 1day

        public string AzureTableStorageConnectionString { get; set; }
        public string AzureTableStorageTableName { get; set; }
    }

    public enum ResolverCacheType
    {
        None,
        Memory,
        AzureTableStorage,
    }
}
