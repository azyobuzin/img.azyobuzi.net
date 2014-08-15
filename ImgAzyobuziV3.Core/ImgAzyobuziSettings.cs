using System.Runtime.Serialization;

namespace ImgAzyobuziV3.Core
{
    [DataContract]
    public class ImgAzyobuziSettings
    {
        [DataMember]
        public RedisSetting Redis { get; set; }

        [DataMember]
        public InfluxDbSetting InfluxDb { get; set; }
    }

    [DataContract]
    public class RedisSetting
    {
        [DataMember]
        public string Configuration { get; set; }

        [DataMember]
        public int Database { get; set; }
    }

    [DataContract]
    public class InfluxDbSetting
    {
        [DataMember]
        public string Uri { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Database { get; set; }
    }
}
