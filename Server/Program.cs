using Server.Networking;
using Server.Plugins;
using Server.Worlds;
using Shared.Networking;
using Shared.Worlds;
using System.Diagnostics;
public class Program
{
    public static ServerNetwork server = new ServerNetwork();

    public static void Main()
    {
        PluginLoader.LoadAllPlugins();
        Registry.InRegistryStage = true;
        DefaultBlocks.Register();
        PluginLoader.RegisterAll();
        Registry.InRegistryStage = false;

        Console.WriteLine("--- Loading Server ---");

        Stopwatch sw = Stopwatch.StartNew();

        Multiverse.Start();
        PluginLoader.RunAll();

        Console.WriteLine("Starting server...");

        server.Start(5050);
        server.OnConnect += (Connection c) =>
        {
            Console.WriteLine("New Connection!");
        };

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Server has started in {sw.ElapsedMilliseconds}ms!");
        Console.ForegroundColor = ConsoleColor.White;

        while (true)
        {
            server.AcceptTcpServerConnections();

            Multiverse.TickWorlds();
            server.ReadPackets();
        }
    }
}