namespace Shared.Networking;

public enum PacketType : byte
{
    Authenticate = 0,
    ChunkData = 1,
    PlayerMove = 2,
    SpawnEntity = 3,
    DestroyEntity = 4,
    MoveEntity = 5,
    PlaceBlock = 6,
    BreakBlock = 7
}
