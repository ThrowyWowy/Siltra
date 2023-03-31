namespace Siltra.Protocol;

using Siltra.Net;

public interface Packet
{
    public int Id {get;}
    public void Serialize(DataBuffer buffer);
    public DataBuffer Deserialize();
    public void Handle();
}