#define DEBUG

namespace Siltra;

using System;
using Siltra.Accounts;
using Siltra.Net;

public class Client
{

    public Client()
    {
        Logger.WriteLine("Starting &3Siltra&f...");
        Logger.WriteLine("Loading Config...");
#if DEBUG
        Globals.Config = new("test/Config.ini");
#else
        Globals.Config = new("Config.ini");
#endif

        Logger.WriteLine("Logging in... USING SESSION ID");

        Session session = new();
        session.SessionToken = Globals.Config.GetValue<string>("bot.session", "");
        session.PlayerName = Globals.Config.GetValue<string>("bot.name", "SiltraBot");
        session.PlayerUuid = Globals.Config.GetValue<string>("bot.uuid", "-");

        Globals.Session = session;

        Bot Bot = new(session);
        Bot.Connect("hypixel.net", 25565);
    }
}