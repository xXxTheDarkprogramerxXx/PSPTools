using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PSP_Tools.PSP.PSPElf;

namespace PSP_Tools
{
     public class PS4
    {
        public static void LoadElf(string elf)
        {
            FileStream SourceStream = File.Open(elf, FileMode.OpenOrCreate);

            ElfFile elf_file = new ElfFile();
            elf_file.Load(SourceStream);


            Console.WriteLine("[*] magic: {0}", BitConverter.ToString(elf_file.ehdr.magic).Replace("-", ""));
            Console.WriteLine("[*] machine_class: {0}", BitConverter.ToString(elf_file.ehdr.machine_class).Replace("-", ""));
            Console.WriteLine("[*] data_encoding: {0}", BitConverter.ToString(elf_file.ehdr.data_encoding).Replace("-", ""));
            Console.WriteLine("[*] version: {0}", BitConverter.ToString(elf_file.ehdr.version).Replace("-", ""));
            Console.WriteLine("[*] os_abi: {0}", BitConverter.ToString(elf_file.ehdr.os_abi).Replace("-", ""));
            Console.WriteLine("[*] abi_version: {0}", BitConverter.ToString(elf_file.ehdr.abi_version).Replace("-", ""));
            Console.WriteLine("[*] nident_size: {0}", BitConverter.ToString(elf_file.ehdr.nident_size).Replace("-", ""));
            Console.WriteLine("[*] type: 0x{0}", BitConverter.ToString(elf_file.ehdr.type).Replace("-", ""));
            Console.WriteLine("[*] machine: {0}", BitConverter.ToString(elf_file.ehdr.machine).Replace("-", ""));
            Console.WriteLine("[*] version: {0}", BitConverter.ToString(elf_file.ehdr.version).Replace("-", ""));
            Console.WriteLine("[*] entry: 0x{0}", BitConverter.ToString(elf_file.ehdr.entry).Replace("-", ""));
            Console.WriteLine("[*] phoff: 0x{0}", BitConverter.ToString(elf_file.ehdr.phoff).Replace("-", ""));
            Console.WriteLine("[*] shoff: 0x{0}", BitConverter.ToString(elf_file.ehdr.shoff).Replace("-", ""));
            Console.WriteLine("[*] flags: 0x{0}", BitConverter.ToString(elf_file.ehdr.flags).Replace("-", ""));
            Console.WriteLine("[*] ehsize: 0x{0}", BitConverter.ToString(elf_file.ehdr.ehsize).Replace("-", ""));
            Console.WriteLine("[*] phentsize: 0x{0}", BitConverter.ToString(elf_file.ehdr.phentsize).Replace("-", ""));
            Console.WriteLine("[*] phnum: 0x{0}", BitConverter.ToString(elf_file.ehdr.phnum).Replace("-", ""));
            Console.WriteLine("[*] shentsize: 0x{0}", BitConverter.ToString(elf_file.ehdr.shentsize).Replace("-", ""));
            Console.WriteLine("[*] shnum: 0x{0}", BitConverter.ToString(elf_file.ehdr.shnum).Replace("-", ""));
            Console.WriteLine("[*] shstridx: {0}", BitConverter.ToString(elf_file.ehdr.shstridx).Replace("-", ""));

            string nstring = elf_file.shstrtab.Replace("\0", "/");
            elf_file.shstrtab = elf_file.shstrtab.Replace("\0", "");
            int s = elf_file.shstrtab.Length;
            string[] names = nstring.Split('/');
            names[1].Replace("/", "");
            if (s <= 0)
            {
                if (names[1] == "")
                {
                    Console.WriteLine("[!] No Section header names found!");
                    Console.WriteLine("[.] attempting to identify required sections...");
                    FixElf(elf_file, SourceStream, elf);
                }
            }


            Console.WriteLine("Section Headers:");
            Console.WriteLine("  [Nr] Name                Type            Addr       Off    Size  Flg Lk Inf Al");
            //for i in xrange(elf_file.ehdr.shnum):
            IEnumerable<int> xrange = Enumerable.Range(0, BitConverter.ToInt16(elf_file.ehdr.shnum, 0));
            foreach (int i in xrange)
            {
                Console.WriteLine("   {0} {1} ({2})  {3}       {4}   {5} {6} {7}", i, names[i], elf_file.shdrs[i].name_len.ToString(), BitConverter.ToString(elf_file.shdrs[i].type).Replace("-", ""), BitConverter.ToString(elf_file.shdrs[i].addr).Replace("-", ""), BitConverter.ToString(elf_file.shdrs[i].offset).Replace("-", ""), BitConverter.ToString(elf_file.shdrs[i].size).Replace("-", ""), BitConverter.ToString(elf_file.shdrs[i].flags).Replace("-", ""));

            }

            Console.WriteLine("[.] done.");
            Console.WriteLine("Press any key to continue . . .");
            Console.ReadKey();
        }

        static void FixElf(ElfFile elf_file1, FileStream SourceStream, string elf)
        {
            Console.WriteLine();
            //ElfFile elf_file = elf_file1;
            byte PF_WRITE = 0x1;
            byte PF_READ = 0x2;
            byte PF_EXEC = 0x4;
            int PF_READ_EXEC = PF_READ | PF_EXEC;
            int PF_READ_WRITE = PF_READ | PF_WRITE;
            int to_fix = 5;
            StringBuilder sb = new StringBuilder(4500);
            //for i in xrange(elf_file.ehdr.shnum):
            IEnumerable<int> xrange = Enumerable.Range(0, BitConverter.ToInt16(elf_file1.ehdr.shnum, 0));
            foreach (int i in xrange)
            {
                FileStream f = SourceStream;
                ElfFile elf_file = elf_file1;


                while (sb.Length <= elf_file.shdrs[i].name[0])
                {

                    sb.Append("\0");
                }
                if ((elf_file.shdrs[i].flags[0] & PF_READ_EXEC) == PF_READ_EXEC && elf_file.shdrs[i].name_len == ".text".Length)
                {
                    Console.WriteLine("[!] found .text at section {0}", i);
                    sb.Insert(elf_file.shdrs[i].name[0], ".text");
                    //elf_file.shstrtab.Insert(elf_file.shdrs[i].name[0], ".text");
                    to_fix -= 1;
                }
                if (elf_file.shdrs[i].type[0] == 8 && elf_file.shdrs[i].name_len == ".bss".Length)
                {
                    Console.WriteLine("[!] found .bss at section {0}", i);
                    sb.Insert(elf_file.shdrs[i].name[0], ".bss");
                    //elf_file.shstrtab.Insert(elf_file.shdrs[i].name[0], ".bss");
                    //elf_file.shstrtab += elf_file.shdrs[i].name + ".bss" + elf_file.shstrtab[elf_file.shdrs[i].name_end];
                    to_fix -= 1;
                }
                if (elf_file.shdrs[i].type[0] == 1 && elf_file.shdrs[i].flags[0] == 3 && elf_file.shdrs[i].name_len == ".data".Length)
                {
                    Console.WriteLine("[!] found .data at section {0}", i);
                    sb.Insert(elf_file.shdrs[i].name[0], ".data");
                    //elf_file.shstrtab.Insert(elf_file.shdrs[i].name[0], ".data");
                    //elf_file.shstrtab += elf_file.shdrs[i].name + ".data" + elf_file.shstrtab[elf_file.shdrs[i].name_end];
                    to_fix -= 1;
                }
                if (elf_file.shdrs[i].type[0] == 1 && elf_file.shdrs[i].size[0] == 0x34 && elf_file.shdrs[i].name_len == ".rodata.sceModuleInfo".Length)
                {
                    Console.WriteLine("[!] found .rodata.sceModuleInfo at section {0}", i);
                    sb.Insert(elf_file.shdrs[i].name[0], ".rodata.sceModuleInfo");
                    //elf_file.shstrtab.Insert(elf_file.shdrs[i].name[0], ".rodata.sceModuleInfo");
                    //elf_file.shstrtab += elf_file.shdrs[i].name + ".rodata.sceModuleInfo" + elf_file.shstrtab[elf_file.shdrs[i].name_end];
                    to_fix -= 1;
                }
                if (elf_file.shdrs[i].type[0] == 3 && elf_file.shdrs[i].size[0] > 1 && elf_file.shdrs[i].name_len == ".shstrtab".Length)
                {
                    Console.WriteLine("[!] found .shstrtab at section {0}", i);
                    sb.Insert(elf_file.shdrs[i].name[0], ".shstrtab");
                    //elf_file.shstrtab.Insert(elf_file.shdrs[i].name[0], ".shstrtab");
                    //elf_file.shstrtab += elf_file.shdrs[i].name + ".shstrtab" + elf_file.shstrtab[elf_file.shdrs[i].name_end];
                    to_fix -= 1;
                }
                if (to_fix == 0)
                {
                    Console.WriteLine("[.] Writing new file...");
                    f.Seek(0, 0);
                    elf_file.shstrtab = sb.ToString();
                    FileStream out_file = new FileStream(elf, FileMode.Create);
                    f.CopyTo(out_file);
                    byte[] data = Encoding.ASCII.GetBytes(elf_file.shstrtab);


                    out_file.Write(data, 0, data.Length);//byte[] data = f.Read(data, 0, f.Length);

                    out_file.Close();
                    // data = data[:elf_file.shstrtab_offset] + elf_file.shstrtab + data[elf_file.shstrtab_offset + len(elf_file.shstrtab):];
                    //open(sys.argv[1] + ".new", "wb").write(data);
                }
                else
                    Console.WriteLine("[!] Error. Could not identify required sections.");
            }

        }
    }
}
