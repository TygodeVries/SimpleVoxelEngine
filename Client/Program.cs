using Client;
using Client.Networking;
using Client.Rendering;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;

public class Program
{
    private static async Task LookForDeadParentAsync(int parentPid)
    {
        try
        {
            using (Process parentProcess = Process.GetProcessById(parentPid))
            {
                while (!parentProcess.HasExited)
                {
                    await Task.Delay(1000);
                }
            }
        }
        catch (ArgumentException)
        {

        }

        Environment.Exit(0);
    }

    public static void Main(string[] args)
    {
        DedicatedSwitch.Switch();

        Console.WriteLine("#################");
        Console.WriteLine("#     CLIENT    #");
        Console.WriteLine("#################");

        if (args.Length != 0)
        {
            if (args[0] == "--multiple")
            {
                /*
                int currentId = Environment.ProcessId;

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    Arguments = $"--depends {currentId}"
                };

                Process.Start(startInfo);
                */
            }
            else if (args[0] == "--depends" && args.Length > 1)
            {
                if (int.TryParse(args[1], out int parentPid))
                {
                    _ = LookForDeadParentAsync(parentPid);
                }
            }
        }

        // The program starts here.
        // Setup the settings for our window
        GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
        NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;

        gameWindowSettings.UpdateFrequency = 144.0; // Max FPS

        Console.WriteLine("How do you want to connect to the server?");

        Console.WriteLine("[1]: Via a direct connection");
        Console.WriteLine("[2]: Via a dreams server");


        bool isDirect = false;

        while (true)
        {
            string msg = Console.ReadLine();

            if (msg == "1")
            {
                isDirect = true;
                break;
            }

            if (msg == "2")
            {
                isDirect = false;
                break;
            }

            Console.WriteLine($"I don't know what you mean with {msg}");
        }

        Console.Clear();
        Console.WriteLine("What address do you want to connect with?");

        if (isDirect)
        {
            Console.WriteLine("Format: [IP]:[PORT]");
        }
        string address = Console.ReadLine();

        Network.Connect(isDirect, address);
        LocalWorld.ListenForPackets();

        nativeWindowSettings.Title = "A yet to be named game.";
        nativeWindowSettings.ClientSize = new OpenTK.Mathematics.Vector2i(1920, 1080);

        // Create the window object
        GameCanvas gameCanvas = new GameCanvas(gameWindowSettings, nativeWindowSettings);

        // Start the program (blocks this thread until the window closes.)
        gameCanvas.Run();

        gameCanvas.IsVisible = false;
        if (HasCrashed)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("(>_<) Oops! (We detected a crash, and kept the console open. Press enter to continue)");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("The game has closed!");
        }
    }

    public static bool HasCrashed = false;
}