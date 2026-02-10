using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Netgear.App.Components {
    public class TrafficResponse {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("stats")]
        public TrafficStats Stats { get; set; } = new TrafficStats();
    }

    public class TrafficStats {
        public double NewTodayUpload { get; set; } = 0;
        public double NewTodayDownload { get; set; } = 0;
        public double NewMonthUpload { get; set; } = 0;
        public double NewMonthDownload { get; set; } = 0;
    }
}
