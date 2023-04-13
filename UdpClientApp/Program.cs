using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

const int LOCAL_PORT = 8080;
const string BRODCAST_ADDRESS = "235.2.3.4";
IPAddress brodcastAddress = IPAddress.Parse(BRODCAST_ADDRESS);
Console.Write("Enter name: ");
string? username = Console.ReadLine();

// only Windows
const uint IOC_IN = 0x80000000U;
const uint IOC_VENDOR = 0x18000000U;
const int SIO_UDP_CONNRESET = unchecked((int)(IOC_IN | IOC_VENDOR | 12));

_ = Task.Run(ReceiveMessageAsync);
await SendMessageAsync();

async Task SendMessageAsync()
{
    using UdpClient clientSender = new();
    // only Windows
    //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //{
    //    clientSender.Client.IOControl(SIO_UDP_CONNRESET, new byte[32], null);
    //}

    while (true)
	{
		string? message = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(message))
		{
			break;
		}
		message = $"{username}: {message}";
		byte[] data = Encoding.UTF8.GetBytes(message);

		await clientSender.SendAsync(data, new IPEndPoint(brodcastAddress, LOCAL_PORT));
	}
}

async Task ReceiveMessageAsync()
{
	using UdpClient clientReceiver = new(LOCAL_PORT);
	// only Windows
	if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
	{
        clientReceiver.Client.IOControl(SIO_UDP_CONNRESET, new byte[] {0x00}, null);
    }
	
	clientReceiver.JoinMulticastGroup(brodcastAddress);
	clientReceiver.MulticastLoopback = false;

	while (true)
	{
		var result = await clientReceiver.ReceiveAsync();
		string message = Encoding.UTF8.GetString(result.Buffer);
		Console.WriteLine(message);
	}
}