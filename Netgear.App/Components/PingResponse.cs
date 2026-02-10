using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Netgear.App.Components {
    public class PingResponse {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("ip")]
        public string IP { get; set; }

        [JsonPropertyName("online")]
        public bool Online { get; set; }

        [JsonPropertyName("latency")]
        public int? Latency { get; set; }
    }
}
