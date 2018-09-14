using System.Text;

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

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerMe0F8000 : MMIOHandlerMeBase
	{
		private const int STATE_VERSION = 0;
		private int unknown000;
		private int unknown004;
		private int unknown008;
		private int unknown00C;
		private int unknown074;
		private int unknown088;
		private int unknown08C;
		private int unknown094;
		private int unknown09C;
		private int unknown0A0;
		private int unknown0A4;
		private int unknown0AC;
		private int unknown0B4;
		private int unknown0B8;
		private int unknown0BC;
		private int unknown0C4;
		private int unknown0E4;
		private int unknown0E8;
		private int unknown0F4;
		private int unknown0FC;
		private int unknown100;
		private int unknown118;
		private int unknown12C;
		private int unknown130;
		private int unknown144;
		private int unknown148;
		private int unknown174;
		private int unknown178;
		private int unknown18C;
		private int unknown190;

		public MMIOHandlerMe0F8000(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			unknown000 = stream.readInt();
			unknown004 = stream.readInt();
			unknown008 = stream.readInt();
			unknown00C = stream.readInt();
			unknown074 = stream.readInt();
			unknown088 = stream.readInt();
			unknown08C = stream.readInt();
			unknown094 = stream.readInt();
			unknown09C = stream.readInt();
			unknown0A0 = stream.readInt();
			unknown0A4 = stream.readInt();
			unknown0AC = stream.readInt();
			unknown0B4 = stream.readInt();
			unknown0B8 = stream.readInt();
			unknown0BC = stream.readInt();
			unknown0C4 = stream.readInt();
			unknown0E4 = stream.readInt();
			unknown0E8 = stream.readInt();
			unknown0F4 = stream.readInt();
			unknown0FC = stream.readInt();
			unknown100 = stream.readInt();
			unknown118 = stream.readInt();
			unknown12C = stream.readInt();
			unknown130 = stream.readInt();
			unknown144 = stream.readInt();
			unknown148 = stream.readInt();
			unknown174 = stream.readInt();
			unknown178 = stream.readInt();
			unknown18C = stream.readInt();
			unknown190 = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(unknown000);
			stream.writeInt(unknown004);
			stream.writeInt(unknown008);
			stream.writeInt(unknown00C);
			stream.writeInt(unknown074);
			stream.writeInt(unknown088);
			stream.writeInt(unknown08C);
			stream.writeInt(unknown094);
			stream.writeInt(unknown09C);
			stream.writeInt(unknown0A0);
			stream.writeInt(unknown0A4);
			stream.writeInt(unknown0AC);
			stream.writeInt(unknown0B4);
			stream.writeInt(unknown0B8);
			stream.writeInt(unknown0BC);
			stream.writeInt(unknown0C4);
			stream.writeInt(unknown0E4);
			stream.writeInt(unknown0E8);
			stream.writeInt(unknown0F4);
			stream.writeInt(unknown0FC);
			stream.writeInt(unknown100);
			stream.writeInt(unknown118);
			stream.writeInt(unknown12C);
			stream.writeInt(unknown130);
			stream.writeInt(unknown144);
			stream.writeInt(unknown148);
			stream.writeInt(unknown174);
			stream.writeInt(unknown178);
			stream.writeInt(unknown18C);
			stream.writeInt(unknown190);
			base.write(stream);
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x000:
					unknown000 = value;
					break;
				case 0x004:
					unknown004 = value;
					break;
				case 0x008:
					unknown008 = value;
					break;
				case 0x00C:
					unknown00C = value;
					break;
				case 0x074:
					unknown074 = value;
					break;
				case 0x088:
					unknown088 = value;
					break;
				case 0x08C:
					unknown08C = value;
					break;
				case 0x094:
					unknown094 = value;
					break;
				case 0x09C:
					unknown09C = value;
					break;
				case 0x0A0:
					unknown0A0 = value;
					break;
				case 0x0A4:
					unknown0A4 = value;
					break;
				case 0x0AC:
					unknown0AC = value;
					break;
				case 0x0B4:
					unknown0B4 = value;
					break;
				case 0x0B8:
					unknown0B8 = value;
					break;
				case 0x0BC:
					unknown0BC = value;
					break;
				case 0x0C4:
					unknown0C4 = value;
					break;
				case 0x0E4:
					unknown0E4 = value;
					break;
				case 0x0E8:
					unknown0E8 = value;
					break;
				case 0x0F4:
					unknown0F4 = value;
					break;
				case 0x0FC:
					unknown0FC = value;
					break;
				case 0x100:
					unknown100 = value;
					break;
				case 0x118:
					unknown118 = value;
					break;
				case 0x12C:
					unknown12C = value;
					break;
				case 0x130:
					unknown130 = value;
					break;
				case 0x144:
					unknown144 = value;
					break;
				case 0x148:
					unknown148 = value;
					break;
				case 0x174:
					unknown174 = value;
					break;
				case 0x178:
					unknown178 = value;
					break;
				case 0x18C:
					unknown18C = value;
					break;
				case 0x190:
					unknown190 = value;
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

		public override string ToString()
		{
			StringBuilder s = new StringBuilder("MMIOHandlerMe0F8000 ");

			s.Append(string.Format("unknown000=0x{0:X}", unknown000));
			s.Append(string.Format(", unknown004=0x{0:X}", unknown004));
			s.Append(string.Format(", unknown008=0x{0:X}", unknown008));
			s.Append(string.Format(", unknown00C=0x{0:X}", unknown00C));
			s.Append(string.Format(", unknown074=0x{0:X}", unknown074));
			s.Append(string.Format(", unknown088=0x{0:X}", unknown088));
			s.Append(string.Format(", unknown08C=0x{0:X}", unknown08C));
			s.Append(string.Format(", unknown094=0x{0:X}", unknown094));
			s.Append(string.Format(", unknown09C=0x{0:X}", unknown09C));
			s.Append(string.Format(", unknown0A0=0x{0:X}", unknown0A0));
			s.Append(string.Format(", unknown0A4=0x{0:X}", unknown0A4));
			s.Append(string.Format(", unknown0AC=0x{0:X}", unknown0AC));
			s.Append(string.Format(", unknown0B4=0x{0:X}", unknown0B4));
			s.Append(string.Format(", unknown0B8=0x{0:X}", unknown0B8));
			s.Append(string.Format(", unknown0BC=0x{0:X}", unknown0BC));
			s.Append(string.Format(", unknown0C4=0x{0:X}", unknown0C4));
			s.Append(string.Format(", unknown0E4=0x{0:X}", unknown0E4));
			s.Append(string.Format(", unknown0E8=0x{0:X}", unknown0E8));
			s.Append(string.Format(", unknown0F4=0x{0:X}", unknown0F4));
			s.Append(string.Format(", unknown0FC=0x{0:X}", unknown0FC));
			s.Append(string.Format(", unknown100=0x{0:X}", unknown100));
			s.Append(string.Format(", unknown118=0x{0:X}", unknown118));
			s.Append(string.Format(", unknown12C=0x{0:X}", unknown12C));
			s.Append(string.Format(", unknown130=0x{0:X}", unknown130));
			s.Append(string.Format(", unknown144=0x{0:X}", unknown144));
			s.Append(string.Format(", unknown148=0x{0:X}", unknown148));
			s.Append(string.Format(", unknown174=0x{0:X}", unknown174));
			s.Append(string.Format(", unknown178=0x{0:X}", unknown178));
			s.Append(string.Format(", unknown18C=0x{0:X}", unknown18C));
			s.Append(string.Format(", unknown190=0x{0:X}", unknown190));

			return s.ToString();
		}
	}

}