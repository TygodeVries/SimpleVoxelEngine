using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Server.Plugins;

public class PluginLoader
{
    internal static void LoadAllPlugins()
    {
        Stopwatch sw = Stopwatch.StartNew();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("--- Loading Plugins ---");

        if (!Directory.Exists("plugins"))
        {
            Directory.CreateDirectory("plugins");
        }

        string[] dirs = Directory.GetDirectories("plugins");

        int successCount = 0;

        foreach (string dir in dirs)
        {
            bool success = LoadPlugin(dir);

            if (success)
            {
                successCount++;
            }
            else
            {
                Console.WriteLine($"Failed to load {dir}.");
            }
        }

        if (successCount == dirs.Length)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        Console.WriteLine($"Loaded {successCount}/{dirs.Length} plugins in {sw.ElapsedMilliseconds}ms!");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static bool LoadPlugin(string pluginPath)
    {
        string pluginName = Path.GetFileName(pluginPath);

        Console.WriteLine($"Loading plugin {pluginName}...");


        string pluginDataPath = $"{pluginPath}/plugin.json";
        if (!File.Exists(pluginDataPath))
        {
            Console.WriteLine($"Missing plugin.json inside of {pluginName}.");
            return false;
        }

        string pluginDataContent = File.ReadAllText(pluginDataPath);
        PluginData? pluginData = JsonSerializer.Deserialize<PluginData>(pluginDataContent);

        if (pluginData == null)
        {
            Console.WriteLine($"Plugin.json could not be loaded {pluginName}.");
            return false;
        }

        string sourceDataPath = $"{pluginPath}/Source";
        if (Directory.Exists(sourceDataPath))
        {
            Console.WriteLine("Found code in this plugin");
            CompileAndLoad(sourceDataPath);
        }
        else
        {
            Console.WriteLine($"No source directory was found (expected {sourceDataPath}).");
        }

        return true;
    }

    internal static void RunAll()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract)
                    continue;

                if (!typeof(Plugin).IsAssignableFrom(type))
                    continue;

                Plugin? plugin = Activator.CreateInstance(type) as Plugin;

                if (plugin == null)
                    continue;

                plugin.OnLoad();
            }
        }
    }

    private static bool CompileAndLoad(string path)
    {
        Console.WriteLine("Compiling plugin...");
        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            Arguments = "build -c Release",
            WorkingDirectory = path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using Process process = Process.Start(startInfo)!;

        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();

        process.WaitForExit();

        Console.WriteLine(output);

        if (process.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Plugin compilation failed:");
            Console.WriteLine(errors);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
            return false;
        }

        Console.WriteLine("Plugin compiled successfully!");

        string dllPath = Path.GetFullPath(
    Path.Combine(path, "bin", "Release", "net10.0", "plugin.dll")
);

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"Compiled plugin DLL not found: {dllPath}");
            return false;
        }

        Assembly.LoadFile(dllPath);

        return true;
    }
}

public class PluginData
{
    public string author { get; set; } = "Unknown";
}
