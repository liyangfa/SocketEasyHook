using System.IO;
using System.Text;

public class ByteArray
{
    private MemoryStream _memoryStream;
    private const byte BooleanFalse = 2;
    private const byte BooleanTrue = 3;
    public string endian;

    //字节顺序（AS3默认是BIG_ENDIAN）
    public static string BIG_ENDIAN = "BIG_ENDIAN";
    public static string LITTLE_ENDIAN = "LITTLE_ENDIAN";

    public ByteArray()
    {
        _memoryStream = new MemoryStream();
        endian = ByteArray.BIG_ENDIAN;
    }

    public ByteArray(MemoryStream ms)
    {
        _memoryStream = ms;
        endian = ByteArray.BIG_ENDIAN;
    }

    public ByteArray(byte[] buffer)
    {
        _memoryStream = new MemoryStream();
        _memoryStream.Write(buffer, 0, buffer.Length);
        _memoryStream.Position = 0;
        endian = ByteArray.BIG_ENDIAN;
    }
    public void ByteReset(byte[] buffer)
    {
        _memoryStream = new MemoryStream();
        _memoryStream.Write(buffer, 0, buffer.Length);
        _memoryStream.Position = 0;
        endian = ByteArray.BIG_ENDIAN;
    }




    public void dispose()
    {
        if (_memoryStream != null)
        {
            _memoryStream.Close();
            _memoryStream.Dispose();
        }
        _memoryStream = null;
    }

    public uint Length
    {
        get
        {
            return (uint)_memoryStream.Length;
        }
    }

    public uint Position
    {
        get { return (uint)_memoryStream.Position; }
        set { _memoryStream.Position = value; }
    }

    public uint BytesAvailable
    {
        get { return Length - Position; }
    }

    public byte[] GetBuffer()
    {
        return _memoryStream.GetBuffer();
    }

    public byte[] ToArray()
    {
        return _memoryStream.ToArray();
    }

    public MemoryStream MemoryStream
    {
        get
        {
            return _memoryStream;
        }
    }

    public bool ReadBoolean()
    {
        return _memoryStream.ReadByte() == BooleanTrue;
    }

    public byte ReadByte()
    {
        return (byte)_memoryStream.ReadByte();
    }

    public void ReadBytes(byte[] bytes, uint offset, uint length)
    {
        _memoryStream.Read(bytes, (int)offset, (int)length);
    }

    public void ReadBytes(ByteArray bytes, uint offset, uint length)
    {
        uint tmp = bytes.Position;
        int count = (int)(length != 0 ? length : BytesAvailable);
        for (int i = 0; i < count; i++)
        {
            bytes._memoryStream.Position = i + offset;
            bytes._memoryStream.WriteByte(ReadByte());
        }
        bytes.Position = tmp;
    }

    private byte[] priReadBytes(uint c)
    {
        byte[] a = new byte[c];
        if (endian == ByteArray.BIG_ENDIAN)
        {
            //BIG_ENDIAN
            for (uint i = 0; i < c; i++)
            {
                a[c - 1 - i] = (byte)_memoryStream.ReadByte();
            }
        }
        else
        {
            //LITTLE_ENDIAN
            for (uint i = 0; i < c; i++)
            {
                a[i] = (byte)_memoryStream.ReadByte();
            }
        }
        return a;
    }

    public double ReadDouble()
    {
        byte[] bytes = priReadBytes(8);
        double value = System.BitConverter.ToDouble(bytes, 0);
        return value;
    }

    public float ReadFloat()
    {
        byte[] bytes = priReadBytes(4);
        float value = System.BitConverter.ToSingle(bytes, 0);
        return value;
    }

    public int ReadInt()
    {
        byte[] bytes = priReadBytes(4);
        int value = System.BitConverter.ToInt32(bytes, 0);
        return value;
    }
    //public uint ReadUint()
    //{
    //    byte[] bytes = priReadBytes(4);
    //    uint value = System.BitConverter.ToUInt32(bytes, 0);
    //    return value;
    //}

    public short ReadShort()
    {
        byte[] bytes = priReadBytes(2);
        short value = System.BitConverter.ToInt16(bytes, 0);
        return value;
    }

    public string ReadUTF()
    {
        uint length = (uint)ReadShort();
        return ReadUTFBytes(length);
    }

    public string ReadUTFBytes(uint length)
    {
        if (length == 0)
            return string.Empty;
        UTF8Encoding utf8 = new UTF8Encoding(false, true);
        byte[] encodedBytes = new byte[length];
        for (uint i = 0; i < length; i++)
        {
            encodedBytes[i] = (byte)_memoryStream.ReadByte();
        }
        string decodedString = utf8.GetString(encodedBytes, 0, encodedBytes.Length);
        return decodedString;
    }

    //=========================================


    public void WriteBoolean(bool value)
    {
        WriteByte((byte)(value ? BooleanTrue : BooleanFalse));
    }

    public void WriteByte(byte value)
    {
        _memoryStream.WriteByte(value);
    }

    public void WriteBytes(byte[] bytes, int offset, int length)
    {
        for (int i = offset; i < offset + length; i++)
        {
            if (i < bytes.Length)
            {
                _memoryStream.WriteByte(bytes[i]);
            }
            else
            {
                break;
            }
        }
    }

    public void WriteBytes(ByteArray bytes, int offset, int length)
    {
        byte[] data = bytes.ToArray();
        WriteBytes(data, offset, length);
    }

    public void WriteDouble(double value)
    {
        byte[] bytes = System.BitConverter.GetBytes(value);
        WriteBigEndian(bytes);
    }

    private void WriteBigEndian(byte[] bytes)
    {
        if (bytes == null)
            return;
        if (endian == ByteArray.BIG_ENDIAN)
        {
            //BIG_ENDIAN
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                WriteByte(bytes[i]);
            }
        }
        else
        {
            //LITTLE_ENDIAN;
            for (int i = 0; i < bytes.Length; i++)
            {
                _memoryStream.WriteByte(bytes[i]);
            }
        }
    }

    public void WriteFloat(float value)
    {
        byte[] bytes = System.BitConverter.GetBytes(value);
        WriteBigEndian(bytes);
    }

    public void WriteInt(int value)
    {
        WriteInt32(value);
    }
    public void WriteunsignedInt(uint value)//自己新添加
    {
        WriteIntunsigned32(value);
    }


    private void WriteInt32(int value)
    {
        byte[] bytes = System.BitConverter.GetBytes(value);
        WriteBigEndian(bytes);
    }
    private void WriteIntunsigned32(uint value)//自己新添加
    {
        byte[] bytes = System.BitConverter.GetBytes(value);
        WriteBigEndian(bytes);
    }

    public void WriteShort(short value)
    {
        byte[] bytes = System.BitConverter.GetBytes((ushort)value);
        WriteBigEndian(bytes);
    }

    public void WriteUTF(string value)
    {
        UTF8Encoding utf8Encoding = new UTF8Encoding();
        int byteCount = utf8Encoding.GetByteCount(value);
        byte[] buffer = utf8Encoding.GetBytes(value);
        this.WriteShort((short)byteCount);
        if (buffer != null && buffer.Length > 0)
        {
            this.WriteBytes(buffer, 0, buffer.Length);
        }
    }
}
