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

        Network.Connect();

        LocalWorld.ListenForPackets();

        nativeWindowSettings.Title = "Simple Voxel Engine";

        // Create the window object
        GameCanvas gameCanvas = new GameCanvas(gameWindowSettings, nativeWindowSettings);

        // Start the program (blocks this thread until the window closes.)
        gameCanvas.Run();

        // The program end here.
        Console.WriteLine("The game has closed!");

        Environment.Exit(0);
    }
}