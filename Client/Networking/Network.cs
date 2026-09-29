using Client.Rendering.UI;
using Shared;
using Shared.Mathf;
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

            Console.WriteLine($"Connecting with {address}...");
            if (connection != null)
                connection.Disconnect();

            Registry.Clear();
            LocalWorld.ResetWorld();
            ToastManager.Clear();
            Registry.InRegistryStage = true;
            Defaults.Register();
            Registry.InRegistryStage = false;

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

        private static int reconnecting;

        private static void Connection_OnDisconnect()
        {
            Console.WriteLine("The connection to the server was lost!");

            connection = null;

            Schedule.Run(() =>
            {

                ToastManager.Clear();
                ToastManager.Send("<red>Connection Lost...", 300);

                if (Interlocked.Exchange(ref reconnecting, 1) == 1)
                    return;

                _ = ReconnectAsync();
            });
        }

        private static async Task ReconnectAsync()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for server...");

                    if (await IsServerAvailableAsync("127.0.0.1", 5050))
                    {
                        Schedule.Run(() =>
                        {
                            ToastManager.Clear();
                            ToastManager.Send("<green>Reconnecting...", 300);

                            Connect(true, "127.0.0.1:5050");
                        });

                        return;
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnect failed: {ex}");
            }
            finally
            {
                Interlocked.Exchange(ref reconnecting, 0);
            }
        }

        private static async Task<bool> IsServerAvailableAsync(
            string address,
            int port)
        {
            try
            {
                using var client = new TcpClient();

                await client.ConnectAsync(address, port);

                return client.Connected;
            }
            catch
            {
                return false;
            }
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
            {
                // No connection, #TODO need to figgure out for something, not just disconnect.
            }
        }

        public static event Action<Packet>? OnPacket;
    }
}
