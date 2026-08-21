using Shared.Networking;
using Shared.Worlds;

namespace Server.Worlds;

public class Player : ServerEntity
{
    public Connection Connection;
    public Player(Connection connection)
    {
        this.Connection = connection;

        connection.OnPacket += OnPlayerPacket;

        connection.OnDisconnect += Connection_OnDisconnect;
    }

    private void Connection_OnDisconnect()
    {
        // Destroy ourselfs on logout
        Destroy();
    }

    private const int ChunkLoadDistance = 3;

    private readonly HashSet<(int X, int Y, int Z)> loadedChunks = new();

    private static int WorldToChunk(float position)
    {
        return (int)MathF.Floor(position / 16f);
    }

    private int currentChunkX;
    private int currentChunkY;

    private int currentChunkZ;

    private void OnPlayerPacket(Packet packet)
    {
        if (packet.GetPacketType() == PacketType.PlayerMove)
        {
            HandlePlayerMove(packet);
        }

        if (packet.GetPacketType() == PacketType.PlaceBlock)
        {
            PlaceBlockPacket placeBlockPacket = new PlaceBlockPacket();
            placeBlockPacket.Read(packet);

            Block? block = Registry.GetBlock(placeBlockPacket.Type); // #TODO_IMP Check if the player actually has this block in their hand.

            if (block == null)
            {
                Console.WriteLine("Player is trying to place unknown block");
                return;
            }

            if (block.id == 0)
            {
                GetWorld().BreakBlock(placeBlockPacket.X, placeBlockPacket.Y, placeBlockPacket.Z);
            }
            else
            {
                GetWorld().SetBlockAt(block, placeBlockPacket.X, placeBlockPacket.Y, placeBlockPacket.Z);
            }

        }
    }

    private void HandlePlayerMove(Packet packet)
    {
        PlayerMovePacket playerMovePacket = new PlayerMovePacket();
        playerMovePacket.Read(packet);

        position.X = playerMovePacket.X;
        position.Y = playerMovePacket.Y;
        position.Z = playerMovePacket.Z;

        Teleport(position);

        int newChunkX = WorldToChunk(position.X);
        int newChunkY = WorldToChunk(position.Y);
        int newChunkZ = WorldToChunk(position.Z);

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
}
