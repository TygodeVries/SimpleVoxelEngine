namespace Shared.Networking;

public abstract class PacketWriter
{
    public abstract PacketType WriterType();
    public abstract Packet Write();
    public abstract void Read(Packet packet);
}
