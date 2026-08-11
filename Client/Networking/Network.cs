using Shared.Networking;
using System.Net.Sockets;

namespace Client.Networking
{
    public class Network
    {
        private static Connection? connection;
        public static void Connect()
        {
            Console.WriteLine("Connecting to server...");
            TcpClient client = new TcpClient("127.0.0.1", 5050);
            connection = new Connection(client);

            Console.WriteLine("Connected!");

            connection.OnPacket += (Packet packet) =>
            {
                OnPacket?.Invoke(packet);
            };
        }

        public static void Tick()
        {
            connection?.ReadPackets(100);
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
