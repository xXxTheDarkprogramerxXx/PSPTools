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
namespace pspsharp.mediaengine
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.signExtend;

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerMeDecoderQuSpectra : MMIOHandlerMeBase
	{
		private const int STATE_VERSION = 0;
		private int control;
		private int numSpecs;
		private int groupSize;
		private int numCoeffs;
		private int tabBits;
		private int vlcBits;
		private int unknown18;
		private int inputBuffer;
		private int bitIndex;
		private int vlcTableCode;
		private int vlsTableN;
		private int unknown2C;
		private int outputBuffer;
		private int outputBufferSize; // ???, always 0x2000
		private int cacheAddress;
		private int cacheValue8;

		public MMIOHandlerMeDecoderQuSpectra(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			control = stream.readInt();
			numSpecs = stream.readInt();
			groupSize = stream.readInt();
			numCoeffs = stream.readInt();
			tabBits = stream.readInt();
			vlcBits = stream.readInt();
			unknown18 = stream.readInt();
			inputBuffer = stream.readInt();
			bitIndex = stream.readInt();
			vlcTableCode = stream.readInt();
			vlsTableN = stream.readInt();
			unknown2C = stream.readInt();
			outputBuffer = stream.readInt();
			outputBufferSize = stream.readInt();
			cacheAddress = stream.readInt();
			cacheValue8 = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(control);
			stream.writeInt(numSpecs);
			stream.writeInt(groupSize);
			stream.writeInt(numCoeffs);
			stream.writeInt(tabBits);
			stream.writeInt(vlcBits);
			stream.writeInt(unknown18);
			stream.writeInt(inputBuffer);
			stream.writeInt(bitIndex);
			stream.writeInt(vlcTableCode);
			stream.writeInt(vlsTableN);
			stream.writeInt(unknown2C);
			stream.writeInt(outputBuffer);
			stream.writeInt(outputBufferSize);
			stream.writeInt(cacheAddress);
			stream.writeInt(cacheValue8);
			base.write(stream);
		}

		private int Control
		{
			set
			{
				// Flag 0x1 is "running"?
				// Flag 0x2 is "error"?
				// Flag 0x100/0x200 is signed/unsigned?
				this.control = value & ~0x3;
    
				if ((value & 0x1) != 0)
				{
					decodeQuSpectra();
				}
			}
		}

		private bool Signed
		{
			get
			{
				return (control & 0x300) == 0x100;
			}
		}

		private bool readBool()
		{
			return read1() != 0;
		}

		private int read1()
		{
			int address = inputBuffer + (bitIndex >> 3);
			if (address != cacheAddress)
			{
				cacheAddress = address;
				cacheValue8 = Memory.read8(cacheAddress);
			}

			int bit = (cacheValue8 >> (7 - (bitIndex & 0x7))) & 0x1;

			bitIndex++;

			return bit;
		}

		private int read(int n)
		{
			int read = 0;
			for (; n > 0; n--)
			{
				read = (read << 1) + read1();
			}

			return read;
		}

		private int peek(int n)
		{
			int bitIndex = this.bitIndex;
			int value = read(n);
			this.bitIndex = bitIndex;

			return value;
		}

		private void skip(int n)
		{
			bitIndex += n;
		}

		private int getVLC2(int maxDepth)
		{
			int index = peek(vlcBits);
			int code = Memory.read8(vlcTableCode + index);
			int n = (int)(sbyte) Memory.read8(vlsTableN + code);

			if (maxDepth > 1 && n < 0)
			{
				skip(vlcBits);

				int nbBits = -n;

				index = peek(nbBits) + code;
				code = Memory.read8(vlcTableCode + index);
				n = (int)(sbyte) Memory.read8(vlsTableN + code);
				if (maxDepth > 2 && n < 0)
				{
					skip(nbBits);

					nbBits = -n;

					index = peek(nbBits) + code;
					code = Memory.read8(vlcTableCode + index);
					n = (int)(sbyte) Memory.read8(vlsTableN + code);
				}
			}
			skip(n);

			return code;
		}

		private int VLC2
		{
			get
			{
				return getVLC2(1);
			}
		}

		// See pspsharp.media.codec.atrac3plus.ChannelUnit.decodeQuSpectra()
		private void decodeQuSpectra()
		{
			cacheAddress = -1;
			int mask = (1 << tabBits) - 1;
			bool isSigned = this.Signed;

			for (int pos = 0; pos < numSpecs;)
			{
				if (groupSize == 1 || readBool())
				{
					for (int j = 0; j < groupSize; j++)
					{
						int val = VLC2;

						for (int i = 0; i < numCoeffs; i++)
						{
							int cf = val & mask;
							if (isSigned)
							{
								cf = signExtend(cf, tabBits);
							}
							else if (cf != 0 && readBool())
							{
								cf = -cf;
							}

							Memory.write32(outputBuffer + (pos << 2), cf);
							pos++;
							val >>= tabBits;
						}
					}
				}
				else
				{
					// Group skipped
					pos += groupSize * numCoeffs;
				}
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = control;
					break;
				case 0x20:
					value = bitIndex;
					break;
				default:
					value = base.read32(address);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) returning 0x{2:X8}", Pc - 4, address, value));
			}

			return value;
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x00:
					Control = value;
					break;
				case 0x04:
					numSpecs = value;
					break;
				case 0x08:
					groupSize = value;
					break;
				case 0x0C:
					numCoeffs = value;
					break;
				case 0x10:
					tabBits = value;
					break;
				case 0x14:
					vlcBits = value;
					break;
				case 0x18:
					unknown18 = value;
					break;
				case 0x1C:
					inputBuffer = value;
					break;
				case 0x20:
					bitIndex = value;
					break;
				case 0x24:
					vlcTableCode = value;
					break;
				case 0x28:
					vlsTableN = value;
					break;
				case 0x2C:
					unknown2C = value;
					break;
				case 0x30:
					outputBuffer = value;
					break;
				case 0x3C:
					outputBufferSize = value;
					break;
				default:
					base.write32(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc - 4, address, value, this));
			}
		}
	}

}