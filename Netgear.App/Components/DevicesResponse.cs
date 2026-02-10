using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Netgear.App.Components {
    public class DevicesResponse {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("devices")]
        public List<Device> Devices { get; set; }
    }

    public class Device {
        [JsonPropertyName("IP")]
        public string IP { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("MAC")]
        public string MAC { get; set; }

        [JsonPropertyName("ConnectionType")]
        public string ConnectionType { get; set; }

        [JsonPropertyName("Linkspeed")]
        public int? Linkspeed { get; set; }

        [JsonPropertyName("SignalStrength")]
        public int SignalStrength { get; set; }

        [JsonPropertyName("AllowOrBlock")]
        public string AllowOrBlock { get; set; }

        [JsonPropertyName("latency")]
        public int? Latency { get; set; }

        [JsonPropertyName("previousLatency")]
        public int PreviousLatency { get; set; } = 0;
    }
}
