using Netgear.App.Components;
using Netgear.App.Extensions;
using Newtonsoft.Json;
using Spectre.Console;

namespace Netgear.App {
    internal class Program {
        private static HttpClient _httpClient = new HttpClient();
        private static string _netgearApiUrl = "http://localhost:3000/api/router";

        static async Task Main(string[] args) {
            Console.Title = "Dogwatch";

            AnsiConsole.MarkupLine("Starting...");

            // devices
            // ping?ip=192.168.1.19
            // traffic

            //var trafficTable = new Table().BorderColor(Color.Grey).RoundedBorder()
            //    .AddColumn(" ").AddColumn("Download").AddColumn("Upload");

            //var trafficResponse = Get<TrafficResponse>("traffic").Result;
            //trafficTable.AddRow("Day", $"{trafficResponse.Stats.NewTodayDownload.ToSize()}", $"{trafficResponse.Stats.NewTodayUpload.ToSize()}");
            //trafficTable.AddRow("Month", $"{trafficResponse.Stats.NewMonthDownload.ToSize()}", $"{trafficResponse.Stats.NewMonthUpload.ToSize()}");

            var devicesTable = new Table().BorderColor(Color.Grey).RoundedBorder().Expand()
                .AddColumn("Type")
                .AddColumn("IP")
                .AddColumn("Name")
                .AddColumn("Signal Strength")
                .AddColumn("Linkspeed")
                .AddColumn("Ping (ms)");

            var existingDevices = new List<Device>();
            var devicesResponse = Get<DevicesResponse>("devices").Result;
            existingDevices = devicesResponse.Devices;

            existingDevices = existingDevices.OrderBy(x => Convert.ToInt32(x.IP.Split('.')[3])).ToList();
            foreach (var device in existingDevices) {
                devicesTable.AddRow(device.ConnectionType == "wireless" ? "wifi" : "wired", device.IP, device.Name, device.SignalStrengthResult(), device.LinkspeedResult(), device.LatencyResult());
            }

            AnsiConsole.Clear();

            AnsiConsole.Live(devicesTable).Start(ctx => {

                while (true) {
                    ctx.Refresh();

                    foreach (var existingDevice in existingDevices) {
                        existingDevice.PreviousLinkspeed = existingDevice.Linkspeed ?? 0;
                        existingDevice.PreviousSignalStrength = existingDevice.SignalStrength;
                    }

                    devicesResponse = Get<DevicesResponse>("devices").Result;
                    var currentDevices = devicesResponse.Devices;

                    foreach (var currentDevice in currentDevices) {
                        var existingDevice = existingDevices.FirstOrDefault(x => x.MAC == currentDevice.MAC);
                        if (existingDevice == null) {
                            existingDevices.Add(currentDevice);
                        }
                        else {
                            existingDevice.Linkspeed = currentDevice.Linkspeed;
                            existingDevice.SignalStrength = currentDevice.SignalStrength;
                        }
                    }

                    existingDevices = existingDevices.OrderBy(x => Convert.ToInt32(x.IP.Split('.')[3])).ToList();
                    devicesTable.Rows.Clear();

                    for (var row = 0; row < existingDevices.Count; row++) {
                        var device = existingDevices[row];
                        device.PreviousLatency = device.Latency ?? 0;

                        var pingResponse = Get<PingResponse>($"ping?ip={device.IP}").Result;
                        device.Latency = pingResponse.Latency;

                        devicesTable.AddRow(device.ConnectionType == "wireless" ? "wifi" : "wired", device.IP, device.Name, device.SignalStrengthResult(), device.LinkspeedResult(), device.LatencyResult());
                    }

                    ctx.Refresh();

                    Thread.Sleep(5 * 1000);
                }

            });


            AnsiConsole.MarkupLine("Finished.");
            Console.ReadKey();
        }

        static async Task<T> Get<T>(string requestUrl) {
            var url = $"{_netgearApiUrl}/{requestUrl}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<T>(responseBody);

            return responseJson;
        }

        static async Task<dynamic?> Post(string requestUrl, dynamic content) {
            var stringContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");

            var url = $"{_netgearApiUrl}/{requestUrl}";

            var response = await _httpClient.PostAsync(url, stringContent);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonConvert.DeserializeObject<dynamic>(responseBody);

            return responseJson;
        }
    }
}
