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
	public class pspGeCallbackData : pspAbstractMemoryMappedStructure
	{
		/// <summary>
		/// GE callback for the signal interrupt </summary>
		public int signalFunction;
		/// <summary>
		/// GE callback argument for signal interrupt </summary>
		public int signalArgument;
		/// <summary>
		/// GE callback for the finish interrupt </summary>
		public int finishFunction;
		/// <summary>
		/// GE callback argument for finish interrupt </summary>
		public int finishArgument;

		protected internal override void read()
		{
			signalFunction = read32();
			signalArgument = read32();
			finishFunction = read32();
			finishArgument = read32();
		}

		public override int @sizeof()
		{
			return 4 * 4;
		}

		protected internal override void write()
		{
			write32(signalFunction);
			write32(signalArgument);
			write32(finishFunction);
			write32(finishArgument);
		}
	}

}