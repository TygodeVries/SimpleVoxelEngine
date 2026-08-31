

using Server.Networking;
using Server.Plugins;
using Server.Worlds;
using Shared.Worlds;
using Spectre.Console;
using System.Diagnostics;
public class Program
{
    public static ServerNetwork server = new ServerNetwork();

    public static void Main()
    {
        AnsiConsole.MarkupLine("[white]Loading Plugins...[/]");
        Stopwatch fullTime = Stopwatch.StartNew();
        PluginLoader.LoadAllPluginsAsync().Wait();

        AnsiConsole.MarkupLine("Loading Registry...");
        Registry.InRegistryStage = true;
        DefaultBlocks.Register();
        PluginLoader.RegisterAll();
        Registry.InRegistryStage = false;

        Stopwatch serverTime = Stopwatch.StartNew();

        AnsiConsole.MarkupLine("Loading World...");
        Multiverse.Start();


        PluginLoader.RunAll();
        AnsiConsole.Status()
            .Start("Starting Server...", ctx =>
            {
                server.Start(5050);

                AnsiConsole.MarkupLine($"[Lime]Server has started in {fullTime.ElapsedMilliseconds}ms! (Server: {serverTime.ElapsedMilliseconds}ms)[/]");
            });


        bool dreamsConnected = false;
        AnsiConsole.Status()
            .Start("Connecting With Dreams...", ctx =>
            {
                dreamsConnected = server.StartDreams();
            });

        if (dreamsConnected)
        {
            AnsiConsole.Markup("[lime]Connected with Dreams.[/]");
        }
        else
        {
            AnsiConsole.Markup("[yellow]Could not connect with Dreams.[/]");
        }

        while (true)
        {
            server.AcceptTcpServerConnections();

            Multiverse.TickWorlds();
            server.ReadPackets();
        }
    }
}