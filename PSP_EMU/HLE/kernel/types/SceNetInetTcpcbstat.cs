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
	public class SceNetInetTcpcbstat : pspAbstractMemoryMappedStructure
	{
		public int next;
		public int ts_so_snd_sb_cc;
		public int ts_so_rcv_sb_cc;
		public int ts_inp_laddr;
		public int ts_inp_faddr;
		public int ts_inp_lport;
		public int ts_inp_fport;
		public int ts_t_state;

		protected internal override void read()
		{
			next = read32();
			ts_so_snd_sb_cc = read32();
			ts_so_rcv_sb_cc = read32();
			ts_inp_laddr = read32();
			ts_inp_faddr = read32();
			ts_inp_lport = endianSwap16((short) read16());
			ts_inp_fport = endianSwap16((short) read16());
			ts_t_state = read16();
		}

		protected internal override void write()
		{
			write32(next);
			write32(ts_so_snd_sb_cc);
			write32(ts_so_rcv_sb_cc);
			write32(ts_inp_laddr);
			write32(ts_inp_faddr);
			write16((short) endianSwap16((short) ts_inp_lport));
			write16((short) endianSwap16((short) ts_inp_fport));
			write16((short) ts_t_state);
		}

		public override int @sizeof()
		{
			// Aligned on 32-bit
			return 28;
		}
	}

}