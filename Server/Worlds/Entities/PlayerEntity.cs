using Shared.Mathf;
using Shared.Networking;
using Shared.Networking.Packets.Writers;
using Shared.Worlds;
using static Shared.Worlds.Chunk;

namespace Server.Worlds;

public class PlayerEntity : ServerEntity
{
    public event Action<ItemClickBlockArgs>? OnLeftClickEmptyHand;

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
            var itemClickArgs = new ItemClickBlockArgs(this, args.Block, args.Normal);
            var itemInHand = GetItemInHand();


            itemInHand?.Type.ExecuteBlockLeftClick(itemClickArgs);
            if (itemInHand == null)
                OnLeftClickEmptyHand?.Invoke(itemClickArgs);
        };

        OnRightClickBlock += (args) =>
        {
            GetItemInHand()?.Type.ExecuteBlockRightClick(new ItemClickBlockArgs(this, args.Block, args.Normal));
        };

        OnCommand += (command) =>
        {
            if (command.ToLower() == "reload")
            {
                Program.Restart();
            }
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
    private const int ChunkLoadDistance = 20;

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

    public void SetItemInMainHand(ItemStack? itemStack)
    {
        Inventory.SetSlot(CurrentHotbarSlot, itemStack);
    }

    public void AddCountInMainHand(int amount)
    {
        ItemStack? stack = GetItemInHand();

        if (stack == null)
            return;

        stack.Count += amount;
        if (stack.Count == 0)
        {
            Console.WriteLine("Item ran out!");
            SetItemInMainHand(null);
            return;
        }

        Console.WriteLine($"Item count is now {stack.Count}");
        SetItemInMainHand(stack);
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

    public event Action<string>? OnCommand;

    public void RunCommand(string command)
    {
        OnCommand?.Invoke(command);
    }

    /// <summary>
    /// #TODO Implement this!
    /// </summary>
    /// <returns></returns>
    public Vector3 GetForward()
    {
        return Vector3.Forwards;
    }

    public void DropItem()
    {
        ItemStack? hand = GetItemInHand();
        if (hand == null)
            return;

        ItemStack drop = new ItemStack(hand.Type, 1);
        GetWorld()?.DropItem(GetEye(), drop, (GetForward() * 6) + new Vector3(0, 6, 0));
        AddCountInMainHand(-1);
    }

    public Vector3 GetEye()
    {
        return Position + new Vector3(0, 1, 0);
    }

    private void OnPlayerPacket(Packet packet)
    {
        if (packet.GetPacketType() == PacketType.PlayerMove)
        {
            HandlePlayerMove(packet);
        }

        if (packet.GetPacketType() == PacketType.DropItem)
        {
            DropItem();
        }

        if (packet.GetPacketType() == PacketType.Command)
        {
            CommandPacket commandPacket = new CommandPacket();
            commandPacket.Read(packet);
            RunCommand(commandPacket.Command);
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

            if (pip.InteractionType == InteractionType.ReleaseLeftMouse)
            {
                StopBreakingBlock();
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

    public void StartBreakingBlock(Vector3 block)
    {
        BlockBreakProgress = 0;
        IsBreakingBlock = true;
        targetBreakBlock = block;
        lastBreakStage = -1;

        BlockBreakProgressPacket packet = new BlockBreakProgressPacket();
        packet.position = block;
        packet.stage = 0;

        Program.server.BroadcastPacket(packet.Write());
    }


    public void StopBreakingBlock()
    {

        if (targetBreakBlock != null)
        {
            BlockBreakProgressPacket blockBreakProgressPacket = new BlockBreakProgressPacket();
            blockBreakProgressPacket.position = targetBreakBlock.Value;
            blockBreakProgressPacket.stage = -1;

            Program.server.BroadcastPacket(blockBreakProgressPacket.Write());
        }
        BlockBreakProgress = 0;
        IsBreakingBlock = false;
        targetBreakBlock = null;
    }

    public override void Tick()
    {
        base.Tick();

        if (!IsBreakingBlock)
            return;

        BlockBreakProgress += Time.DeltaTime;

        if (targetBreakBlock == null)
        {
            StopBreakingBlock();
            return;
        }

        var world = GetWorld();
        if (world == null)
        {
            StopBreakingBlock();
            return;
        }

        var block = world.GetBlockAt(targetBreakBlock.Value);
        float hardness = block.Hardness;

        if (hardness <= 0)
        {
            world.BreakBlock(targetBreakBlock.Value);
            StopBreakingBlock();
            return;
        }

        float progress = BlockBreakProgress / hardness;

        int stage = Math.Clamp((int)(progress * 4f), 0, 3);

        if (stage != lastBreakStage)
        {
            lastBreakStage = stage;

            BlockBreakProgressPacket packet = new BlockBreakProgressPacket
            {
                position = targetBreakBlock.Value,
                stage = stage
            };

            Program.server.BroadcastPacket(packet.Write());
        }

        if (progress >= 1f)
        {
            world.BreakBlock(targetBreakBlock.Value);
            StopBreakingBlock();
        }

        // #TODO IMPORTANT: DON"T DO THIS
        UpdateChunks();
    }
    private int lastBreakStage = -1;


    private float BlockBreakProgress = 0;
    public bool IsBreakingBlock { get; private set; } = false;
    private Vector3? targetBreakBlock;

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
        World world = GetWorld();

        int centerX = currentChunkX;
        int centerY = currentChunkY;
        int centerZ = currentChunkZ;
        int distance = ChunkLoadDistance;

        var wantedChunks = new HashSet<(int X, int Y, int Z)>();
        var queue = new Queue<(int X, int Y, int Z)>();

        var start = (centerX, centerY, centerZ);

        wantedChunks.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            Chunk currentChunk = world.GetOrGenerateChunkAt(
                current.X,
                current.Y,
                current.Z
            );

            bool isStart =
                current.X == centerX &&
                current.Y == centerY &&
                current.Z == centerZ;

            foreach (var direction in ChunkDirections)
            {
                int nextX = current.X + direction.X;
                int nextY = current.Y + direction.Y;
                int nextZ = current.Z + direction.Z;

                // Distance check first.
                if (Math.Abs(nextX - centerX) > distance ||
                    Math.Abs(nextY - centerY) > distance ||
                    Math.Abs(nextZ - centerZ) > distance)
                {
                    continue;
                }

                var next = (nextX, nextY, nextZ);

                // Already visited.
                if (!wantedChunks.Add(next))
                    continue;

                // The starting chunk can always expand.
                // Other chunks require a passable connection.
                if (!isStart &&
                    !currentChunk.CanPass(
                        direction.Direction,
                        direction.Opposite))
                {
                    // We added it above, so remove it again.
                    wantedChunks.Remove(next);
                    continue;
                }

                queue.Enqueue(next);
            }
        }

        // Unload chunks that are no longer wanted.
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

        // Load newly wanted chunks.
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

        // Replace the loaded set.
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
        return Defaults.PlayerEntity;
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

    public void SendToast(string message, float displayTime = 7)
    {
        ToastPacket toastPacket = new ToastPacket();
        toastPacket.msg = message;
        toastPacket.time = displayTime;
        Connection.SendPacket(toastPacket.Write());
    }

    private static readonly (int X, int Y, int Z, Direction Direction, Direction Opposite)[] ChunkDirections =
    {
        (-1,  0,  0, Direction.Left,    Direction.Right),
        ( 1,  0,  0, Direction.Right,   Direction.Left),
        ( 0, -1,  0, Direction.Down,    Direction.Up),
        ( 0,  1,  0, Direction.Up,      Direction.Down),
        ( 0,  0, -1, Direction.Back,    Direction.Forward),
        ( 0,  0,  1, Direction.Forward, Direction.Back)
    };

}
