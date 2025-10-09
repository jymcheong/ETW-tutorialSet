using ETWhelper;
using Microsoft.Diagnostics.Tracing;
using System.Net;

using (ETWatcher etw = new()
{   
    NetworkEventHandler = Handler_NetworkEvent
})
{
    etw.Start();
    Console.ReadKey();
    //Thread.Sleep(Timeout.Infinite); // Keeps the process alive forever...
}

/**
 * 
 ADMIN Psh> (Get-WinEvent -ListProvider (Microsoft-Windows-Kernel-Network).Events | Select Id, Description

Id Description
-- -----------
10 TCPv4: %2 bytes transmitted from %4:%6 to %3:%5.
11 TCPv4: %2 bytes received from %4:%6 to %3:%5.
12 TCPv4: Connection attempted between %4:%6 and %3:%5.
13 TCPv4: Connection closed between %4:%6 and %3:%5.
14 TCPv4: %2 bytes retransmitted from %4:%6 to %3:%5.
15 TCPv4: Connection established between %4:%6 and %3:%5.
16 TCPv4: Reconnect attempt between %4:%6 and %3:%5.
17 TCPv4: Connection attempt failed with error code %2.
18 TCPv4: %2 bytes copied in protocol on behalf of user for connection between %4:%6 and %3:%5.
26 TCPv6: %2 bytes transmitted from %4:%6 to %3:%5.
27 TCPv6: %2 bytes received from %4:%6 to %3:%5.
28 TCPv6: Connection attempted between %4:%6 and %3:%5.
29 TCPv6: Connection closed between %4:%6 and %3:%5.
30 TCPv6: %2 bytes retransmitted from %4:%6 to %3:%5.
31 TCPv6: Connection established between %4:%6 and %3:%5.
32 TCPv6: Reconnect attempt between %4:%6 and %3:%5.
34 TCPv6: %2 bytes copied in protocol on behalf of user for connection between %4:%6 and %3:%5.
42 UDPv4: %2 bytes transmitted from %4:%6 to %3:%5.
43 UDPv4: %2 bytes received from %4:%6 to %3:%5.
49 UDPv4: Connection attempt failed with error code %2.
58 UDPv6: %2 bytes transmitted from %4:%6 to %3:%5.
59 UDPv6: %2 bytes received from %4:%6 to %3:%5.

 */
void Handler_NetworkEvent(TraceEvent obj)
{
    // Loop through all payload names and display key-value pairs
    Console.WriteLine($"\nEventID: {obj.ID}");
    for (int i = 0; i < obj.PayloadNames.Length; i++)
    {
        string payloadName = obj.PayloadNames[i];
        object payloadValue = obj.PayloadValue(i);
        Console.WriteLine($"{payloadName}: {payloadValue}");
    }
    if (obj.PayloadNames.Contains("daddr"))
    {
        Console.WriteLine($"Converted Destination: {ConvertToIPAddress(obj, (int)obj.ID)}");
    }
}

/***
 * Note: when Windows is configured with an explicit proxy (via Internet Options, PAC file, or WPAD), 
 * applications send all HTTP/HTTPS traffic directly to the proxy server's IP address. 
 * In ETW network events, all destination addresses would appear as the proxy's IP (e.g., 192.168.1.100:8080),
 */
string ConvertToIPAddress(TraceEvent obj, int eventId)
{
    object destValue = obj.PayloadByName("daddr");

    // Event IDs 10-18 and 42-49 are IPv4
    // Event IDs 26-34 and 58-59 are IPv6
    bool isIPv4 = (eventId >= 10 && eventId <= 18) ||
                  (eventId >= 42 && eventId <= 49);

    if (isIPv4)
    {
        // Handle both signed and unsigned integers
        uint ipv4;

        if (destValue is int signedValue)
        {
            // Convert negative signed int to unsigned using unchecked cast
            ipv4 = unchecked((uint)signedValue);
        }
        else
        {
            ipv4 = Convert.ToUInt32(destValue);
        }

        byte[] bytes = BitConverter.GetBytes(ipv4);
        return new IPAddress(bytes).ToString();
    }
    else
    {
        // Try to get the bytes directly from the payload index
        for (int i = 0; i < obj.PayloadNames.Length; i++)
        {
            if (obj.PayloadNames[i] == "daddr")
            {
                object rawValue = obj.PayloadValue(i);

                // Check what type it actually is
                if (rawValue is byte[] bytes && bytes.Length == 16)
                {
                    return new IPAddress(bytes).ToString();
                }
                else
                {
                    // Debug output
                    return $"Type: {rawValue?.GetType()}, Value: {rawValue}";
                }
            }
        }

        return "daddr field not found";
    }

}
