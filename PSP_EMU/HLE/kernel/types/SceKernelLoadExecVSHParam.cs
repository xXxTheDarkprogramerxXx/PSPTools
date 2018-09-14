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

	public class SceKernelLoadExecVSHParam : pspAbstractMemoryMappedStructureVariableLength
	{
		public int args;
		public int argp;
		public TPointer keyAddr;
		public string key;
		public int vshmainArgsSize;
		public int vshmainArgs;
		public TPointer configFileAddr;
		public string configFile;
		public int unknownString;
		public int flags;
		public int extArgs;
		public int extArgp;
		public int opt11;

		protected internal override void read()
		{
			base.read();
			args = read32();
			argp = read32();
			keyAddr = readPointer();
			vshmainArgsSize = read32();
			vshmainArgs = read32();
			configFileAddr = readPointer();
			unknownString = read32();
			flags = read32();

			if (@sizeof() >= 48)
			{
				extArgs = read32();
				extArgp = read32();
				opt11 = read32();
			}
			key = keyAddr.StringZ;
			configFile = configFileAddr.StringZ;
		}

		protected internal override void write()
		{
			base.write();
			write32(args);
			write32(argp);
			writePointer(keyAddr);
			write32(vshmainArgsSize);
			write32(vshmainArgs);
			writePointer(configFileAddr);
			write32(unknownString);
			write32(flags);

			if (@sizeof() >= 48)
			{
				write32(extArgs);
				write32(extArgp);
				write32(opt11);
			}
		}

		public override string ToString()
		{
			return string.Format("args=0x{0:X}, argp=0x{1:X8}, key={2}('{3}'), vshmainArgsSize=0x{4:X}, vshmainArgs=0x{5:X8}, configFile={6}('{7}'), unknownString=0x{8:X8}, flags=0x{9:X}, extArgs=0x{10:X}, extArgp=0x{11:X8}, opt11=0x{12:X}, vshmainArgs: {13}", args, argp, keyAddr, key, vshmainArgsSize, vshmainArgs, configFileAddr, configFile, unknownString, flags, extArgs, extArgp, opt11, Utilities.getMemoryDump(vshmainArgs, vshmainArgsSize));
		}
	}

}