using Client.Entities;
using Client.Networking;
using Client.Rendering;
using Client.Sound;
using Shared.Networking;
using Shared.Worlds;

namespace Client;

// #TODO some packets should NOT be in here.
public class LocalWorld
{
    public static World World { get; private set; } = new World();

    public static void ResetWorld()
    {
        while (World.GetChunks().Count > 0)
        {
            World.RemoveChunk(World.GetChunks().First());
        }

        while (World.GetEntities().Count > 0)
        {
            World.DestroyEntity(World.GetEntities().First());
            World.Tick();
        }


        SoundPlayer.Reset();
        World = new World();
        localPlayerId = -1;

        Registry.OnBlockRegister -= Registry_OnBlockRegister;
        LocalWorld.World.OnAddChunk -= GameCanvas.canvas.World_OnAddChunk;
        LocalWorld.World.OnRemoveChunk -= GameCanvas.canvas.World_OnRemoveChunk;
        Network.OnPacket -= OnPacket;

        LocalInventory.Clear();

        Startup();
    }

    public static void Regenerate()
    {
        foreach (Chunk chunk in World.GetChunks())
        {
            chunk.isDirty = true;
        }
    }

    public static void Startup()
    {
        Registry.OnBlockRegister += Registry_OnBlockRegister;
        LocalWorld.World.OnAddChunk += GameCanvas.canvas.World_OnAddChunk;
        LocalWorld.World.OnRemoveChunk += GameCanvas.canvas.World_OnRemoveChunk;
        Network.OnPacket += OnPacket;
    }

    private static void Registry_OnBlockRegister(Block obj)
    {
        BlockTexture? blockTexture = obj.Texture;
        if (blockTexture == null)
            return;

        RenderData.BlockTexturesMap.AddMapping(blockTexture.Up, obj.RegistryId, BlockFace.Up);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Down, obj.RegistryId, BlockFace.Down);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Left, obj.RegistryId, BlockFace.Left);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Right, obj.RegistryId, BlockFace.Right);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Forward, obj.RegistryId, BlockFace.Forward);
        RenderData.BlockTexturesMap.AddMapping(blockTexture.Backward, obj.RegistryId, BlockFace.Backward);

        RenderData.SingleChunkShader.SetVector4("u_TextureInfo", new OpenTK.Mathematics.Vector4(RenderData.BlockTexturesMap.row, RenderData.BlockTexturesMap.col, 16, 0));
    }

    private static int localPlayerId = -1;
    private static void OnPacket(Shared.Networking.Packet packet)
    {
        if (packet.GetPacketType() == PacketType.RegistryData)
        {
            RegistryDataPacket regData = new RegistryDataPacket();
            regData.Read(packet);

            Registry.LoadAll(regData.Data);
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

            if (Program.Version != authenticatePacket.ServerVersion)
            {
                throw new Exception($"Versions do not match. Server is on version {authenticatePacket.ServerVersion} but client is on version {Program.Version}.");
            }

            // We don't want to see ourselfs.
            Entity? currentPlayerEntity = World.GetEntityWithId(authenticatePacket.EntityId);
            if (currentPlayerEntity != null)
                World.DestroyEntity(currentPlayerEntity);

            localPlayerId = authenticatePacket.EntityId;
            Console.WriteLine("Local Entity RegistryId is: " + localPlayerId);
            LocalPlayer localPlayer = new LocalPlayer();

            // Fit ourselfs into the empty slot
            World.SpawnEntity(localPlayer, authenticatePacket.EntityId);
            World.Tick();
        }

        if (packet.GetPacketType() == Shared.Networking.PacketType.SpawnEntity)
        {
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
                Console.WriteLine("Invalid entity RegistryId for MoveEntityPacket");
                return;
            }

            entity.Teleport(moveEntityPacket.X, moveEntityPacket.Y, moveEntityPacket.Z);
            LocalWorld.World.Tick();
        }

        if (packet.GetPacketType() == PacketType.SetVelocity)
        {
            SetVelocityPacket velocityPacket = new SetVelocityPacket();
            velocityPacket.Read(packet);

            Entity? currentPlayerEntity = World.GetEntityWithId(localPlayerId);
            if (currentPlayerEntity == null)
            {
                World.PrintEntityDump();
                throw new NullReferenceException("No local player entity could be found!");
            }

            currentPlayerEntity.SetVelocity(velocityPacket.X, velocityPacket.Y, velocityPacket.Z);
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
                Console.WriteLine("Invalid entity RegistryId for DestroyEntityPacket");
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

        if (packet.GetPacketType() == PacketType.SetBlock)
        {
            SetBlockPacket placeBlockPacket = new SetBlockPacket();
            placeBlockPacket.Read(packet);

            Block? block = Registry.GetBlock(placeBlockPacket.Type);
            if (block == null)
                throw new Exception($"Invalid block type send by server. We don't know what {placeBlockPacket.Type} is!!");
            World.SetBlockAt(block, placeBlockPacket.X, placeBlockPacket.Y, placeBlockPacket.Z);
        }

        if (packet.GetPacketType() == PacketType.ResourcePack)
        {
            ResourcePackPacket resourcepack = new ResourcePackPacket();
            resourcepack.Read(packet);
            Console.WriteLine("Resourcepack.");
            if (resourcepack.resourceType == ResourceType.BLOCKS_TEXTURES)
            {
                RenderData.SetBlockTexture(resourcepack.names, ImageTexture.LoadFromBytes(resourcepack.resourceData));
            }
            else if (resourcepack.resourceType == ResourceType.ITEMS_TEXTURES)
            {
                RenderData.SetItemTexture(resourcepack.names, ImageTexture.LoadFromBytes(resourcepack.resourceData));
            }
            else if (resourcepack.resourceType == ResourceType.SOUND)
            {
                Console.WriteLine("Got sound resource!");
                SoundPlayer.AddAudioResource(resourcepack.names, resourcepack.resourceData);
            }

        }

        if (packet.GetPacketType() == PacketType.InventoryChange)
        {
            InventoryChangePacket icp = new InventoryChangePacket();
            icp.Read(packet);


            LocalInventory.SetItem(icp.slot, icp.itemStack);
        }

        if (packet.GetPacketType() == PacketType.PlayerMove)
        {
            Entity? currentPlayerEntity = World.GetEntityWithId(localPlayerId);
            if (currentPlayerEntity == null)
            {
                World.PrintEntityDump();
                throw new NullReferenceException("No local player entity could be found!");
            }

            PlayerMovePacket playerMove = new PlayerMovePacket();

            currentPlayerEntity.Teleport(playerMove.X, playerMove.Y, playerMove.Z);
        }

        if (packet.GetPacketType() == PacketType.PlaySound)
        {
            PlaySoundPacket playSoundPacket = new PlaySoundPacket();
            playSoundPacket.Read(packet);

            if (playSoundPacket.IsGlobal)
            {
                SoundPlayer.PlayAudioGlobal(playSoundPacket.Sound);
            }
            else
            {
                SoundPlayer.PlayAudioAtPosition(playSoundPacket.Sound, playSoundPacket.Position, playSoundPacket.Volume, playSoundPacket.ReferenceDistance, playSoundPacket.MaxDistance, playSoundPacket.RolloffFactor);
            }
        }
    }
}
