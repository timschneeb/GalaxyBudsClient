// Copyright 2008 Alp Toker <alp@atoker.com>
// This software is made available under the MIT License
// See COPYING for details

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ThePBone.BlueZNet.Interop
{
    
    using SizeT = System.UIntPtr;
    using SSizeT = System.IntPtr;

    public sealed class UnixStream : Stream
    {
        private readonly UnixSocket _usock;

        public UnixStream (int fd)
        {
            this._usock = new UnixSocket (fd);
        }

        public UnixSocket Socket => _usock;
        
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException ("Seeking is not implemented");

        public override long Position
        {
            get => throw new NotImplementedException ("Seeking is not implemented");
            set => throw new NotImplementedException ("Seeking is not implemented");
        }

        public override long Seek (long offset, SeekOrigin origin) => throw new NotImplementedException ("Seeking is not implemented");

        public override void SetLength (long value) => throw new NotImplementedException ("Not implemented");

        public override void Flush (){ }

        public override int Read ([In, Out] byte[] buffer, int offset, int count)
        {
            return _usock.Read (buffer, offset, count);
        }
        
        public override void Write (byte[] buffer, int offset, int count)
        {
            _usock.Write (buffer, offset, count);
        }

        public override unsafe int ReadByte ()
        {
            byte value;
            _usock.Read (&value, 1);
            return value;
        }

        public override unsafe void WriteByte (byte value)
        {
            _usock.Write (&value, 1);
        }
    }
}