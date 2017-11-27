using System;
using Newtonsoft.Json;

namespace ApiApp.Models
{
    public class UploadBatch
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string ByWho { get; set; }

        public string Tag { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }
    }
}