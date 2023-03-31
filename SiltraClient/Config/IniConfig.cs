namespace Siltra.Config;

public class IniConfig
{
    private Dictionary<string, object> Pairs = new();
    private string FilePath;
    public IniConfig(string path)
    {
        FilePath = path;
        Load();
    }

    private void Load()
    {
        string[] lines = File.ReadAllLines(FilePath);

        foreach (string line in lines)
        {
            string[] split = line.Split("=");
            SetValue<object>(split[0], split[1]);
        }
    }
    private void Save()
    {
        List<string> Save = new();

        foreach (KeyValuePair<string, object> pair in Pairs)
        {
            Save.Add(pair.Key + "=" + pair.Value);
        }

        File.WriteAllLines(FilePath, Save);
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        if (Pairs.ContainsKey(key)) return (T) Pairs[key];
        else
        {
            SetValue<T>(key, defaultValue);
            return defaultValue;
        }
    }

    public void SetValue<T>(string key, T value)
    {
        Pairs[key] = value!;
        Save();
    }
}