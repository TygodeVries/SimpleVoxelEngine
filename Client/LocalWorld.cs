using Client.Entities;
using Client.Networking;
using Client.Rendering;
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

    public static void Regenerate()
    {
        foreach (Chunk chunk in World.GetChunks())
        {
            chunk.isDirty = true;
        }
    }

    public static void ListenForPackets()
    {
        Network.OnPacket += OnPacket;

        Registry.OnBlockRegister += Registry_OnBlockRegister;
    }

    private static void Registry_OnBlockRegister(Block obj)
    {
        BlockTexture? blockTexture = obj.texture;
        if (blockTexture == null)
            return;

        RenderData.BlockTexturesMap.AddMapping(blockTexture.Up, obj.id, BlockFace.Up);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Down, obj.id, BlockFace.Down);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Left, obj.id, BlockFace.Left);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Right, obj.id, BlockFace.Right);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Forward, obj.id, BlockFace.Forward);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Backward, obj.id, BlockFace.Backward);
    }

    private static int localPlayerId = -1;
    private static void OnPacket(Shared.Networking.Packet packet)
    {
        if (packet.GetPacketType() == PacketType.BlockData)
        {
            BlockDataPacket blockData = new BlockDataPacket();
            blockData.Read(packet);

            Registry.LoadAll(blockData.BlockData);
        }

        if (packet.GetPacketType() == PacketType.Error)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" ----  Oopsies! Something went very wrong!  ---- ");
            ErrorPacket errorPacket = new ErrorPacket();
            errorPacket.Read(packet);

            Console.WriteLine($"The following was reported: {errorPacket.Message}");
            Program.HasCrashed = true;
            GameCanvas.ForceClose();
        }

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
            World.AddChunk(chunk);
        }

        if (packet.GetPacketType() == PacketType.UnloadChunk)
        {
            UnloadChunkPacket unloadChunkPacket = new UnloadChunkPacket();
            unloadChunkPacket.Read(packet);
            Chunk chunk = World.GetOrGenerateChunkAt(unloadChunkPacket.X, unloadChunkPacket.Y, unloadChunkPacket.Z);
            World.RemoveChunk(chunk);
        }

        if (packet.GetPacketType() == PacketType.PlaceBlock)
        {
            PlaceBlockPacket placeBlockPacket = new PlaceBlockPacket();
            placeBlockPacket.Read(packet);

            Block? block = Registry.GetBlock(placeBlockPacket.Type);
            if (block == null)
                throw new Exception($"Invalid block type send by server. We don't know what {placeBlockPacket.Type} is!!");
            World.SetBlockAt(block, placeBlockPacket.X, placeBlockPacket.Y, placeBlockPacket.Z);
        }

        if (packet.GetPacketType() == PacketType.Texturepack)
        {
            TexturepackPacket texturepackPacket = new TexturepackPacket();
            texturepackPacket.Read(packet);
            BlockTextureAtlas.textureNames = texturepackPacket.names;
            RenderData.SetBlockTexture(ImageTexture.LoadFromBytes(texturepackPacket.textureData));
        }
    }
}
