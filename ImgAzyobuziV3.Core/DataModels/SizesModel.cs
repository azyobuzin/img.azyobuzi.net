﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace ImgAzyobuziV3.Core.DataModels
{
    [JsonObject]
    public class SizesModel
    {
        public SizesModel() { }

        public SizesModel(IResolver resolver, IReadOnlyCollection<ImageInfo> images, string id)
        {
            this.ServiceName = resolver.ServiceName;
            this.Images = images;
            this.ServiceId = resolver.ServiceId;
            this.Id = id;
        }

        [JsonProperty("service")]
        public string ServiceName { get; set; }

        [JsonProperty("images")]
        public IReadOnlyCollection<ImageInfo> Images { get; set; }

        [JsonIgnore]
        public string ServiceId { get; set; }

        [JsonIgnore]
        public string Id { get; set; }
    }
}
