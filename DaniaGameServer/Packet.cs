using System.Text;

public sealed class PacketReader : IDisposable
{
    private ushort type;
    private byte[] readBuffer;
    private int readPosition;

    public ushort Type => type;

    public PacketReader(byte[] data)
    {
        readBuffer = data;
        readPosition = 0;
        type = Read<ushort>();
    }

    public T Read<T>() where T : IConvertible
    {
        T defaultValue = default(T);
        if (typeof(T) == typeof(string))
            defaultValue = (T)Convert.ChangeType("", typeof(T));

        switch (defaultValue)
        {
            default:
                throw new Exception($"Type '{typeof(T).Name}' is not supported.");

            case byte:
                byte byteValue = readBuffer[readPosition];
                readPosition += 1;
                return (T)Convert.ChangeType(byteValue, typeof(T));

            case int:
                int intValue = BitConverter.ToInt32(readBuffer, readPosition);
                readPosition += 4;
                return (T)Convert.ChangeType(intValue, typeof(T));

            case long:
                long longValue = BitConverter.ToInt64(readBuffer, readPosition);
                readPosition += 8;
                return (T)Convert.ChangeType(longValue, typeof(T));

            case short:
                short shortValue = BitConverter.ToInt16(readBuffer, readPosition);
                readPosition += 2;
                return (T)Convert.ChangeType(shortValue, typeof(T));

            case uint:
                uint uintValue = BitConverter.ToUInt32(readBuffer, readPosition);
                readPosition += 4;
                return (T)Convert.ChangeType(uintValue, typeof(T));

            case ulong:
                ulong ulongValue = BitConverter.ToUInt64(readBuffer, readPosition);
                readPosition += 8;
                return (T)Convert.ChangeType(ulongValue, typeof(T));

            case ushort:
                ushort ushortValue = BitConverter.ToUInt16(readBuffer, readPosition);
                readPosition += 2;
                return (T)Convert.ChangeType(ushortValue, typeof(T));

            case float:
                float floatValue = BitConverter.ToSingle(readBuffer, readPosition);
                readPosition += 4;
                return (T)Convert.ChangeType(floatValue, typeof(T));

            case double:
                double doubleValue = BitConverter.ToDouble(readBuffer, readPosition);
                readPosition += 8;
                return (T)Convert.ChangeType(doubleValue, typeof(T));

            case bool:
                bool boolValue = BitConverter.ToBoolean(readBuffer, readPosition);
                readPosition += 1;
                return (T)Convert.ChangeType(boolValue, typeof(T));

            case char:
                char charValue = BitConverter.ToChar(readBuffer, readPosition);
                readPosition += 2;
                return (T)Convert.ChangeType(charValue, typeof(T));

            case string:
                int stringLength = Read<int>();
                string stringValue = Encoding.UTF8.GetString(readBuffer, readPosition, stringLength);
                readPosition += stringLength;
                return (T)Convert.ChangeType(stringValue, typeof(T));
        }
    }

    public T[] ReadArray<T>() where T : IConvertible
    {
        int length = Read<int>();
        T[] array = new T[length];
        for (int i = 0; i < length; i++)
            array[i] = Read<T>();

        return array;
    }

    public void Dispose()
    {
        type = 0;
        readBuffer = null;
        readPosition = 0;
        GC.SuppressFinalize(this);
    }
}

public sealed class PacketWriter : IDisposable
{
    private List<byte> writeBuffer;

    public byte[] Bytes => writeBuffer.ToArray();

    public PacketWriter(ushort type)
    {
        writeBuffer = new List<byte>();
        Write(type);
    }

    public PacketWriter(Enum type)
    {
        writeBuffer = new List<byte>();
        Write(Convert.ToUInt16(type));
    }

    public void Write<T>(T value) where T : IConvertible
    {
        byte[] bytes = new byte[0];
        switch (value)
        {
            default:
                throw new Exception($"Type '{typeof(T).Name}' is not supported.");

            case byte byteValue:
                writeBuffer.Add(byteValue);
                break;

            case int intValue:
                bytes = BitConverter.GetBytes(intValue);
                break;

            case long longValue:
                bytes = BitConverter.GetBytes(longValue);
                break;

            case short shortValue:
                bytes = BitConverter.GetBytes(shortValue);
                break;

            case uint uintValue:
                bytes = BitConverter.GetBytes(uintValue);
                break;

            case ulong ulongValue:
                bytes = BitConverter.GetBytes(ulongValue);
                break;

            case ushort ushortValue:
                bytes = BitConverter.GetBytes(ushortValue);
                break;

            case float floatValue:
                bytes = BitConverter.GetBytes(floatValue);
                break;

            case double doubleValue:
                bytes = BitConverter.GetBytes(doubleValue);
                break;

            case bool boolValue:
                bytes = BitConverter.GetBytes(boolValue);
                break;

            case char charValue:
                bytes = BitConverter.GetBytes(charValue);
                break;

            case string stringValue:
                byte[] stringBytes = Encoding.UTF8.GetBytes(stringValue);
                Write(stringBytes.Length);
                bytes = stringBytes;
                break;
        }

        if (bytes.Length > 0)
            writeBuffer.AddRange(bytes);
    }

    public void WriteArray<T>(params T[] array) where T : IConvertible
    {
        Write(array.Length);
        foreach(T item in array)
            Write(item);
    }

    public void Dispose()
    {
        writeBuffer = null;
        GC.SuppressFinalize(this);
    }
}