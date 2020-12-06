// Copyright 2008 Alp Toker <alp@atoker.com>
// This software is made available under the MIT License
// See COPYING for details

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ThePBone.BlueZNet.Interop
{
	// size_t
	using SizeT = System.UIntPtr;
	// ssize_t
	using SSizeT = System.IntPtr;
	// socklen_t: assumed to be 4 bytes
	// uid_t: assumed to be 4 bytes

	//[StructLayout(LayoutKind.Sequential, Pack=1)]

	public unsafe class UnixSocket
	{
		const string LIBC = "libc";

		const short AF_UNIX = 1;

		const short SOCK_STREAM = 1;

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern int close (int fd);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern int socket (int domain, int type, int protocol);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern int connect (int sockfd, byte[] serv_addr, uint addrlen);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern int bind (int sockfd, byte[] my_addr, uint addrlen);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern int listen (int sockfd, int backlog);

		//TODO: this prototype is probably wrong, fix it
		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern int accept (int sockfd, void* addr, ref uint addrlen);
		
		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern SSizeT read (int fd, byte* buf, SizeT count);

		[DllImport (LIBC, CallingConvention=CallingConvention.Cdecl, SetLastError=true)]
		static extern SSizeT write (int fd, byte* buf, SizeT count);
		
		public int Handle;
		private readonly bool _ownsHandle = false;
		
		public UnixSocket (int handle, bool ownsHandle = false)
		{
			this.Handle = handle;
			this._ownsHandle = ownsHandle;
		}

		public UnixSocket ()
		{
			//TODO: don't hard-code PF_UNIX and SOCK_STREAM or SocketType.Stream
			//AddressFamily family, SocketType type, ProtocolType proto

			int r = socket (AF_UNIX, SOCK_STREAM, 0);
			if (r < 0)
				throw UnixError.GetLastUnixException ();

			Handle = r;
			_ownsHandle = true;
		}

		~UnixSocket ()
		{
			if (_ownsHandle && Handle > 0)
				Close ();
		}

		protected bool Connected = false;

		//TODO: consider memory management
		public void Close ()
		{
			int r = 0;

			do {
				r = close (Handle);
			} while (r < 0 && UnixError.ShouldRetry);

			if (r < 0)
				throw UnixError.GetLastUnixException ();

			Handle = -1;
			Connected = false;
		}

		public void Connect()
		{
			int r = 0;

			do {
				r = connect (Handle, null, 0);
			} while (r < 0 && UnixError.ShouldRetry);

			if (r < 0)
				throw UnixError.GetLastUnixException ();

			Connected = true;
		}

		//TODO: consider memory management
		public void Connect (byte[] remote_end)
		{
			int r = 0;

			do {
				r = connect (Handle, remote_end, (uint)remote_end.Length);
			} while (r < 0 && UnixError.ShouldRetry);

			if (r < 0)
				throw UnixError.GetLastUnixException ();

			Connected = true;
		}

		//assigns a name to the socket
		public void Bind (byte[] localEnd)
		{
			int r = bind (Handle, localEnd, (uint)localEnd.Length);
			if (r < 0)
				throw UnixError.GetLastUnixException ();
		}

		public void Listen (int backlog)
		{
			int r = listen (Handle, backlog);
			if (r < 0)
				throw UnixError.GetLastUnixException ();
		}

		public UnixSocket Accept ()
		{
			byte[] addr = new byte[110];
			uint addrlen = (uint)addr.Length;

			fixed (byte* addrP = addr) {
				int r = 0;

				do {
					r = accept (Handle, addrP, ref addrlen);
				} while (r < 0 && UnixError.ShouldRetry);

				if (r < 0)
					throw UnixError.GetLastUnixException ();
				
				return new UnixSocket (r, true);
			}
		}

		public int Read (byte[] buf, int offset, int count)
		{
			fixed (byte* bufP = buf)
				return Read (bufP + offset, count);
		}

		public int Write (byte[] buf, int offset, int count)
		{
			fixed (byte* bufP = buf)
				return Write (bufP + offset, count);
		}

		public int Read (byte* bufP, int count)
		{
			int r = 0;

			do 
			{
				r = (int)read (Handle, bufP, (SizeT)count);
			}
			while (r < 0 && UnixError.ShouldRetry);

			/* Read was successful */
			if (r >= 0)
				return r;
			
			var error = Marshal.GetLastWin32Error();

			if (error == 11)  /* EAGAIN (ignore for blocking sockets) */
			{
				return -error;
			}
			else if (error >= 103 && error <= 113) /* Bundle common socket errno identifiers */
			{
				/* ECONNABORTED, ECONNRESET, ENOBUFS, ETIMEDOUT, ... */
				Console.WriteLine ("IO-Error " + error);
				throw new UnixSocketException(error);
			}
			else if (error == 9) /* EBADF */
			{
				Console.WriteLine ("Socket closed: bad file descriptor");
				throw new UnixSocketException(error);
			}
			else
			{
				Console.WriteLine ("Error " + error);
				throw UnixError.GetLastUnixException ();
			}

			return r;
		}

		public int Write (byte* bufP, int count)
		{
			int r = 0;

			do
			{
				r = (int)write (Handle, bufP, (SizeT)count);
			}
			while (r < 0 && UnixError.ShouldRetry);

			if (r < 0)
			{
				throw UnixError.GetLastUnixException();
			}

			return r;
		}
		
	}
}