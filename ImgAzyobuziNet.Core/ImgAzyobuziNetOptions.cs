using Microsoft.Extensions.Configuration;

namespace ImgAzyobuziNet.Core
{
    public class ImgAzyobuziNetOptions
    {
        public ApiKeys ApiKeys { get; set; }
        public string FallbackV2ApiUri { get; set; }

        public void BindConfiguration(IConfiguration configuration)
        {
            configuration.Bind(this);

            // 500pxConsumerKey はプロパティ名と違うので自分で代入
            if (configuration["ApiKeys:500pxConsumerKey"] is string x)
                (this.ApiKeys ?? (this.ApiKeys = new ApiKeys()))._500pxConsumerKey = x;
        }
    }

    public class ApiKeys
    {
        public string _500pxConsumerKey { get; set; }
        public string FlickrApiKey { get; set; }
        public string InstagramAccessToken { get; set; }
        public string MobypictureDeveloperKey { get; set; }
    }
}
