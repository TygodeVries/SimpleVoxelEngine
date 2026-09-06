using Shared;
using Shared.Networking;
using Shared.Worlds;
using System.Net.Sockets;

namespace Client.Networking
{
    public class Network
    {
        private static Connection? connection;
        public static void Connect(bool isTcpServer, string address)
        {
            if (connection != null)
                connection.Disconnect();
            Registry.Clear();
            LocalWorld.ResetWorld();

            if (isTcpServer)
            {
                string[] args = address.Split(':');
                Console.WriteLine("Connecting to server...");
                TcpClient client = new TcpClient(args[0], int.Parse(args[1]));
                connection = new TcpConnection(client);

                Console.WriteLine("Connected!");

                connection.OnPacket += (Packet packet) =>
                {
                    OnPacket?.Invoke(packet);
                };
            }
            else
            {
                Thread.Sleep(3000); // #TEMP WAIT FOR SERVER

                Console.WriteLine("Connecting to dreams server...");
                TcpClient client = new TcpClient(Dreams.DREAMS_IP, Dreams.DREAMS_PORT);
                connection = new TcpConnection(client);

                DreamsJoinPacket dreamsJoinPacket = new DreamsJoinPacket();
                dreamsJoinPacket.code = address;
                Console.WriteLine("Joining with code: '" + address + "'");
                connection.SendPacket(dreamsJoinPacket.Write());

                connection.OnPacket += (Packet packet) =>
                {
                    OnPacket?.Invoke(packet);
                };
            }

            connection!.ReadPacketsLoop();
            connection!.OnDisconnect += Connection_OnDisconnect;
        }

        private static void Connection_OnDisconnect()
        {
            Console.WriteLine("The connection to the server was lost!");
            // #TODO -> Go to backup server
        }

        public static void Tick()
        {
            connection?.HandlePackets();
        }

        public static void SendPacket(Packet packet)
        {
            if (connection != null)
                connection.SendPacket(packet);
            else
                throw new Exception("Server not connected!");
        }

        public static event Action<Packet>? OnPacket;
    }
}
