using System.Text;

namespace GalaxyBudsClient.Utils
{
    public class HexUtils
    {
        private readonly byte[] _bytes;
        private readonly int _bytesPerLine;
        private readonly bool _showHeader;
        private readonly bool _showOffset;
        private readonly bool _showAscii;

        private readonly int _length;

        private int _index;
        private readonly StringBuilder _sb = new StringBuilder();

        private HexUtils(byte[] bytes, int bytesPerLine, bool showHeader, bool showOffset, bool showAscii)
        {
            _bytes = bytes;
            _bytesPerLine = bytesPerLine;
            _showHeader = showHeader;
            _showOffset = showOffset;
            _showAscii = showAscii;
            _length = bytes.Length;
        }

        public static string Dump(byte[]? bytes, int bytesPerLine = 16, bool showHeader = true, bool showOffset = true, bool showAscii = true)
        {
            if (bytes == null)
            {
                return "<null>";
            }
            return (new HexUtils(bytes, bytesPerLine, showHeader, showOffset, showAscii)).Dump();
        }
        public static string DumpAscii(byte[]? bytes)
        {
            if (bytes == null)
            {
                return "<null>";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(Translate(bytes[i]));
            }
            return sb.ToString();
        }

        private string Dump()
        {
            if (_showHeader)
            {
                WriteHeader();
            }
            WriteBody();
            return _sb.ToString();
        }

        private void WriteHeader()
        {
            if (_showOffset)
            {
                _sb.Append("Offset(h)  ");
            }
            for (int i = 0; i < _bytesPerLine; i++)
            {
                _sb.Append($"{i & 0xFF:X2}");
                if (i + 1 < _bytesPerLine)
                {
                    _sb.Append(" ");
                }
            }
            _sb.AppendLine();
        }

        private void WriteBody()
        {
            while (_index < _length)
            {
                if (_index % _bytesPerLine == 0)
                {
                    if (_index > 0)
                    {
                        if (_showAscii)
                        {
                            WriteAscii();
                        }
                        _sb.AppendLine();
                    }

                    if (_showOffset)
                    {
                        WriteOffset();
                    }
                }

                WriteByte();
                if (_index % _bytesPerLine != 0 && _index < _length)
                {
                    _sb.Append(" ");
                }
            }

            if (_showAscii)
            {
                WriteAscii();
            }
        }

        private void WriteOffset()
        {
            _sb.Append($"{_index:X8}   ");
        }

        private void WriteByte()
        {
            _sb.Append($"{_bytes[_index]:X2}");
            _index++;
        }

        private void WriteAscii()
        {
            int backtrack = ((_index - 1) / _bytesPerLine) * _bytesPerLine;
            int length = _index - backtrack;

            // This is to fill up last string of the dump if it's shorter than _bytesPerLine
            _sb.Append(new string(' ', (_bytesPerLine - length) * 3));

            _sb.Append("   ");
            for (int i = 0; i < length; i++)
            {
                _sb.Append(Translate(_bytes[backtrack + i]));
            }
        }

        private static string Translate(byte b)
        {
            return b < 32 ? "." : Encoding.GetEncoding(1252).GetString(new[] { b });
        }
    }
}
