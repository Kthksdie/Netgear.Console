using Netgear.App.Components;
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

            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            var trafficTable = new Table().RoundedBorder()
                .AddColumn(" ").AddColumn("Download").AddColumn("Upload");
            //var trafficPanel = new Panel(trafficTable)
            //    .Header("Bandwidth")
            //    .BorderColor(Color.Grey)
            //    .RoundedBorder();

            var trafficResponse = Get<TrafficResponse>("traffic").Result;
            trafficTable.AddRow("Day", $"{trafficResponse.Stats.NewTodayDownload.ToSize()}", $"{trafficResponse.Stats.NewTodayUpload.ToSize()}");
            trafficTable.AddRow("Month", $"{trafficResponse.Stats.NewMonthDownload.ToSize()}", $"{trafficResponse.Stats.NewMonthUpload.ToSize()}");

            var devicesTable = new Table().RoundedBorder()
                .AddColumn("Status").AddColumn("IP").AddColumn("Name").AddColumn("Signal Strength").AddColumn("Ping (ms)");
            //var devicesPanel = new Panel(devicesTable)
            //    .Header("Network | Devices")
            //    .BorderColor(Color.Grey)
            //    .RoundedBorder();

            var currentDevices = new List<Device>();
            var devicesResponse = Get<DevicesResponse>("devices").Result;
            currentDevices = devicesResponse.Devices;

            foreach (var device in currentDevices) {
                devicesTable.AddRow("", device.IP, device.Name, $"{device.SignalStrength}", "");
            }

            grid.AddRow(devicesTable, trafficTable);

            AnsiConsole.Clear();

            AnsiConsole.Live(grid).Start(ctx => {

                while (true) {
                    for (var row = 0; row < currentDevices.Count; row++) {
                        var device = currentDevices[row];
                        device.PreviousLatency = device.Latency ?? 0;

                        devicesTable.UpdateCell(row, 4, new Markup("---").Justify(Justify.Left));
                        ctx.Refresh();

                        var pingResponse = Get<PingResponse>($"ping?ip={device.IP}").Result;
                        device.Latency = pingResponse.Latency;

                        var status = pingResponse.Online ? $"[yellow]●[/]" : "[grey]●[/]";
                        var latencyValue = device.Latency ?? 0;
                        var previousLatencyValue = device.PreviousLatency;

                        var latencyDiff = previousLatencyValue > 0 ? (latencyValue - previousLatencyValue) : 0;

                        var latency = $"{latencyValue} {(latencyDiff > 0 ? $"+{latencyDiff}" : $"{latencyDiff}").ToSuperscript()}";

                        devicesTable.UpdateCell(row, 0, new Markup(status).Justify(Justify.Center));
                        devicesTable.UpdateCell(row, 4, new Markup(latency).Justify(Justify.Left));
                        ctx.Refresh();
                    }

                    devicesResponse = Get<DevicesResponse>("devices").Result;

                    foreach (var device in devicesResponse.Devices) {
                        var currentDevice = currentDevices.FirstOrDefault(x => x.MAC == device.MAC);
                        if (currentDevice == null) {
                            currentDevices.Add(device);
                            devicesTable.AddRow("", device.IP, device.Name, $"{device.SignalStrength}", "");
                        }
                        else {
                            var row = currentDevices.IndexOf(currentDevice);

                            devicesTable.UpdateCell(row, 3, new Markup($"{device.SignalStrength}").Justify(Justify.Left));
                        }
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
