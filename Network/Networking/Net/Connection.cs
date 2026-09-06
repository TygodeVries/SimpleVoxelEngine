namespace Shared.Networking;

public abstract class Connection
{
    public abstract bool IsConnected();
    public abstract Task ReadPacketsLoop();
    public abstract void HandlePackets();
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
        packet.ResetRead();
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
