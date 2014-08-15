using Newtonsoft.Json;

namespace ImgAzyobuziV3.Core.DataModels
{
    [JsonObject]
    public class RegexModel
    {
        public RegexModel() { }

        public RegexModel(IResolver resolver)
        {
            this.Name = resolver.ServiceName;
            this.Regex = resolver.Pattern.ToString();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("regex")]
        public string Regex { get; set; }
    }
}
