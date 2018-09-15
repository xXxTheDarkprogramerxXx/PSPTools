/*
This file is part of pspsharp.

pspsharp is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

pspsharp is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with pspsharp.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace pspsharp.network
{
	//using Logger = org.apache.log4j.Logger;

	using sceNetInet = pspsharp.HLE.modules.sceNetInet;

	using RawSocket = com.savarese.rocksaw.net.RawSocket;

	public class SelectableRawSocket : RawSocket
	{
		protected internal static Logger log = sceNetInet.log;

		public virtual bool SelectedForRead
		{
			get
			{
				int result = __select(__socket, true, 0, 0);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("SelectableRawSocket.isSelectedForRead: {0:D}", result));
				}
				return result >= 0;
			}
		}

		public virtual bool SelectedForWrite
		{
			get
			{
				int result = __select(__socket, false, 0, 0);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("SelectableRawSocket.isSelectedForWrite: {0:D}", result));
				}
				return result >= 0;
			}
		}
	}

}