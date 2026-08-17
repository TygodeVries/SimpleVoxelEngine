namespace Shared.Networking;

public abstract class Connection
{
    public abstract bool IsConnected();
    public abstract void ReadPackets(int maxPacketsRead);
    public event Action? OnDisconnect;
    public bool isDisconnected = false;
    public void Disconnect()
    {
        isDisconnected = true;
        OnDisconnect?.Invoke();
    }

    public event Action<Packet>? OnPacket;
    public void ExecutePacket(Packet packet)
    {
        OnPacket?.Invoke(packet);
    }

    public abstract void SendPacket(Packet packet);

    public void SendError(string error)
    {
        ErrorPacket errorPacket = new ErrorPacket();
        errorPacket.Message = error;
        SendPacket(errorPacket.Write());
    }
}
