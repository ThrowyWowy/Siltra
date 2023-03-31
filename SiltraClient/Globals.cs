namespace Siltra;

using Siltra.Config;
using Siltra.Accounts;

public static class Globals
{
    public static IniConfig? Config;
    public static HttpClient HttpClient = new();
    public static Session? Session;
}