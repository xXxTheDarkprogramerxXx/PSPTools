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
namespace pspsharp.network.pspsharp
{
	using AdhocDatagramSocket = pspsharp.network.adhoc.AdhocDatagramSocket;
	using AdhocSocket = pspsharp.network.adhoc.AdhocSocket;
	using PdpObject = pspsharp.network.adhoc.PdpObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class JpcspPdpObject : PdpObject
	{
		public JpcspPdpObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
		}

		protected internal override AdhocSocket createSocket()
		{
			return new AdhocDatagramSocket();
		}
	}

}