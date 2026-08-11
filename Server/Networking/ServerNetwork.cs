using Server.Worlds;
using Shared.Networking;
using System.Net.Sockets;

namespace Server.Networking;

public class ServerNetwork
{
    private List<Connection> connections = new List<Connection>();
    private TcpListener? listener;

    public void Start(int port)
    {
        listener = new TcpListener(System.Net.IPAddress.Any, port);
        listener.Start();
    }

    public void AcceptPending()
    {
        if (listener == null)
            throw new Exception("ServerNetwork has to be started before you can accept pending clients.");

        if (listener.Pending())
        {
            TcpClient client = listener.AcceptTcpClient();
            Connection connection = new Connection(client);
            connections.Add(connection);
            OnConnect?.Invoke(connection);

            Player player = new Player(connection);
            // Load all world data
            Multiverse.SendWorldData(connection, Multiverse.GetMainWorld());

            Multiverse.GetMainWorld().SpawnEntity(player);

            AuthenticatePacket authenticatePacket = new AuthenticatePacket();
            authenticatePacket.EntityId = player.Id;
            connection.SendPacket(authenticatePacket.Write());
        }
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
            connection.ReadPackets(100);
        }
    }

}
