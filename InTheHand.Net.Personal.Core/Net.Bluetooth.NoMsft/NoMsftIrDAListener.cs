#if NO_MSFT
using System;

namespace InTheHand.Net.Sockets
{
	internal class IrDAListener
	{
		private IrDAListener ()
		{
		}

		internal void Start()
		{
			throw new NotSupportedException();
		}

		internal void Stop()
		{
			throw new NotSupportedException();
		}

		internal System.Net.Sockets.TcpClient AcceptIrDAClient()
		{
			throw new NotSupportedException();
		}
	
	}
}
#endif