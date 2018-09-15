using System.Collections.Generic;
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
namespace pspsharp.format
{

	using Utilities = pspsharp.util.Utilities;

	public class Elf32
	{
		// File offset
		private int elfOffset;
		private bool kernelMode;

		// Headers
		private Elf32Header header;
		private IList<Elf32ProgramHeader> programHeaderList;
		private IList<Elf32SectionHeader> sectionHeaderList;
		private Dictionary<string, Elf32SectionHeader> sectionHeaderMap;
		private Elf32SectionHeader shstrtab;

		// Debug info
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private string ElfInfo_Renamed; // ELF header
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private string ProgInfo_Renamed; // ELF program headers
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private string SectInfo_Renamed; // ELF section headers

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Elf32(ByteBuffer f) throws java.io.IOException
		public Elf32(ByteBuffer f)
		{
			elfOffset = f.position();
			loadHeader(f);
			if (header.Valid)
			{
				loadProgramHeaders(f);
				loadSectionHeaders(f);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void loadHeader(ByteBuffer f) throws java.io.IOException
		private void loadHeader(ByteBuffer f)
		{
			header = new Elf32Header(f);
			ElfInfo_Renamed = header.ToString();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void loadProgramHeaders(ByteBuffer f) throws java.io.IOException
		private void loadProgramHeaders(ByteBuffer f)
		{
			programHeaderList = new LinkedList<Elf32ProgramHeader>();
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < header.E_phnum; i++)
			{
				f.position(elfOffset + header.E_phoff + (i * header.E_phentsize));
				Elf32ProgramHeader phdr = new Elf32ProgramHeader(f);

				// Save loaded header
				programHeaderList.Add(phdr);

				// Construct ELF program header info for debugger
				sb.Append("-----PROGRAM HEADER #" + i + "-----" + "\n");
				sb.Append(phdr.ToString());

				// yapspd: if the PRX file is a kernel module then the most significant
				// bit must be set in the phsyical address of the first program header.
				if (i == 0 && (phdr.P_paddr & 0x80000000) != 0)
				{
					kernelMode = true;
					Emulator.Console.WriteLine("Kernel mode PRX detected");
				}
			}

			ProgInfo_Renamed = sb.ToString();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void loadSectionHeaders(ByteBuffer f) throws java.io.IOException
		private void loadSectionHeaders(ByteBuffer f)
		{
			sectionHeaderList = new LinkedList<Elf32SectionHeader>();
			sectionHeaderMap = new Dictionary<string, Elf32SectionHeader>();

			// 1st pass
			// - save headers
			// - find .shstrtab
			for (int i = 0; i < header.E_shnum; i++)
			{
				f.position(elfOffset + header.E_shoff + (i * header.E_shentsize));
				Elf32SectionHeader shdr = new Elf32SectionHeader(f);

				// Save loaded header
				sectionHeaderList.Add(shdr);

				// Find the .shstrtab section
				if (shdr.Sh_type == Elf32SectionHeader.SHT_STRTAB && shstrtab == null && shdr.Sh_size > 1)
				{
					shstrtab = shdr;
				}
			}

			if (shstrtab == null)
			{
				Emulator.Console.WriteLine(".shstrtab section not found");
				return;
			}

			// 2nd pass
			// - Construct ELF section header info for debugger
			StringBuilder sb = new StringBuilder();
			int SectionCounter = 0;
			foreach (Elf32SectionHeader shdr in sectionHeaderList)
			{
				int position = elfOffset + shstrtab.Sh_offset + shdr.Sh_name;
				f.position(position); // removed past end of file check (fiveofhearts 18/10/08)

				// Number the section
				sb.Append("-----SECTION HEADER #" + SectionCounter + "-----" + "\n");

				string SectionName = Utilities.readStringZ(f); // removed readStringZ exception check (fiveofhearts 18/10/08)
				if (SectionName.Length > 0)
				{
					shdr.Sh_namez = SectionName;
					sb.Append(SectionName + "\n");
					sectionHeaderMap[SectionName] = shdr;
				}
				else
				{
					//Emulator.Console.WriteLine("Section header #" + SectionCounter + " has no name");
				}

				// Add this section header's info
				sb.Append(shdr.ToString());
				SectionCounter++;
			}

			SectInfo_Renamed = sb.ToString();
		}

		/// <returns> The elf was loaded from some kind of file or buffer. The elf
		/// offset is an offset into this buffer where the elf actually starts. If
		/// the returned offset is non-zero this is typically due to the elf being
		/// embedded inside a pbp.  </returns>
		public virtual int ElfOffset
		{
			get
			{
				return elfOffset;
			}
		}

		public virtual Elf32Header Header
		{
			get
			{
				return header;
			}
		}

		public virtual IList<Elf32ProgramHeader> ProgramHeaderList
		{
			get
			{
				return programHeaderList;
			}
		}

		public virtual Elf32ProgramHeader getProgramHeader(int index)
		{
			if (index < 0 || index >= programHeaderList.Count)
			{
				return null;
			}
			return programHeaderList[index];
		}

		public virtual IList<Elf32SectionHeader> SectionHeaderList
		{
			get
			{
				return sectionHeaderList;
			}
		}

		public virtual Elf32SectionHeader getSectionHeader(int index)
		{
			if (index < 0 || index >= sectionHeaderList.Count)
			{
				return null;
			}
			return sectionHeaderList[index];
		}

		public virtual Elf32SectionHeader getSectionHeader(string name)
		{
			return sectionHeaderMap[name];
		}

		public virtual string ElfInfo
		{
			get
			{
				return ElfInfo_Renamed;
			}
		}

		public virtual string ProgInfo
		{
			get
			{
				return ProgInfo_Renamed;
			}
		}

		public virtual string SectInfo
		{
			get
			{
				return SectInfo_Renamed;
			}
		}

		public virtual bool KernelMode
		{
			get
			{
				return kernelMode;
			}
		}
	}

}