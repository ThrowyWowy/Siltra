namespace Siltra.Net;

using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using Siltra.Types;

public class DataBuffer
{
    public Stream BaseStream {get; set;}
    public SemaphoreSlim Lock {get; set;} = new(1, 1);
    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }
    public bool CanRead => BaseStream.CanRead;

    public bool CanSeek => BaseStream.CanSeek;

    public bool CanWrite => BaseStream.CanWrite;

    public long Length => BaseStream.Length;
    public DataBuffer()
    {
        BaseStream = new MemoryStream();
    }
    public DataBuffer(Stream stream)
    {
        BaseStream = stream;
    }
    public DataBuffer(byte[] data)
    {
        BaseStream = new MemoryStream(data);
    }

    public void Flush() => BaseStream.Flush();
    public int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);
    public void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);
    public void Write(Span<byte> buffer) => BaseStream.Write(buffer.ToArray(), 0, buffer.Length);
    public long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);
    public void SetLength(long value) => BaseStream.SetLength(value);
    public byte[] ToArray()
    {
        this.Position = 0;
        byte[] buffer = new byte[this.Length];
        for (int totalBytes = 0; totalBytes < this.Length;)
            totalBytes += Read(buffer, totalBytes, Convert.ToInt32(this.Length) - totalBytes);
        
        return buffer;
    }
    public void Read(Span<byte> buffer) => BaseStream.Read(buffer);

    #region READ
    
    public sbyte ReadSignedByte() => (sbyte) ReadUnsignedByte();    
    public byte ReadUnsignedByte()
    {
        Span<byte> buffer = stackalloc byte[1];
        BaseStream.Read(buffer);
        return buffer[0];
    }

    public bool ReadBoolean() => ReadUnsignedByte() == 0x01;
    public ushort ReadUnsignedShort()
    {
        Span<byte> buffer = stackalloc byte[2];
        Read(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public short ReadShort()
    {
        Span<byte> buffer = stackalloc byte[2];
        this.Read(buffer);
        return BinaryPrimitives.ReadInt16BigEndian(buffer);
    }

    public int ReadInt()
    {
        Span<byte> buffer = stackalloc byte[4];
        this.Read(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    public long ReadLong()
    {
        Span<byte> buffer = stackalloc byte[8];
        this.Read(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    public ulong ReadUnsignedLong()
    {
        Span<byte> buffer = stackalloc byte[8];
        this.Read(buffer);
        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }

    public float ReadFloat()
    {
        Span<byte> buffer = stackalloc byte[4];
        this.Read(buffer);
        return BinaryPrimitives.ReadSingleBigEndian(buffer);
    }

    public double ReadDouble()
    {
        Span<byte> buffer = stackalloc byte[8];
        this.Read(buffer);
        return BinaryPrimitives.ReadDoubleBigEndian(buffer);
    }

    public string ReadString(int maxLength = 32767)
    {
        int length = ReadVarInt();
        byte[] buffer = new byte[length];
        this.Read(buffer, 0, length);

        string value = Encoding.UTF8.GetString(buffer);
        if (maxLength > 0 && value.Length > maxLength)
        {
            throw new ArgumentException($"string ({value.Length}) exceeded maximum length ({maxLength})", nameof(value));
        }
        return value;
    }

    public int ReadVarInt()
    {
        int numRead = 0;
        int result = 0;
        byte read;
        do
        {
            read = this.ReadUnsignedByte();
            int value = read & 0b01111111;
            result |= value << (7 * numRead);

            numRead++;
            if (numRead > 5)
            {
                throw new InvalidOperationException("VarInt is too big");
            }
        } while ((read & 0b10000000) != 0);

        return result;
    }

    public byte[] ReadUInt8Array(int length = 0)
    {
        if (length == 0)
            length = ReadVarInt();

        byte[] result = new byte[length];
        if (length == 0)
            return result;

        int n = length;
        while (true)
        {
            n -= Read(result, length - n, n);
            if (n == 0)
                break;
        }
        return result;
    }

    public long ReadVarLong()
    {
        int numRead = 0;
        long result = 0;
        byte read;
        do
        {
            read = this.ReadUnsignedByte();
            int value = (read & 0b01111111);
            result |= (long)value << (7 * numRead);

            numRead++;
            if (numRead > 10)
            {
                throw new InvalidOperationException("VarLong is too big");
            }
        } while ((read & 0b10000000) != 0);

        return result;
    }

    public DateTimeOffset ReadDateTimeOffset() => DateTimeOffset.FromUnixTimeMilliseconds(this.ReadLong());

    public Vector3 ReadPosition()
    {
        ulong value = this.ReadUnsignedLong();

        long x = (long)(value >> 38);
        long y = (long)(value & 0xFFF);
        long z = (long)(value << 26 >> 38);

        if (x >= Math.Pow(2, 25))
            x -= (long)Math.Pow(2, 26);

        if (y >= Math.Pow(2, 11))
            y -= (long)Math.Pow(2, 12);

        if (z >= Math.Pow(2, 25))
            z -= (long)Math.Pow(2, 26);

        return new Vector3
        {
            X = (int) x,
            Y = (int) y,
            Z = (int) z
        };
    }

    public Vector3 ReadAbsolutePosition()
    {
        return new Vector3
        {
            X = (int) ReadDouble(),
            Y = (int) ReadDouble(),
            Z = (int) ReadDouble()
        };
    }

    public Guid ReadGuid() => GuidHelper.FromLongs(ReadLong(), ReadLong());

    #endregion

    #region WRITE

    public void WriteByte(sbyte value) => BaseStream.WriteByte((byte)value);
    public void WriteUnsignedByte(byte value) => BaseStream.WriteByte(value);
    public void WriteBoolean(bool value) => BaseStream.WriteByte((byte)(value ? 0x01 : 0x00));
    public void WriteUnsignedShort(ushort value)
    {
        Span<byte> span = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(span, value);
        BaseStream.Write(span);
    }

    public void WriteShort(short value)
    {
        Span<byte> span = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(span, value);
        BaseStream.Write(span);
    }
    public void WriteInt(int value)
    {
        Span<byte> span = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(span, value);
        BaseStream.Write(span);
    }
    public void WriteLong(long value)
    {
        Span<byte> span = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(span, value);
        BaseStream.Write(span);
    }
    public void WriteFloat(float value)
    {
        Span<byte> span = stackalloc byte[4];
        BinaryPrimitives.WriteSingleBigEndian(span, value);
        BaseStream.Write(span);
    }
    public void WriteDouble(double value)
    {
        Span<byte> span = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleBigEndian(span, value);
        BaseStream.Write(span);
    }
    public void WriteString(string value, int maxLength = short.MaxValue)
    {
        RentedArray<byte> bytes = new RentedArray<byte>(Encoding.UTF8.GetByteCount(value));
        Encoding.UTF8.GetBytes(value, bytes.Span);
        WriteVarInt(bytes.Length);
        Write(bytes);
    }

    public void WriteVarInt(int value)
    {
        uint unsigned = (uint)value;

        do
        {
            byte temp = (byte)(unsigned & 127);
            unsigned >>= 7;

            if (unsigned != 0)
                temp |= 128;

            BaseStream.WriteByte(temp);
        }
        while (unsigned != 0);
    }

    public void WriteVarInt(Enum value)
    {
        WriteVarInt(Convert.ToInt32(value));
    }

    public void WriteLongArray(long[] values)
    {
        Span<byte> buffer = stackalloc byte[8];
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteInt64BigEndian(buffer, values[i]);
            BaseStream.Write(buffer);
        }
    }
    public void WriteVarLong(long value)
    {
        ulong unsigned = (ulong)value;

        do
        {
            byte temp = (byte)(unsigned & 127);

            unsigned >>= 7;

            if (unsigned != 0)
                temp |= 128;


            BaseStream.WriteByte(temp);
        }
        while (unsigned != 0);
    }
    public void WriteDateTimeOffset(DateTimeOffset date)
    {
        this.WriteLong(date.ToUnixTimeMilliseconds());
    }

    public void WriteUuid(Guid value)
    {
        if (value == Guid.Empty)
        {
            WriteLong(0L);
            WriteLong(0L);
        }
        else
        {
            var uuid = System.Numerics.BigInteger.Parse(value.ToString().Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
            Write(uuid.ToByteArray(false, true));
        }
    }

    public void WritePosition(Vector3 value)
    {
        long val = (long)(value.X & 0x3FFFFFF) << 38;
        val |= (long)(value.Z & 0x3FFFFFF) << 12;
        val |= (long)(value.Y & 0xFFF);

        WriteLong(val);
    }
    public void WriteAbsolutePosition(Vector3 value)
    {
        WriteDouble(value.X);
        WriteDouble(value.Y);
        WriteDouble(value.Z);
    }
    public void WriteAbsoluteFloatPosition(Vector3 value)
    {
        WriteFloat(value.X);
        WriteFloat(value.Y);
        WriteFloat(value.Z);
    }
    public void WriteAbsoluteShortPosition(Vector3 value)
    {
        WriteShort((short)value.X);
        WriteShort((short)value.Y);
        WriteShort((short)value.Z);
    }

    #endregion
}
public static class GuidHelper {
    public static  Guid FromLongs(long mostSig, long leastSig)
    {
        var mostSigBytes = BitConverter.GetBytes(mostSig);
        var leastSigBytes = BitConverter.GetBytes(leastSig);

        Span<byte> guidBytes = stackalloc byte[16]//Is there a better way??
        {
            mostSigBytes[4],
            mostSigBytes[5],
            mostSigBytes[6],
            mostSigBytes[7],
            mostSigBytes[2],
            mostSigBytes[3],
            mostSigBytes[0],
            mostSigBytes[1],
            leastSigBytes[7],
            leastSigBytes[6],
            leastSigBytes[5],
            leastSigBytes[4],
            leastSigBytes[3],
            leastSigBytes[2],
            leastSigBytes[1],
            leastSigBytes[0]
        };

        return new Guid(guidBytes);
    }
}