using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AvaloniaHex.Document;

namespace GalaxyBudsClient.Model;

public class InMemoryBinaryDocument : IBinaryDocument
{
    private readonly List<byte> _data = [];
    
    public void Flush()
    {
    }
    
    public void Dispose()
    {
    }
    
    public void ReadBytes(ulong offset, Span<byte> buffer)
    {
        CollectionsMarshal.AsSpan(_data).Slice((int)offset, buffer.Length).CopyTo(buffer);
    }

    public void WriteBytes(ulong offset, ReadOnlySpan<byte> buffer)
    {
        _data.RemoveRange((int)offset, buffer.Length);
        _data.InsertRange((int)offset, buffer);
    }

    public void InsertBytes(ulong offset, ReadOnlySpan<byte> buffer)
    {
        _data.InsertRange((int)offset, buffer);
    }

    public void RemoveBytes(ulong offset, ulong length)
    {
        _data.RemoveRange((int)offset, (int)length);
    }

    public ulong Length => (ulong)_data.Count;
    public bool IsReadOnly => false;
    public bool CanInsert => true;
    public bool CanRemove => true;
    public IReadOnlyBitRangeUnion ValidRanges => 
        new BitRangeUnion(new List<BitRange>{new (0UL, Length)}.AsReadOnly());

#pragma warning disable CS0067
    public event EventHandler<BinaryDocumentChange>? Changed;
#pragma warning restore CS0067
}

public static class BinaryDocumentExtensions
{
    public static void WriteAllToStream(this IBinaryDocument document, Stream stream)
    {
        for (ulong i = 0; i < document.Length; i += 0x100)
        {
            var count = Math.Min(document.Length - i, 0x100);
            var buffer = new Span<byte>(new byte[count]);
            document.ReadBytes(i, buffer);
            stream.Write(buffer);
        }
    }
}
