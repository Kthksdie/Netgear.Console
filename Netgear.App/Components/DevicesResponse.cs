using Netgear.App.Extensions;
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

        [JsonPropertyName("AllowOrBlock")]
        public string AllowOrBlock { get; set; }

        [JsonPropertyName("ConnectionType")]
        public string ConnectionType { get; set; }

        [JsonPropertyName("Linkspeed")]
        public int? Linkspeed { get; set; }

        public int? PreviousLinkspeed { get; set; }

        public int? LinkspeedDiff {
            get {
                return this.PreviousLinkspeed > 0 ? (this.Linkspeed - this.PreviousLinkspeed) : null;
            }
        }

        public string LinkspeedResult() {
            if (this.Latency == null) {
                return $"[grey]⨯[/]";
            }

            var result = (this.LinkspeedDiff > 0 ? $"[green]+{this.LinkspeedDiff}[/]" : $"[grey]{this.LinkspeedDiff}[/]").ToSuperscript();

            return $"{this.Linkspeed}{(this.LinkspeedDiff != 0 ? result : string.Empty)}";
        }

        [JsonPropertyName("SignalStrength")]
        public int SignalStrength { get; set; }

        public int PreviousSignalStrength { get; set; } = 0;

        public int SignalStrengthDiff {
            get {
                return this.PreviousSignalStrength > 0 ? (this.SignalStrength - this.PreviousSignalStrength) : 0;
            }
        }

        public string SignalStrengthResult() {
            if (ConnectionType == "wired") {
                return $"∞";
            }

            var result = $"{(this.SignalStrengthDiff > 0 ? $"[green]+{this.SignalStrengthDiff}[/]" : $"[grey]{this.SignalStrengthDiff}[/]").ToSuperscript()}";

            return $"{this.SignalStrength}{(this.SignalStrengthDiff != 0 ? result : string.Empty)}";
        }

        [JsonPropertyName("Latency")]
        public int? Latency { get; set; }

        public int PreviousLatency { get; set; } = 0;

        public int LatencyDiff {
            get {
                return this.PreviousLatency > 0 ? ((this.Latency ?? 0) - this.PreviousLatency) : 0;
            }
        }

        public string LatencyResult() {
            if (this.Latency == null) {
                return $"[grey]⨯[/]";
            }

            var result = (this.LatencyDiff > 0 ? $"[grey]+{this.LatencyDiff}[/]" : $"[green]{this.LatencyDiff}[/]").ToSuperscript();

            return $"{this.Latency}{(this.LatencyDiff != 0 ? result : string.Empty)}";
        }
    }
}
