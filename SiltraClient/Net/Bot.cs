namespace Siltra.Net;

using Siltra.Accounts;
using System.Net;
using System.Net.Sockets;
using Siltra.Protocol;
using Siltra.Protocol.Packets;
using Ionic.Zlib;

public class Bot
{
    public Session Session;
    public TcpClient Client;
    public int CompressionThreshold { get; set; }
    public bool Disconnected { get; private set; }
    public Bot(Session session)
    {
        Session = session;
        Client = new();
    }

    public void Connect(string ip, ushort port)
    {
        Client.SendBufferSize = 1024^2;

        try
        {
            Client.Connect(ip, port);
            Logger.WriteLine("&aStarted Connection.");
            while (!Disconnected)
            {
                DataBuffer buffer = ReadPacket(out int packetId);
                if (!OnPacket(buffer, packetId))
                {
                    Logger.WriteLine("&cError receiving packet: ID: 0x" + packetId.ToString("X2"));
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.WriteLine("&c" + ex.ToString());
        }
    }

    public bool OnPacket(DataBuffer buffer, int packetId)
    {
        

        return true;
    }

    public void SendPacket(Packet packet)
    {
        
    }

    public DataBuffer ReadPacket(out int packId)
    {
        int size = ReadNextVarInt();
        DataBuffer buffer = new(Receive(size));
        if (CompressionThreshold > 0)
        {
            int sizeUncompressed = buffer.ReadVarInt();
            if (sizeUncompressed != 0)
            {
                buffer = new(Zlib.Decompress(buffer.ToArray(), sizeUncompressed));
            }
        }
        packId = buffer.ReadVarInt();
        return buffer;
    }

    private void Send(byte[] data)
    {
        Client.Client.Send(data);
    }

    private int ReadNextVarInt()
    {
        int i = 0;
        int j = 0;
        int k = 0;
        byte[] tmp = new byte[1];
        while (true)
        {
            Receive(tmp, 0, 1);
            k = tmp[0];
            i |= (k & 0x7F) << j++ * 7;
            if (j > 5) throw new OverflowException("VarInt too big");
            if ((k & 0x80) != 128) break;
        }
        return i;
    }

    private byte[] Receive(int length)
    {
        var buffer = new byte[length];
        Receive(buffer, 0, buffer.Length);
        return buffer;
    }

    private void Receive(byte[] buffer, int start, int length)
    {
        if (Disconnected) return;
        try
        {
            int read = 0;
            while (read < length)
            {
                read += Client.Client.Receive(buffer, start + read, length - read, SocketFlags.None);
            }
        }
        catch
        {
                Disconnected = true;
        }
    }
}