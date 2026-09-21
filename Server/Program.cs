

using Server.Networking;
using Server.Plugins;
using Server.Worlds;
using Shared.Mathf;
using Shared.Worlds;
using Spectre.Console;
using System.Diagnostics;
public class Program
{
    public static int Version = 0;
    public static ServerNetwork server = new ServerNetwork();

    public static void Main()
    {
        AnsiConsole.MarkupLine("[white]Loading Plugins...[/]");
        Stopwatch fullTime = Stopwatch.StartNew();
        PluginLoader.LoadAllPluginsAsync().Wait();

        AnsiConsole.MarkupLine("Loading Registry...");
        Registry.InRegistryStage = true;
        DefaultBlocks.Register();
        DefaultEntities.Register();
        PluginLoader.RegisterAll();
        Registry.InRegistryStage = false;

        Stopwatch serverTime = Stopwatch.StartNew();

        AnsiConsole.MarkupLine("Loading World...");
        Multiverse.Start();

        PluginLoader.OnPluginChange += () =>
        {
            foreach (PlayerEntity playerEntity in Multiverse.GetPlayers())
            {
                playerEntity.SendToast("Code change detected, press F5 to restart.");
            }
        };

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
            AnsiConsole.MarkupLine("[lime]Connected with Dreams.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Could not connect with Dreams.[/]");
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        double previousTime = stopwatch.Elapsed.TotalSeconds;

        while (true)
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            float deltaTime = (float)(currentTime - previousTime);
            previousTime = currentTime;


            Time.DeltaTime = deltaTime;

            server.AcceptTcpServerConnections();

            Multiverse.TickWorlds();
            Schedule.Tick();
            server.ReadPackets();

            Thread.Sleep(5);
        }
    }

    public static void Restart()
    {
        string? executable = Environment.ProcessPath;

        if (executable == null)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = true
        });

        Environment.Exit(0);
    }

}