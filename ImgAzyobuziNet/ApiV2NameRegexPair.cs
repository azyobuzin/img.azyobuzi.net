using Jil;
using Newtonsoft.Json;

namespace ImgAzyobuziNet
{
    public struct ApiV2NameRegexPair
    {
        [JilDirective("name"), JsonProperty("name")]
        public string Name { get; set; }

        [JilDirective("regex"), JsonProperty("regex")]
        public string Regex { get; set; }

        public ApiV2NameRegexPair(string name, string regex)
        {
            this.Name = name;
            this.Regex = regex;
        }
    }
}
