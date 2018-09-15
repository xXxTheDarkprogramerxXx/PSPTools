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
namespace pspsharp.HLE.modules
{
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using Utilities = pspsharp.util.Utilities;
	//using Logger = org.apache.log4j.Logger;

	public class sceAdler : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceAdler");
		// Do not use the JDK Adler32 implementation as we need to specify the initial checksum value.
		// This value is always forced to 1 in the JDK Adler32 implementation.
		protected internal Adler32 adler32;

		public override void start()
		{
			adler32 = new Adler32();

			base.start();
		}

		public override void stop()
		{
			adler32 = null;

			base.stop();
		}

		[HLEFunction(nid : 0x9702EF11, version : 150)]
		public virtual int sceAdler32(int adler, TPointer data, int Length)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceAdler32 data:{0}", Utilities.getMemoryDump(data.Address, Length)));
			}

			sbyte[] b = new sbyte[Length];
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(data.Address, Length, 1);
			for (int i = 0; i < Length; i++)
			{
				b[i] = (sbyte) memoryReader.readNext();
			}

			adler32.reset();
			adler32.update(adler);
			adler32.update(b);
			int result = (int) adler32.Value;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAdler32 returning 0x{0:X8}", result));
			}

			return result;
		}
	}

}