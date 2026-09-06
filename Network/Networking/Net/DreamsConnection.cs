namespace Shared.Networking;

public class DreamsConnection : Connection
{
    public int id { get; private set; }
    public DreamsConnection(int id)
    {
        this.id = id;
    }


    public override bool IsConnected()
    {
        return !isDisconnected;
    }

    public async override Task ReadPacketsLoop()
    {
        // No need to read packets
    }

    public override void HandlePackets()
    {
        // No need to handle packets
    }

    public override void SendPacket(Packet packet)
    {
        try
        {
            OnSendPacket?.Invoke(packet);
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not send packet to dreams user!");
            Disconnect();
        }
    }

    public event Action<Packet>? OnSendPacket = null;
}
