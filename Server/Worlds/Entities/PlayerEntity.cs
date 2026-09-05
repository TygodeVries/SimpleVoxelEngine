using Shared.Mathf;
using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public class PlayerEntity : ServerEntity
{
    public PlayerEntity(Connection connection)
    {
        this.Connection = connection;

        connection.OnPacket += OnPlayerPacket;

        connection.OnDisconnect += Connection_OnDisconnect;

        Inventory.OnSlotSet += Inventory_OnSlotSet;

        OnLeftClick += () =>
        {
            GetItemInHand()?.Type.ExecuteLeftClick(new ItemClickArgs(this));
        };

        OnRightClick += () =>
        {
            GetItemInHand()?.Type.ExecuteRightClick(new ItemClickArgs(this));
        };

        OnLeftClickBlock += (args) =>
        {
            GetItemInHand()?.Type.ExecuteBlockLeftClick(new ItemClickBlockArgs(this, args.Block, args.Normal));
        };

        OnRightClickBlock += (args) =>
        {
            GetItemInHand()?.Type.ExecuteBlockRightClick(new ItemClickBlockArgs(this, args.Block, args.Normal));
        };

        OnSetVelocity += () =>
        {
            SetVelocityPacket packet = new SetVelocityPacket();
            packet.X = Velocity.X;
            packet.Y = Velocity.Y;
            packet.Z = Velocity.Z;
            connection.SendPacket(packet.Write());
        };
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        UpdateChunks();
    }
    /// <summary>
    /// Whenever the player changes the slot they are holding.
    /// </summary>
    public event Action<PlayerChangeSlotArgs>? OnSlotChange;

    /// <summary>
    /// Whenever the player right clicks on a block
    /// </summary>
    public event Action<PlayerClickBlocksArgs>? OnRightClickBlock;

    /// <summary>
    /// Whenever the player left clicks on a block
    /// </summary>
    public event Action<PlayerClickBlocksArgs>? OnLeftClickBlock;


    /// <summary>
    /// Whenever the player *right* clicks, no matter if its on a block, entity or air
    /// </summary>
    public event Action? OnRightClick;

    /// <summary>
    /// Whenever the player *left* clicks, no matter if its on a block, entity or air
    /// </summary>
    public event Action? OnLeftClick;

    /// <summary>
    /// The inventory of the player, where items are stored
    /// </summary>
    public Inventory Inventory { get; private set; } = new Inventory(9);

    /// <summary>
    /// The connection of the player
    /// </summary>
    public Connection Connection { get; private set; }

    /// <summary>
    /// The hotbar slot that the player currently has selected
    /// </summary>
    public int CurrentHotbarSlot { get; private set; }

    /// <summary>
    /// The view distance of the player
    /// </summary>
    private const int ChunkLoadDistance = 8;

    /// <summary>
    /// The lists of chunks that are loaded
    /// </summary>
    private readonly HashSet<(int X, int Y, int Z)> loadedChunks = new();

    public bool IsChunkLoaded(int X, int Y, int Z)
    {
        return loadedChunks.Contains((X, Y, Z));
    }

    // The current chunk we are in
    private int currentChunkX;
    private int currentChunkY;
    private int currentChunkZ;


    /// <summary>
    /// The item the player is currently holding
    /// </summary>
    /// <returns></returns>
    public ItemStack? GetItemInHand()
    {
        return Inventory.GetItem(CurrentHotbarSlot);
    }

    private void Inventory_OnSlotSet(OnSlotSetArgs obj)
    {
        // Whenever the inventory of the player changes, we need to send that to their client.
        InventoryChangePacket inventoryChangePacket = new InventoryChangePacket();
        inventoryChangePacket.itemStack = obj.stack;
        inventoryChangePacket.slot = obj.slot;

        Connection.SendPacket(inventoryChangePacket.Write());
    }

    private void Connection_OnDisconnect()
    {
        // Destroy ourselfs on logout
        Destroy();
    }

    private static int WorldToChunk(float position)
    {
        return (int)MathF.Floor(position / 16f);
    }

    private void OnPlayerPacket(Packet packet)
    {
        if (packet.GetPacketType() == PacketType.PlayerMove)
        {
            HandlePlayerMove(packet);
        }

        if (packet.GetPacketType() == PacketType.PlayerInteract)
        {
            PlayerInteractPacket pip = new PlayerInteractPacket();
            pip.Read(packet);

            if (pip.InteractionType == InteractionType.LeftClickBlock)
            {
                OnLeftClickBlock?.Invoke(new PlayerClickBlocksArgs(pip.BlockPos, pip.BlockNormal));
                OnLeftClick?.Invoke();
            }

            if (pip.InteractionType == InteractionType.RightClickBlock)
            {
                OnRightClickBlock?.Invoke(new PlayerClickBlocksArgs(pip.BlockPos, pip.BlockNormal));
                OnRightClick?.Invoke();
            }

            if (pip.InteractionType == InteractionType.LeftClickAir)
            {
                OnLeftClick?.Invoke();
            }

            if (pip.InteractionType == InteractionType.RightClickAir)
            {
                OnRightClick?.Invoke();
            }
        }

        if (packet.GetPacketType() == PacketType.SelectSlot)
        {
            SelectSlotPacket selectSlotPacket = new SelectSlotPacket();
            selectSlotPacket.Read(packet);
            int oldSlot = CurrentHotbarSlot;
            CurrentHotbarSlot = selectSlotPacket.Slot;

            OnSlotChange?.Invoke(new PlayerChangeSlotArgs(oldSlot, CurrentHotbarSlot));
        }
    }

    public override void Teleport(Vector3 position)
    {
        base.Teleport(position);
        PlayerMovePacket playerMove = new PlayerMovePacket();
        playerMove.X = position.X;
        playerMove.Y = position.Y;
        playerMove.Z = position.Z;

        Connection.SendPacket(playerMove.Write());
    }


    private void HandlePlayerMove(Packet packet)
    {
        PlayerMovePacket playerMovePacket = new PlayerMovePacket();
        playerMovePacket.Read(packet);

        base.Teleport(new Vector3(playerMovePacket.X, playerMovePacket.Y, playerMovePacket.Z));

        int newChunkX = WorldToChunk(Position.X);
        int newChunkY = WorldToChunk(Position.Y);
        int newChunkZ = WorldToChunk(Position.Z);

        if (newChunkX == currentChunkX &&
            newChunkY == currentChunkY &&
            newChunkZ == currentChunkZ)
        {
            return;
        }

        currentChunkX = newChunkX;
        currentChunkY = newChunkY;
        currentChunkZ = newChunkZ;

        UpdateChunks();
    }

    private void UpdateChunks()
    {
        HashSet<(int X, int Y, int Z)> wantedChunks = new();

        for (int x = currentChunkX - ChunkLoadDistance; x <= currentChunkX + ChunkLoadDistance; x++)
        {
            for (int y = currentChunkY - ChunkLoadDistance; y <= currentChunkY + ChunkLoadDistance; y++)
            {
                for (int z = currentChunkZ - ChunkLoadDistance; z <= currentChunkZ + ChunkLoadDistance; z++)
                {
                    wantedChunks.Add((x, y, z));
                }
            }
        }

        foreach (var chunk in loadedChunks)
        {
            if (!wantedChunks.Contains(chunk))
            {
                SendUnloadChunk(
                    chunk.X,
                    chunk.Y,
                    chunk.Z
                );
            }
        }

        foreach (var chunk in wantedChunks)
        {
            if (!loadedChunks.Contains(chunk))
            {
                SendLoadChunk(
                    chunk.X,
                    chunk.Y,
                    chunk.Z
                );
            }
        }

        loadedChunks.Clear();

        foreach (var chunk in wantedChunks)
        {
            loadedChunks.Add(chunk);
        }
    }

    private void SendUnloadChunk(int chunkX, int chunkY, int chunkZ)
    {
        UnloadChunkPacket unloadChunkPacket = new UnloadChunkPacket();
        unloadChunkPacket.X = chunkX;
        unloadChunkPacket.Y = chunkY;
        unloadChunkPacket.Z = chunkZ;
        Connection.SendPacket(unloadChunkPacket.Write());
    }

    private void SendLoadChunk(int chunkX, int chunkY, int chunkZ)
    {
        World world = GetWorld();

        Chunk chunk = world.GetOrGenerateChunkAt(
            chunkX,
            chunkY,
            chunkZ
        );

        // Before sending anything, make sure its as compressed as possible!
        chunk.Optimize();

        ChunkDataPacket packet = new ChunkDataPacket();

        packet.X = chunkX;
        packet.Y = chunkY;
        packet.Z = chunkZ;
        packet.data = chunk.GetByteArray();

        Connection.SendPacket(packet.Write());
    }

    public override EntityType GetEntityType()
    {
        return EntityType.Player;
    }

    public event Action<PlaySoundArgs>? OnSoundPlay;

    public void PlaySound(string sound)
    {
        OnSoundPlay?.Invoke(new PlaySoundArgs()
        {
            Sound = sound,
            IsGlobal = true
        });

        PlaySoundPacket playSoundPacket = new PlaySoundPacket();
        playSoundPacket.Sound = sound;
        playSoundPacket.IsGlobal = true;

        Connection.SendPacket(playSoundPacket.Write());
    }

    public void PlaySound(string sound, Vector3 position, float volume = 1, float referenceDistance = 1.0f, float maxDistance = 50.0f, float rolloffFactor = 1.0f)
    {
        OnSoundPlay?.Invoke(new PlaySoundArgs()
        {
            Sound = sound,
            IsGlobal = false,
            Volume = volume,
            Position = position,
            MaxDistance = maxDistance,
            ReferenceDistance = referenceDistance,
            RolloffFactor = rolloffFactor
        });

        PlaySoundPacket playSoundPacket = new PlaySoundPacket()
        {
            Sound = sound,
            IsGlobal = false,
            Volume = volume,
            Position = position,
            MaxDistance = maxDistance,
            ReferenceDistance = referenceDistance,
            RolloffFactor = rolloffFactor
        };

        Connection.SendPacket(playSoundPacket.Write());
    }
}
