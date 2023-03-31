namespace Siltra.Config;

public sealed class Key
{
    public string Value;
    public string Entry;
    public Key(string key, string value)
    {
        Value = value;
        Entry = key;
    }
}