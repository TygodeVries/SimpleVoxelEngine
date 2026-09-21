using System.Collections.Concurrent;
using System.Net.Sockets;
namespace Shared.Networking;

public class TcpConnection : Connection
{
    public bool isDreamsAuthorizedServer;
    public override bool IsConnected()
    {
        return client.Connected;
    }

    private const int MaxPacketSize = int.MaxValue;
    private ConcurrentQueue<byte[]> pendingPackets = new ConcurrentQueue<byte[]>();
    private readonly CancellationTokenSource readCancellation = new();

    public void Stop()
    {
        if (!readCancellation.IsCancellationRequested)
        {
            readCancellation.Cancel();
        }
    }

    public async override Task ReadPacketsLoop()
    {
        await Task.Delay(500);

        try
        {
            NetworkStream stream = client.GetStream();

            while (IsConnected() && !readCancellation.Token.IsCancellationRequested)
            {
                byte[] sizeHeader = new byte[4];

                int bytesRead = await stream.ReadAtLeastAsync(
                    sizeHeader,
                    4,
                    throwOnEndOfStream: false,
                    cancellationToken: readCancellation.Token
                );

                if (bytesRead < 4)
                {
                    Disconnect();
                    break;
                }

                int packetSize = BitConverter.ToInt32(sizeHeader, 0);

                byte[] payload = new byte[packetSize];
                await stream.ReadExactlyAsync(payload, readCancellation.Token);

                pendingPackets.Enqueue(payload);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when Stop() is called.
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"Error reading packets from client: {e}"
            );

            Disconnect();
        }
    }

    public override void HandlePackets()
    {
        while (pendingPackets.TryDequeue(out byte[]? packet))
        {
            if (packet == null)
                continue;

            HandlePacket(packet);
        }
    }

    private void HandlePacket(byte[] packetContent)
    {
        PacketType type = (PacketType)packetContent[0];

        byte[] data = new byte[packetContent.Length - 1];
        Array.Copy(packetContent, 1, data, 0, data.Length);

        Packet packet = new Packet(type, data);
        ExecutePacket(packet);
    }


    public override void SendPacket(Packet packet)
    {

        if (isDisconnected)
            return;

        try
        {
            byte[] bytes = packet.GetBytes();
            int size = bytes.Length;

            byte[] sizeBytes = BitConverter.GetBytes(size);
            client.GetStream().Write(sizeBytes, 0, sizeBytes.Length);
            client.GetStream().Write(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to send a packet.");
            Disconnect();
        }
    }


    public TcpClient client;

    public TcpConnection(TcpClient client)
    {
        this.client = client;
        this.OnDisconnect += Stop;
    }
}
