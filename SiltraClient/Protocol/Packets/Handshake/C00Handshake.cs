using Siltra.Net;

namespace Siltra.Protocol.Packets;

public class C00Handshake : Packet
{
    public int Id => 0x00;

    public DataBuffer Deserialize()
    {
        DataBuffer buffer = new();

        return buffer;
    }

    public void Handle()
    {
        throw new NotImplementedException();
    }

    public void Serialize(DataBuffer buffer)
    {
        throw new NotImplementedException();
    }
}