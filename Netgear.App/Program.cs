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

            var existingDevices = new List<Device>();
            var devicesResponse = Get<DevicesResponse>("devices").Result;
            existingDevices = devicesResponse.Devices;

            foreach (var device in existingDevices) {
                devicesTable.AddRow("", device.IP, device.Name, $"{device.SignalStrength}", "");
            }

            grid.AddRow(devicesTable, trafficTable);

            AnsiConsole.Clear();

            AnsiConsole.Live(grid).Start(ctx => {

                while (true) {
                    var allDevices = new List<Device>();
                    foreach (var existingDevice in existingDevices) {
                        existingDevice.PreviousSignalStrength = existingDevice.SignalStrength;
                        allDevices.Add(existingDevice);
                    }

                    devicesResponse = Get<DevicesResponse>("devices").Result;
                    var currentDevices = devicesResponse.Devices;

                    foreach (var currentDevice in currentDevices) {
                        var sigStr = $"{currentDevice.SignalStrength}%";

                        var existingDevice = allDevices.FirstOrDefault(x => x.MAC == currentDevice.MAC);
                        if (existingDevice == null) {

                            devicesTable.AddRow("", currentDevice.IP, currentDevice.Name, sigStr, "");

                            allDevices.Add(currentDevice);
                        }
                        else {
                            existingDevice.SignalStrength = currentDevice.SignalStrength;
                        }
                    }

                    for (var row = 0; row < allDevices.Count; row++) {
                        var device = allDevices[row];
                        device.PreviousLatency = device.Latency ?? 0;

                        var pingResponse = Get<PingResponse>($"ping?ip={device.IP}").Result;
                        device.Latency = pingResponse.Latency;

                        var status = pingResponse.Online ? $"[yellow]●[/]" : "[grey]●[/]";
                        var sigStrDiff = device.PreviousSignalStrength > 0 ? (device.SignalStrength - device.PreviousSignalStrength) : 0;
                        var sigStr = $"{device.PreviousSignalStrength} {(sigStrDiff > 0 ? $"+{sigStrDiff}" : $"{sigStrDiff}").ToSuperscript()}";

                        var latencyValue = device.Latency ?? 0;
                        var previousLatencyValue = device.PreviousLatency;
                        var latencyDiff = previousLatencyValue > 0 ? (latencyValue - previousLatencyValue) : 0;
                        var latency = $"{latencyValue} {(latencyDiff > 0 ? $"+{latencyDiff}" : $"{latencyDiff}").ToSuperscript()}";



                        devicesTable.UpdateCell(row, 0, new Markup(status));
                        devicesTable.UpdateCell(row, 3, new Markup(sigStr));
                        devicesTable.UpdateCell(row, 4, new Markup(latency));
                        ctx.Refresh();
                    }

                    //devicesResponse = Get<DevicesResponse>("devices").Result;
                    foreach (var device in devicesResponse.Devices) {
                        var currentDevice = existingDevices.FirstOrDefault(x => x.MAC == device.MAC);
                        if (currentDevice == null) {
                            existingDevices.Add(device);
                            devicesTable.AddRow("", device.IP, device.Name, $"{device.SignalStrength}", "");
                        }
                        else {
                            var row = existingDevices.IndexOf(currentDevice);

                            devicesTable.UpdateCell(row, 3, new Markup($"{device.SignalStrength}").Justify(Justify.Left));
                        }
                    }

                    ctx.Refresh();

                    trafficResponse = Get<TrafficResponse>("traffic").Result;
                    devicesTable.UpdateCell(0, 0, new Markup($"{trafficResponse.Stats.NewTodayDownload}"));
                    devicesTable.UpdateCell(0, 1, new Markup($"{trafficResponse.Stats.NewTodayUpload}"));
                    devicesTable.UpdateCell(1, 0, new Markup($"{trafficResponse.Stats.NewMonthDownload}"));
                    devicesTable.UpdateCell(1, 1, new Markup($"{trafficResponse.Stats.NewMonthUpload}"));

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
