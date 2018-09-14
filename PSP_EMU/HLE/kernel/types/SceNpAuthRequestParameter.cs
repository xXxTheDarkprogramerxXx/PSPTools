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
namespace pspsharp.HLE.kernel.types
{
	using Utilities = pspsharp.util.Utilities;

	// Based on https://github.com/RPCS3/rpcs3/blob/master/rpcs3/Emu/PSP2/Modules/sceNpCommon.h
	public class SceNpAuthRequestParameter : pspAbstractMemoryMappedStructureVariableLength
	{
		public int ticketVersionMajor;
		public int ticketVersionMinor;
		public string serviceId;
		public int serviceIdAddr;
		public int cookie;
		public int cookieSize;
		public string entitlementId;
		public int entitlementIdAddr;
		public int consumedCount;
		public int ticketCallback;
		public int callbackArgument;

		protected internal override void read()
		{
			base.read();
			ticketVersionMajor = read16();
			ticketVersionMinor = read16();
			serviceIdAddr = read32();
			cookie = read32();
			cookieSize = read32();
			entitlementIdAddr = read32();
			consumedCount = read32();
			ticketCallback = read32();
			callbackArgument = read32();

			if (serviceIdAddr != 0)
			{
				serviceId = Utilities.readStringZ(serviceIdAddr);
			}
			else
			{
				serviceId = null;
			}
			if (entitlementIdAddr != 0)
			{
				entitlementId = Utilities.readStringZ(entitlementIdAddr);
			}
			else
			{
				entitlementId = null;
			}
		}

		protected internal override void write()
		{
			base.write();
			write16((short) ticketVersionMajor);
			write16((short) ticketVersionMinor);
			write32(serviceIdAddr);
			write32(cookie);
			write32(cookieSize);
			write32(entitlementIdAddr);
			write32(consumedCount);
			write32(ticketCallback);
			write32(callbackArgument);
		}

		public override string ToString()
		{
			return string.Format("serviceId='{0}', cookie=0x{1:X8}(size=0x{2:X}), entitlementId='{3}', consumedCount=0x{4:X}, ticketCallback=0x{5:X8}, callbackArgument=0x{6:X8}", string.ReferenceEquals(serviceId, null) ? "" : serviceId, cookie, cookieSize, string.ReferenceEquals(entitlementId, null) ? "" : entitlementId, consumedCount, ticketCallback, callbackArgument);
		}
	}

}