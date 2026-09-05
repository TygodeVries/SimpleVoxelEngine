using Shared.Mathf;

namespace Shared.Networking;

public class PlaySoundPacket : PacketWriter
{
    /// <summary>
    /// If the sound can be heard anywhere
    /// </summary>
    public bool IsGlobal;


    public float Volume;
    public float ReferenceDistance;
    public float MaxDistance;
    public float RolloffFactor;
    public Vector3 Position;

    public string Sound = "";

    public override Packet Write()
    {
        Packet packet = new Packet(WriterType());

        packet.WriteBool(IsGlobal);
        packet.WriteFloat(Volume);
        packet.WriteFloat(ReferenceDistance);
        packet.WriteFloat(MaxDistance);
        packet.WriteFloat(RolloffFactor);
        packet.WriteVector3(Position);
        packet.WriteString(Sound);
        return packet;
    }

    public override void Read(Packet packet)
    {
        IsGlobal = packet.ReadBool();
        Volume = packet.ReadFloat();
        ReferenceDistance = packet.ReadFloat();
        MaxDistance = packet.ReadFloat();
        RolloffFactor = packet.ReadFloat();
        Position = packet.ReadVector3();
        Sound = packet.ReadString();
    }

    public override PacketType WriterType()
    {
        return PacketType.PlaySound;
    }
}
