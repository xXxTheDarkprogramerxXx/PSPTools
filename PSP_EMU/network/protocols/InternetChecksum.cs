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
namespace pspsharp.network.protocols
{

	/*
	 * Computes the "Internet Checksum" as defined by RFC 1071.
	 * See https://tools.ietf.org/html/rfc1071
	 */
	public class InternetChecksum
	{
		public static int computeInternetChecksum(sbyte[] buffer, int offset, int length)
		{
			NetPacket packet = new NetPacket(buffer, offset, length);
			int sum = 0;
			try
			{
				while (length > 1)
				{
					sum += packet.read16();
					length -= 2;
				}

				// Add left-over byte, if any
				if (length > 0)
				{
					sum += packet.read8() << 8;
				}
			}
			catch (EOFException)
			{
				// Ignore exception
			}

			// Add the carry
			while (((int)((uint)sum >> 16)) != 0)
			{
				sum = (sum & 0xFFFF) + ((int)((uint)sum >> 16));
			}

			// Flip all the bits to obtain the checksum
			int checksum = sum ^ 0xFFFF;

			return checksum;
		}
	}

}