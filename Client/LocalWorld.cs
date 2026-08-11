using Client.Entities;
using Client.Networking;
using Shared.Networking;
using Shared.Worlds;

namespace Client;

public class LocalWorld
{
    public static World World { get; private set; } = new World();

    public static void ResetWorld()
    {
        World = new World();
    }

    public static void ListenForPackets()
    {
        Network.OnPacket += OnPacket;
    }


    private static int localPlayerId = -1;
    private static void OnPacket(Shared.Networking.Packet packet)
    {
        if (packet.GetPacketType() == PacketType.Authenticate)
        {
            AuthenticatePacket authenticatePacket = new AuthenticatePacket();
            authenticatePacket.Read(packet);

            // We don't want to see ourselfs.
            Entity? currentPlayerEntity = World.GetEntityWithId(authenticatePacket.EntityId);
            if (currentPlayerEntity != null)
                World.DestroyEntity(currentPlayerEntity);

            localPlayerId = authenticatePacket.EntityId;
            Console.WriteLine("Local Entity Id is: " + localPlayerId);
            LocalPlayer localPlayer = new LocalPlayer();

            // Fit ourselfs into the empty slot
            World.SpawnEntity(localPlayer, authenticatePacket.EntityId);
        }

        if (packet.GetPacketType() == Shared.Networking.PacketType.SpawnEntity)
        {
            Console.WriteLine("Spawning Entity...");
            SpawnEntityPacket spawnEntityPacket = new SpawnEntityPacket();
            spawnEntityPacket.Read(packet);

            if (spawnEntityPacket.Id == localPlayerId)
            {
                Console.WriteLine("Can not spawn entity with same ID as local player.");
                return;
            }

            Entity entity = EntityFactory.CreateEntity(spawnEntityPacket.Type);
            World.SpawnEntity(entity, spawnEntityPacket.Id);
        }

        if (packet.GetPacketType() == PacketType.MoveEntity)
        {
            MoveEntityPacket moveEntityPacket = new MoveEntityPacket();
            moveEntityPacket.Read(packet);

            // Don't move ourselfs
            if (moveEntityPacket.Id == localPlayerId)
            {
                return;
            }


            Entity? entity = World.GetEntityWithId(moveEntityPacket.Id);
            if (entity == null)
            {
                Console.WriteLine("Invalid entity id for MoveEntityPacket");
                return;
            }

            entity.position.X = moveEntityPacket.X;
            entity.position.Y = moveEntityPacket.Y;
            entity.position.Z = moveEntityPacket.Z;
        }

        if (packet.GetPacketType() == PacketType.DestroyEntity)
        {
            DestroyEntityPacket destroyEntityPacket = new DestroyEntityPacket();
            destroyEntityPacket.Read(packet);

            if (destroyEntityPacket.Id == localPlayerId)
            {
                Console.WriteLine("You have been destroyed!!!");
                return;
            }

            Entity? entity = World.GetEntityWithId(destroyEntityPacket.Id);
            if (entity == null)
            {
                Console.WriteLine("Invalid entity id for DestroyEntityPacket");
                return;
            }

            entity.Destroy();
        }

        if (packet.GetPacketType() == PacketType.ChunkData)
        {
            ChunkDataPacket chunkDataPacket = new ChunkDataPacket();
            chunkDataPacket.Read(packet);

            Chunk chunk = new Chunk(chunkDataPacket.X, chunkDataPacket.Y, chunkDataPacket.Z);
            chunk.SetByteArray(chunkDataPacket.data);
            Console.WriteLine($"Loaded chunk including block of type {chunk.GetBlock(0, 0, 0)}");
            World.AddChunk(chunk);
        }

        if (packet.GetPacketType() == PacketType.PlaceBlock)
        {
            PlaceBlockPacket placeBlockPacket = new PlaceBlockPacket();
            placeBlockPacket.Read(packet);

            World.SetBlockAt(placeBlockPacket.Type, placeBlockPacket.X, placeBlockPacket.Y, placeBlockPacket.Z);
        }
    }
}
