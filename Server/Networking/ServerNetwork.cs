using Server.Plugins;
using Server.Worlds;
using Shared;
using Shared.Mathf;
using Shared.Networking;
using Shared.Worlds;
using Spectre.Console;
using System.Net.Sockets;

namespace Server.Networking;

public class ServerNetwork
{
    private List<Connection> connections = new List<Connection>();
    private List<Connection> futureConnections = new List<Connection>();
    private TcpListener? listener;

    public void Start(int port)
    {
        // Start the classic TCP Listener...
        listener = new TcpListener(System.Net.IPAddress.Any, port);
        listener.Start();
    }

    public bool StartDreams()
    {
        // If Dreams is enabled
        try
        {
            TcpClient client = new TcpClient(Dreams.DREAMS_IP, Dreams.DREAMS_PORT);
            TcpConnection tcpConnection = new TcpConnection(client);

            // Tell them we are a server
            tcpConnection.SendPacket(new DreamsServerInfoPacket().Write());
            _ = tcpConnection.ReadPacketsLoop();
            SetupDreams(tcpConnection);
            connections.Add(tcpConnection);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private TcpConnection? dreamsServerConnection;
    private void SetupDreams(TcpConnection dreamsConnection)
    {
        this.dreamsServerConnection = dreamsConnection;
        dreamsConnection.isDreamsAuthorizedServer = true;

        dreamsConnection.OnPacket += DreamsConnection_OnPacket;
    }

    private void DreamsConnection_OnPacket(Packet packet)
    {
        if (packet.GetPacketType() == PacketType.DreamsServerInfo)
        {
            AnsiConsole.MarkupLine($"[yellow](!)[/][white] Dreams Server Address: {packet.ReadString()}[/]");
        }

        if (packet.GetPacketType() == PacketType.DreamsAddUser)
        {
            DreamsAddUserPacket dreamsAddUserPacket = new DreamsAddUserPacket();
            dreamsAddUserPacket.Read(packet);

            DreamsConnection dreamsConnection = new DreamsConnection(dreamsAddUserPacket.id);
            futureConnections.Add(dreamsConnection);
            dreamsConnections.Add(dreamsConnection.id, dreamsConnection);
            AnsiConsole.MarkupLine("[green](+) [/][white]A user connected via Dreams.[/]");
            // When a connection wants to send a packet, we need to pass it to dreams instead.
            dreamsConnection.OnSendPacket += (Packet packet) =>
            {
                DreamsPacketDataPacket dreamsPacketDataPacket = new DreamsPacketDataPacket();
                dreamsPacketDataPacket.packet = packet;
                dreamsPacketDataPacket.owner = dreamsConnection.id;

                dreamsServerConnection?.SendPacket(dreamsPacketDataPacket.Write());
            };


            CatchupConnection(dreamsConnection);
        }

        if (packet.GetPacketType() == PacketType.DreamsPacketData)
        {
            DreamsPacketDataPacket dreamsPacketDataPacket = new DreamsPacketDataPacket();
            dreamsPacketDataPacket.Read(packet);

            dreamsConnections[dreamsPacketDataPacket.owner].ExecutePacket(dreamsPacketDataPacket.packet);
        }

        if (packet.GetPacketType() == PacketType.DreamsRemoveUser)
        {
            DreamsRemoveUserPacket removePacket = new DreamsRemoveUserPacket();
            removePacket.Read(packet);


            AnsiConsole.MarkupLine("[red](-) [/][white]A user disconnected via Dreams.[/]");
            DreamsConnection connection = dreamsConnections[removePacket.id];
            dreamsConnections.Remove(removePacket.id);

            connection.Disconnect();
        }
    }

    private Dictionary<int, DreamsConnection> dreamsConnections = new Dictionary<int, DreamsConnection>();

    public void AcceptTcpServerConnections()
    {
        if (listener == null)
            throw new Exception("ServerNetwork has to be started before you can accept pending clients.");

        if (listener.Pending())
        {
            Console.WriteLine("Pending Connection!");
            TcpClient client = listener.AcceptTcpClient();
            Connection connection = new TcpConnection(client);
            connections.Add(connection);
            CatchupConnection(connection);
        }
    }

    public static byte[] GetModelData(Mesh[] meshes)
    {
        if (meshes == null || meshes.Length == 0) return new byte[0];

        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            foreach (Mesh mesh in meshes)
            {
                if (mesh == null) continue;

                if (mesh.vertices != null && mesh.vertices.Length > 0)
                {
                    writer.Write(mesh.vertices.Length);
                    foreach (float v in mesh.vertices)
                    {
                        writer.Write(v);
                    }
                }
                else
                {
                    writer.Write(0);
                }

                if (mesh.indices != null && mesh.indices.Length > 0)
                {
                    writer.Write(mesh.indices.Length);
                    foreach (uint idx in mesh.indices)
                    {
                        writer.Write(idx);
                    }
                }
                else
                {
                    writer.Write(0);
                }

                if (mesh.normals != null && mesh.normals.Length > 0)
                {
                    writer.Write(mesh.normals.Length);
                    foreach (float n in mesh.normals)
                    {
                        writer.Write(n);
                    }
                }
                else
                {
                    writer.Write(-1);
                }

                if (mesh.uvs != null && mesh.uvs.Length > 0)
                {
                    writer.Write(mesh.uvs.Length);
                    foreach (float uv in mesh.uvs)
                    {
                        writer.Write(uv);
                    }
                }
                else
                {
                    writer.Write(-1);
                }
            }

            return stream.ToArray();
        }
    }

    public void CatchupConnection(Connection connection)
    {
        connection.ReadPacketsLoop();

        ResourcePackPacket modelPackPacket = new ResourcePackPacket();
        modelPackPacket.resourceType = ResourceType.MODELS;

        // Load the names of the models
        modelPackPacket.names = new List<string>();

        List<Mesh> meshes = new List<Mesh>();
        foreach (var v in PluginLoader.modelBuilder)
        {
            modelPackPacket.names.Add(v.Item1);
            meshes.Add(v.Item2);
        }

        modelPackPacket.resourceData = GetModelData(meshes.ToArray());
        connection.SendPacket(modelPackPacket.Write());



        // First send the textures
        ResourcePackPacket texturepackPacket = new ResourcePackPacket();
        texturepackPacket.resourceType = ResourceType.BLOCKS_TEXTURES;
        texturepackPacket.names = PluginLoader.blockTextureBuilder.GetNames();
        texturepackPacket.textureResolution = PluginLoader.blockTextureBuilder.TextureResolution;
        texturepackPacket.resourceData = PluginLoader.blockTextureBuilder.GetTexture();
        connection.SendPacket(texturepackPacket.Write());

        // The item textures
        texturepackPacket = new ResourcePackPacket();
        texturepackPacket.resourceType = ResourceType.ITEMS_TEXTURES;
        texturepackPacket.names = PluginLoader.itemTextureBuilder.GetNames();
        texturepackPacket.textureResolution = PluginLoader.itemTextureBuilder.TextureResolution;
        texturepackPacket.resourceData = PluginLoader.itemTextureBuilder.GetTexture();
        connection.SendPacket(texturepackPacket.Write());

        foreach ((string, byte[]) clip in PluginLoader.audioBuilder)
        {
            ResourcePackPacket resourcePackPacket = new ResourcePackPacket();
            resourcePackPacket.names = new List<string>()
            {
                clip.Item1
            };
            resourcePackPacket.resourceType = ResourceType.SOUND;
            resourcePackPacket.resourceData = clip.Item2;
            Console.WriteLine("Sending packet for audio data: " + clip.Item1);
            connection.SendPacket(resourcePackPacket.Write());
            Console.WriteLine("Send! : " + clip.Item1);
        }



        // THEN the registry second.
        RegistryDataPacket registryPacket = new RegistryDataPacket();
        registryPacket.Data = Registry.SaveAll(); // #TODO cache this!
        connection.SendPacket(registryPacket.Write());


        OnConnect?.Invoke(connection);


        PlayerEntity player = new PlayerEntity(connection);

        player.OnEntityIdSet += () =>
        {
            AuthenticatePacket authenticatePacket = new AuthenticatePacket();
            authenticatePacket.EntityId = player.Id;
            authenticatePacket.ServerVersion = Program.Version;
            connection.SendPacket(authenticatePacket.Write());
        };

        // Load all world data
        Multiverse.SendWorldData(connection, Multiverse.GetMainWorld());

        Console.WriteLine("Spawning player...");
        Multiverse.GetMainWorld().SpawnEntity(player);
    }

    public void BroadcastPacket(Packet packet, Connection? exlude = null)
    {
        foreach (var connection in connections)
        {
            if (connection == exlude)
                continue;
            connection.SendPacket(packet);
        }
    }
    public event Action<Connection>? OnConnect;

    public void ReadPackets()
    {

        connections.RemoveAll((c) =>
        {
            return !c.IsConnected();
        });



        foreach (Connection connection in connections)
        {
            connection.HandlePackets();
        }

        foreach (Connection connection in futureConnections)
        {
            connections.Add(connection);
        }

        futureConnections.Clear();
    }

}
