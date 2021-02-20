using System;
using System.IO;

namespace Utils
{
    class PairStream : Stream
    {
        readonly Stream _in;
        readonly Stream _out;

        public PairStream(Stream @in, Stream @out)
        {
            _in = @in;
            _out = @out;
        }

        public override bool CanRead
        {
            get { return _in.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _out.CanWrite; }
        }

        public override void Flush()
        {
            _out.Flush();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _in.Read(buffer, offset, count);
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _out.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing) {
                    try {
                        _in.Dispose();
                    } finally {
                        _out.Dispose();
                    }
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        // -- More overrides --
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _in.BeginRead(buffer, offset, count, callback, state);
        }
        public override int EndRead(IAsyncResult asyncResult)
        {
            return _in.EndRead(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _out.BeginWrite(buffer, offset, count, callback, state);
        }
        public override void EndWrite(IAsyncResult asyncResult)
        {
            _out.EndWrite(asyncResult);
        }

        //TODO PairStream override more?
    }
}
