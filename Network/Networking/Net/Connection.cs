using System.Net.Sockets;
namespace Shared.Networking;

public class Connection
{

    private bool kicked = false;
    public bool IsConnected()
    {
        return client.Connected && !kicked;
    }


    private int expectedSize = -1;

    public void ReadPackets(int maxPacketsRead)
    {
        if (!IsConnected())
        {
            return;
        }

        if (maxPacketsRead == 0)
        {
            Console.WriteLine("Max packets read hit.");
            return;
        }

        try
        {
            if (expectedSize == -1)
            {
                if (client.Available > 4)
                {
                    byte[] sizeHeader = new byte[4];

                    client.GetStream().ReadExactly(sizeHeader, 0, 4);

                    expectedSize = BitConverter.ToInt32(sizeHeader);
                }
            }

            if (expectedSize != -1 && client.Available >= expectedSize)
            {
                byte[] packet = new byte[expectedSize];

                client.GetStream().ReadExactly(packet, 0, expectedSize);
                expectedSize = -1;

                HandlePacket(packet);

                // Read another if possible.
                ReadPackets(maxPacketsRead - 1);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error reading packets from client: " + e);
            kicked = true;
        }
    }

    private void HandlePacket(byte[] packetContent)
    {
        PacketType type = (PacketType)packetContent[0];

        byte[] data = new byte[packetContent.Length - 1];
        Array.Copy(packetContent, 1, data, 0, data.Length);

        Packet packet = new Packet(type, data);
        OnPacket?.Invoke(packet);
    }

    public void SendPacket(Packet packet)
    {
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
            kicked = true;
        }
    }
    public event Action<Packet>? OnPacket;

    public TcpClient client;

    public Connection(TcpClient client)
    {
        this.client = client;
    }
}
