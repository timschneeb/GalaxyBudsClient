using System.IO;
using Serilog;

namespace GalaxyBudsClient.Message.Decoder;

public abstract class BaseMessageDecoder : BaseMessageHandler
{
    protected BaseMessageDecoder(SppMessage msg)
    {
        TargetModel = msg.TargetModel;
    }
    
    /// <summary>
    /// Lê um byte de forma segura, retornando um valor padrão se não houver dados suficientes
    /// </summary>
    protected static byte SafeReadByte(BinaryReader reader, byte defaultValue = 0)
    {
        try
        {
            return reader.ReadByte();
        }
        catch (EndOfStreamException)
        {
            Log.Warning("EndOfStreamException ao ler byte - usando valor padrão {DefaultValue}", defaultValue);
            return defaultValue;
        }
    }
    
    /// <summary>
    /// Verifica se há bytes suficientes disponíveis no stream
    /// </summary>
    protected static bool HasBytesAvailable(BinaryReader reader, int bytesNeeded)
    {
        return reader.BaseStream.Length - reader.BaseStream.Position >= bytesNeeded;
    }
}