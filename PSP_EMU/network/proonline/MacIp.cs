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
namespace pspsharp.network.proonline
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.proonline.ProOnlineNetworkAdapter.convertIpToString;


	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MacIp
	{
		public sbyte[] mac;
		public pspNetMacAddress macAddress;
		public int ip;
		public InetAddress inetAddress;

		public MacIp(sbyte[] mac, int ip)
		{
			Mac = mac;
			Ip = ip;
		}

		public virtual sbyte[] Mac
		{
			set
			{
				this.mac = value.Clone();
				macAddress = new pspNetMacAddress(this.mac);
			}
		}

		public virtual int Ip
		{
			set
			{
				this.ip = value;
				try
				{
					inetAddress = InetAddress.getByAddress(getRawIp(value));
				}
				catch (UnknownHostException e)
				{
					ProOnlineNetworkAdapter.log.error("Incorrect IP", e);
				}
			}
		}

		public static sbyte[] getRawIp(int ip)
		{
			sbyte[] rawIp = new sbyte[4];
			rawIp[0] = (sbyte)(ip);
			rawIp[1] = (sbyte)(ip >> 8);
			rawIp[2] = (sbyte)(ip >> 16);
			rawIp[3] = (sbyte)(ip >> 24);

			return rawIp;
		}

		public override string ToString()
		{
			return string.Format("MAC={0}, ip={1}", macAddress, convertIpToString(ip));
		}
	}

}