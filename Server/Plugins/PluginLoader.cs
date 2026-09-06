using Spectre.Console;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Server.Plugins;

public class PluginLoader
{
    public static TextureBuilder blockTextureBuilder = new TextureBuilder();
    public static TextureBuilder itemTextureBuilder = new TextureBuilder();
    public static List<(string, byte[])> audioBuilder = new List<(string, byte[])>();

    internal static async Task LoadAllPluginsAsync()
    {
        if (!Directory.Exists("plugins"))
        {
            Directory.CreateDirectory("plugins");
        }

        string[] dirs = Directory.GetDirectories("plugins");
        if (dirs.Length == 0) return;

        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new SpinnerColumn(Spinner.Known.Dots)
            })
            .StartAsync(async ctx =>
            {
                Task[] loadingTasks = new Task[dirs.Length];

                for (int i = 0; i < dirs.Length; i++)
                {
                    string dir = dirs[i];
                    loadingTasks[i] = LoadPluginAsync(dir, ctx);
                }

                await Task.WhenAll(loadingTasks);
            });
    }

    private static async Task LoadPluginAsync(string pluginPath, ProgressContext ctx)
    {
        string pluginName = Path.GetFileName(pluginPath);

        ProgressTask task = ctx.AddTask($"[white]Initializing {pluginName}...[/]");

        string pluginDataPath = Path.Combine(pluginPath, "plugin.json");
        if (!File.Exists(pluginDataPath))
        {
            task.Description = $"[red]X {pluginName} (Missing plugin.json)[/]";
            task.StopTask();
            return;
        }

        try
        {
            string pluginDataContent = await File.ReadAllTextAsync(pluginDataPath);
            PluginData? pluginData = JsonSerializer.Deserialize<PluginData>(pluginDataContent);

            if (pluginData == null)
            {
                task.Description = $"[red]X {pluginName} (Malformed plugin.json)[/]";
                task.StopTask();
                return;
            }
        }
        catch (Exception ex)
        {
            task.Description = $"[red]X {pluginName} failed reading config: {ex.Message}[/]";
            task.StopTask();
            return;
        }

        string sourceDataPath = Path.Combine(pluginPath, "Source");
        if (Directory.Exists(sourceDataPath))
        {
            task.Description = $"[white]Compiling {pluginName}...[/]";
            await CompileAndLoadAsync(sourceDataPath);
        }

        string assetsData = Path.Combine(pluginPath, "assets");
        if (Directory.Exists(assetsData))
        {
            task.Description = $"[white]Loading assets for {pluginName}...[/]";
            LoadAssets(assetsData);
        }

        task.Description = $"[lime]Loaded {pluginName}[/]";
        task.StopTask();
    }

    private static async Task CompileAndLoadAsync(string path)
    {
        const string dotnetInstallerUrl =
            "https://dotnet.microsoft.com/download/dotnet/10.0";

        if (!await IsDotnetInstalledAsync())
        {
            Console.WriteLine("The .NET SDK is not installed.");
            Console.WriteLine($"Please install .NET 10 from: {dotnetInstallerUrl}");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dotnetInstallerUrl,
                    UseShellExecute = true
                });
            }
            catch
            {

            }

            return;
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = "build -c Release",
            WorkingDirectory = path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo)!;

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("Failed to build plugin.");
            Console.WriteLine(error);
            return;
        }

        string dllPath = Path.GetFullPath(
            Path.Combine(path, "bin", "Release", "net10.0", "plugin.dll")
        );

        if (File.Exists(dllPath))
        {
            Assembly.LoadFile(dllPath);
        }
        else
        {
            Console.WriteLine($"Build succeeded, but plugin DLL was not found: {dllPath}");
        }
    }

    private static async Task<bool> IsDotnetInstalledAsync()
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo)!;

            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static void LoadAssets(string path)
    {
        Thread.Sleep(400);
        LoadTexturesTo(Path.Combine(path, "Textures", "Blocks"), blockTextureBuilder);
        LoadTexturesTo(Path.Combine(path, "Textures", "Items"), itemTextureBuilder);
        LoadAudio(Path.Combine(path, "Audio"));
    }

    private static void LoadAudio(string path)
    {
        if (!Directory.Exists(path)) return;

        string[] audioFiles = Directory.GetFiles(path);
        foreach (string file in audioFiles)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            audioBuilder.Add((name, File.ReadAllBytes(file)));
        }
    }

    private static void LoadTexturesTo(string path, TextureBuilder textureBuilder)
    {
        if (!Directory.Exists(path)) return;

        string[] textureFiles = Directory.GetFiles(path);
        foreach (string file in textureFiles)
        {
            textureBuilder.AddTexture(file);
        }
    }

    internal static void RunAll()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(Plugin).IsAssignableFrom(type)) continue;

                Plugin? plugin = Activator.CreateInstance(type) as Plugin;
                plugin?.OnLoad();
            }
        }
    }

    internal static void RegisterAll()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(Plugin).IsAssignableFrom(type)) continue;

                Plugin? plugin = Activator.CreateInstance(type) as Plugin;
                plugin?.OnRegister();
            }
        }
    }
}

public class PluginData
{
    public string author { get; set; } = "Unknown";
}
