using Server.Networking;
using Server.Worlds;
using Shared.Networking;
using System.Diagnostics;
public class Program
{
    public static ServerNetwork server = new ServerNetwork();

    public static void Main()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Multiverse.Start();
        Console.WriteLine("Starting server...");
        server.Start(5050);
        server.OnConnect += (Connection c) =>
        {
            Console.WriteLine("New Connection!");
        };
        Console.WriteLine($"ServerNetwork has started in {sw.ElapsedMilliseconds}ms!");
        while (true)
        {
            server.AcceptPending();

            Multiverse.TickWorlds();
            server.ReadPackets();
        }
    }
}