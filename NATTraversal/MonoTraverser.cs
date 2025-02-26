using Mono.Nat;
using System.Diagnostics;

namespace NATTraversal
{
    public class MonoTraverser
    {
        public MonoTraverser()
        {
            NatUtility.DeviceFound += NatUtility_DeviceFound;
            NatUtility.StartDiscovery();
        }

        INatDevice natDevice = null!;

        public bool Ready = false;

        private void NatUtility_DeviceFound(object? sender, DeviceEventArgs e)
        {
            natDevice = e.Device;

            Ready = true;

            Debug.WriteLine($"Device found: {natDevice.DeviceEndpoint}, using Protocol: {natDevice.NatProtocol}");
        }

        public bool TryForwardPort(int port, string description, int timeoutInSeconds = 15)
        {
            return TryForwardPort(port, port, description, timeoutInSeconds);
        }

        public bool TryForwardPort(int listen, int external, string description, int timeoutInSeconds = 15)
        {
            bool result = false;
            CancellationTokenSource Token = new();
            long StartTime = Stopwatch.GetTimestamp();

            while (!Token.IsCancellationRequested)
            {
                if (Stopwatch.GetElapsedTime(StartTime) > TimeSpan.FromSeconds(timeoutInSeconds))
                {
                    Token.Cancel();
                    result = false;
                }

                if (natDevice != null)
                {
                    Token.Cancel();
                    result = ForwardPort(listen, external, description);
                }
            }

            return result;
        }

        private bool ForwardPort(int @internal, int external, string description)
        {
            var mapping = new Mapping(Protocol.Tcp, @internal, external, 0, description);

            try
            {
                natDevice.CreatePortMap(mapping);
                Debug.WriteLine($"Port forwarded, external port {external}, internal port {@internal}");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Port forwarding failed: {0}", e.Message);
                Debug.WriteLine("nat", e.StackTrace);

                return false;
            }

            return true;
        }

        public string[] TryGetForwardedPorts(int timeoutInSeconds = 15)
        {
            CancellationTokenSource Token = new();
            long StartTime = Stopwatch.GetTimestamp();

            while (!Token.IsCancellationRequested)
            {
                if (Stopwatch.GetElapsedTime(StartTime) > TimeSpan.FromSeconds(timeoutInSeconds))
                {
                    Token.Cancel();
                }

                if (natDevice != null)
                {
                    Token.Cancel();
                    return GetForwardedPortsAsync().GetAwaiter().GetResult();
                }
            }

            return new string[] { "Failed to retrieved forwarded ports." };
        }

        private async Task<string[]> GetForwardedPortsAsync()
        {
            try
            {
                var maps = await natDevice.GetAllMappingsAsync();

                string header = $"{"Protocol",-8} | {"PrivatePort",-11} | {"PublicPort",-10} | {"Description",-11} | {"Lifetime",-8} | {"Expiration",-10}";

                string bar = Enumerable.Range(0, 58)
                    .Select(s => "-")
                    .Aggregate((a, b) => a + b);

                string[] output = maps
                    .Select(s => $"{s.Protocol,-8} | {s.PrivatePort,-11} | {s.PublicPort,-10} | {s.Description,-11} | {s.Lifetime,-8} | {s.Expiration,-10}")
                    .ToArray();

                return output
                    .Prepend(bar)
                    .Prepend(header)
                    .ToArray();
            }
            catch (Exception)
            {
                return new string[] { "Failed to retrieved forwarded ports." };
            }
        }
    }
}
