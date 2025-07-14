using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace rfid_receiver_api.Helper;

public static class NetworkHelper
{

    public static bool IsIpv6Available = true;

    public static async Task<bool> HasIpv6ConnectivityAsync(int timeoutMs = 3000)
    {
        const string googleIpv6 = "2001:4860:4860::8888";  // Google DNS (IPv6)
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(googleIpv6, timeoutMs);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            // Any exception (socket error, ICMP blocked, etc.) → treat as no IPv6
            return false;
        }
    }

    public static bool HasIpv6Connectivity(int timeoutMs = 3000)
    {
        const string googleIpv6 = "2001:4860:4860::8888";  // Google DNS (IPv6)
        try
        {
            using var ping = new Ping();
            var reply = ping.Send(googleIpv6, timeoutMs);
            IsIpv6Available = reply.Status == IPStatus.Success;
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            // Any exception (socket error, ICMP blocked, etc.) → treat as no IPv6
            IsIpv6Available = false;
            return false;
        }
    }
}