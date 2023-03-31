namespace Siltra.Net;


using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;

public sealed class EncryptedStream : DataBuffer
{
    private IBufferedCipher? encryptCipher;
    private IBufferedCipher? decryptCipher;
    public EncryptedStream(byte[] key)
    {
        encryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesEngine(), 8));
        encryptCipher.Init(true, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

        decryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesEngine(), 8));
        decryptCipher.Init(false, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

        Stream oldStream = this.BaseStream;
        this.BaseStream = new CipherStream(oldStream, decryptCipher, encryptCipher);
    }

    public EncryptedStream(Stream stream, byte[] key) : base(stream)
    {
        encryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesEngine(), 8));
        encryptCipher.Init(true, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

        decryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesEngine(), 8));
        decryptCipher.Init(false, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

        Stream oldStream = this.BaseStream;
        this.BaseStream = new CipherStream(oldStream, decryptCipher, encryptCipher);
    }

    public EncryptedStream(byte[] data, byte[] key) : base(data)
    {
        encryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesEngine(), 8));
        encryptCipher.Init(true, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

        decryptCipher = new BufferedBlockCipher(new CfbBlockCipher(new AesEngine(), 8));
        decryptCipher.Init(false, new ParametersWithIV(new KeyParameter(key), key, 0, 16));

        Stream oldStream = this.BaseStream;
        this.BaseStream = new CipherStream(oldStream, decryptCipher, encryptCipher);
    }
}