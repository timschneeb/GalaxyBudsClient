using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand
{
    /// <summary>
    /// For use on NETCFv2
    /// </summary>
    /// -
    /// <inheritdoc/>
    public class TextWriterTraceListener32f : System.Diagnostics.TraceListener
    {
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member
        readonly System.IO.TextWriter wtr;
        volatile bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:InTheHand.TextWriterTraceListener32f"/> class
        /// </summary>
        /// -
        /// <param name="filename">The filename of the log file to write to.
        /// Unlike the .NET supplied class this filename is relative to the
        /// folder that the calling assembly is located in.
        /// </param>
        public TextWriterTraceListener32f(string filename)
        {
            string pathname = System.IO.Path.Combine(GetCurrentFolder(), filename);
            wtr = System.IO.File.AppendText(pathname);
        }

        static string GetCurrentFolder()
        {
            string fullAppName = System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase;
            string fullAppPath = System.IO.Path.GetDirectoryName(fullAppName);
            return fullAppPath;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
            try {
                wtr.Close();
            } finally {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            if (disposed)
                return;
            wtr.Flush();
            base.Flush();
        }

        public override void Write(string message)
        {
            if (disposed)
                return;
            wtr.Write(message);
            if (System.Diagnostics.Debug.AutoFlush) {
                Flush();
            }
        }

        public override void WriteLine(string message)
        {
            if (disposed)
                return;
            wtr.WriteLine(message);
            if (System.Diagnostics.Debug.AutoFlush) {
                Flush();
            }
        }
    }
}
