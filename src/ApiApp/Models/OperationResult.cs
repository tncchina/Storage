using System;
using Newtonsoft.Json;

namespace ApiApp.Models
{
    public class OperationResult
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public DateTime CreationTime { get; set; }

        public OperationStatus Status { get; set; }
    }
}