namespace Shared.Networking;

public enum PacketType : byte
{
    Authenticate = 0,
    ChunkData = 1,
    PlayerMove = 2,
    SpawnEntity = 3,
    DestroyEntity = 4,
    MoveEntity = 5,
    SetBlock = 6,
    BreakBlock = 7,
    DreamsJoin = 8,
    DreamsServerInfo = 9,
    DreamsAddUser = 10,
    DreamsPacketData = 11,
    Error = 12,
    DreamsRemoveUser = 13,
    DreamsServerList = 14,
    UnloadChunk = 15,
    Texturepack = 16,
    RegistryData = 17,
    SelectSlot = 18,
    PlayerInteract = 19,
    InventoryChange = 20,
    SetVelocity = 21,
    PlaySound = 22
}
