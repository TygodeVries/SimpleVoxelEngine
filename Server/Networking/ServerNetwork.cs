using Server.Plugins;
using Server.Worlds;
using Shared;
using Shared.Networking;
using Shared.Worlds;
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

        // If Dreams is enabled
        try
        {
            TcpClient client = new TcpClient(Dreams.DREAMS_IP, Dreams.DREAMS_PORT);
            TcpConnection tcpConnection = new TcpConnection(client);

            // Tell them we are a server
            tcpConnection.SendPacket(new DreamsServerInfoPacket().Write());

            SetupDreams(tcpConnection);
            connections.Add(tcpConnection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not connect with the dreams server. Reason: {ex}");
        }
    }

    private TcpConnection? dreamsServerConnection;
    public void SetupDreams(TcpConnection dreamsConnection)
    {
        this.dreamsServerConnection = dreamsConnection;
        dreamsConnection.isDreamsAuthorizedServer = true;

        dreamsConnection.OnPacket += DreamsConnection_OnPacket;
    }

    private void DreamsConnection_OnPacket(Packet packet)
    {
        if (packet.GetPacketType() == PacketType.DreamsAddUser)
        {
            DreamsAddUserPacket dreamsAddUserPacket = new DreamsAddUserPacket();
            dreamsAddUserPacket.Read(packet);

            DreamsConnection dreamsConnection = new DreamsConnection(dreamsAddUserPacket.id);
            futureConnections.Add(dreamsConnection);
            dreamsConnections.Add(dreamsConnection.id, dreamsConnection);

            Console.WriteLine("Added dreams user with RegistryId: " + dreamsConnection.id);
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
            TcpClient client = listener.AcceptTcpClient();
            Connection connection = new TcpConnection(client);
            connections.Add(connection);
            CatchupConnection(connection);
        }
    }

    public void CatchupConnection(Connection connection)
    {
        // First send the textures
        TexturepackPacket texturepackPacket = new TexturepackPacket();
        texturepackPacket.textureType = TextureType.BLOCKS;
        texturepackPacket.names = PluginLoader.blockTextureBuilder.GetNames();
        texturepackPacket.textureResolution = PluginLoader.blockTextureBuilder.TextureResolution;
        texturepackPacket.textureData = PluginLoader.blockTextureBuilder.GetTexture();
        connection.SendPacket(texturepackPacket.Write());

        // The item textures
        texturepackPacket = new TexturepackPacket();
        texturepackPacket.textureType = TextureType.ITEMS;
        texturepackPacket.names = PluginLoader.itemTextureBuilder.GetNames();
        texturepackPacket.textureResolution = PluginLoader.itemTextureBuilder.TextureResolution;
        texturepackPacket.textureData = PluginLoader.itemTextureBuilder.GetTexture();
        connection.SendPacket(texturepackPacket.Write());

        // THEN the registry second.
        RegistryDataPacket registryPacket = new RegistryDataPacket();
        registryPacket.Data = Registry.SaveAll(); // #TODO cache this!
        connection.SendPacket(registryPacket.Write());


        OnConnect?.Invoke(connection);

        PlayerEntity player = new PlayerEntity(connection);

        // Load all world data
        Multiverse.SendWorldData(connection, Multiverse.GetMainWorld());

        Multiverse.GetMainWorld().SpawnEntity(player);

        AuthenticatePacket authenticatePacket = new AuthenticatePacket();
        authenticatePacket.EntityId = player.Id;
        connection.SendPacket(authenticatePacket.Write());
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
            connection.ReadPackets(5000);
        }

        foreach (Connection connection in futureConnections)
        {
            connections.Add(connection);
        }

        futureConnections.Clear();
    }

}
