namespace Server.Worlds;

public class Memory
{
    private readonly Dictionary<string, object> memory = new();

    public void Set(string key, object value)
    {
        memory[key] = value!;
    }

    public T? Get<T>(string key)
    {
        if (memory.TryGetValue(key, out var value) && value is T typed)
            return typed;

        return default;
    }

    public bool Has(string key)
    {
        return memory.ContainsKey(key);
    }

    public void Forget(string key)
    {
        memory.Remove(key);
    }
}
