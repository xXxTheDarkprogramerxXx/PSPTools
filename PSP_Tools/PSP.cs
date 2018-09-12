using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSP_Tools
{
    public class PSP
    {
        public class PSPElf
        {
            #region << Elf Header >>

            public class ElfFile
            {


                public ElfEHdr ehdr = new ElfEHdr();
                public List<ElfPHdr> phdrs = new List<ElfPHdr>();
                public List<ElfSHdr> shdrs = new List<ElfSHdr>();

                public long file_size = 0;
                public List<String> segments = new List<String>();
                public List<String> sections = new List<String>();
                public String shstrtab = "";
                //public string shstrtab = null;
                public int shstrtab_offset = 0;


                public void Load(FileStream f)
                {
                    object data;

                    ElfPHdr phdr = new ElfPHdr();
                    //ElfSHdr shdr = new ElfSHdr();
                    int start_offset = 0;
                    //start_offset = f.tell();
                    //data = f.
                    file_size = f.Length;
                    f.Seek(start_offset, 0);
                    // this.ehdr = ElfEHdr();
                    ehdr.Set_ElfEHdr(f);
                    //this.phdrs = new List<object>();
                    // this.segments = new List<object>();
                    ehdr.set_has_segments();
                    ehdr.set_has_sections();
                    if (ehdr.has_segments == true)
                    {

                        IEnumerable<int> xrange = Enumerable.Range(0, BitConverter.ToInt16(ehdr.phnum, 0));

                        //foreach (var i in xrange(ehdr.phnum))
                        foreach (int i in xrange)
                        {
                            //print "[*] ElfPHdr Offset: %x" % (start_offset + self.ehdr.phoff + i * self.ehdr.phentsize)
                            int q = start_offset + BitConverter.ToInt32(ehdr.phoff, 0) + i * BitConverter.ToInt16(ehdr.phentsize, 0);
                            f.Seek(start_offset + BitConverter.ToInt32(ehdr.phoff, 0) + i * BitConverter.ToInt16(ehdr.phentsize, 0), 0);
                            // phdr = new ElfPHdr(i);
                            phdr.load(f);
                            //phdrs.append(phdr);
                            phdrs.Add(phdr);
                            byte[] temp = new byte[phdr.filesz[0]];
                            if (phdr.filesz[0] > 0)
                            {
                                f.Seek(start_offset + phdr.offset[0], 0);
                                data = f.Read(temp, 0, phdr.filesz[0]);
                            }
                            else
                            {
                                data = "";
                            }
                            //segments.append(data);
                            segments.Add(data.ToString());
                        }
                    }
                    //shdrs = new List<object>();
                    //sections = new List<object>();
                    if (ehdr.has_sections == true)
                    {
                        //foreach (var i in xrange(ehdr.shnum))
                        int t = BitConverter.ToInt16(ehdr.shnum, 0);
                        IEnumerable<int> xrange2 = Enumerable.Range(0, BitConverter.ToInt16(ehdr.shnum, 0));

                        foreach (int i in xrange2)
                        {
                            ElfSHdr shdr = new ElfSHdr();
                            //print "[*] ElfSHdr Offset: %x" % (start_offset + self.ehdr.shoff + i * self.ehdr.shentsize)
                            int t2 = start_offset + BitConverter.ToInt32(ehdr.shoff, 0) + (i * BitConverter.ToInt16(ehdr.shentsize, 0));
                            int t3 = i * BitConverter.ToInt16(ehdr.shentsize, 0);

                            f.Seek(start_offset + BitConverter.ToInt32(ehdr.shoff, 0) + i * BitConverter.ToInt16(ehdr.shentsize, 0), 0);
                            //shdr = ElfSHdr(i);
                            shdr.load(f);
                            //shdrs.append(shdr);
                            shdrs.Add(shdr);
                        }

                        foreach (int i in xrange2)
                        {
                            if (shdrs[i].type[0] == 3 && shdrs[i].size[0] > 1)
                            {
                                //self.shdrs[i].name_len == 9: # shstrtab
                                shstrtab_offset = BitConverter.ToInt32(shdrs[i].offset, 0);
                                string temps = shstrtab_offset.ToString("X4");
                                Console.WriteLine(String.Format("[*] shstrtab found at: 0x{0}", temps));
                                f.Seek(shstrtab_offset, 0);
                                byte[] temp = new byte[BitConverter.ToInt32(shdrs[i].size, 0)];
                                f.Read(temp, 0, BitConverter.ToInt32(shdrs[i].size, 0));
                                string converted = Encoding.UTF8.GetString(temp, 0, temp.Length);
                                shstrtab = converted;
                                //print self.shstrtab
                            }
                        }
                        //foreach (var i in xrange(ehdr.shnum))
                        foreach (int i in xrange2)
                        {
                            if (i > 0)
                            {
                                shdrs[i - 1].name_end = shdrs[i].name[0] - 1;
                                shdrs[i - 1].name_len = shdrs[i - 1].name_end - shdrs[i - 1].name[0];
                                //print "%d name_end - name - name_len: %x - %x = %x" % (i,self.shdrs[i-1].name_end, self.shdrs[i-1].name, self.shdrs[i-1].name_len)
                            }
                            if (i == BitConverter.ToInt16(ehdr.shnum, 0) - 1)
                            {
                                shdrs[i].name_end = shstrtab.Length - 1;
                                //shdrs[i].name_end = len(shstrtab) - 1;
                                shdrs[i].name_len = shdrs[i].name_end - shdrs[i].name[0];
                                //print "%d name_end - name - name_len: %x - %x = %x" % (i, self.shdrs[i].name_end, self.shdrs[i].name, self.shdrs[i].name_len)
                            }
                        }
                    }
                }
            }

            public class ElfEHdr
            {
                public byte[] magic = new byte[4];
                public byte[] machine_class = new byte[1];
                public byte[] data_encoding = new byte[1];
                public byte[] version = new byte[2];
                public byte[] os_abi = new byte[2];
                public byte[] abi_version = new byte[2];
                public byte[] nident_size = new byte[4];
                public byte[] type = new byte[2];
                public byte[] machine = new byte[2];
                public byte[] eversion = new byte[4];
                public byte[] entry = new byte[4];
                public byte[] phoff = new byte[4];
                public byte[] shoff = new byte[4];
                public byte[] flags = new byte[4];
                public byte[] ehsize = new byte[2];
                public byte[] phentsize = new byte[2];
                public byte[] phnum = new byte[2];
                public byte[] shentsize = new byte[2];
                public byte[] shnum = new byte[2];
                public byte[] shstridx = new byte[2];

                public bool has_segments;
                public bool has_sections;


                public void Set_ElfEHdr(FileStream ehdr)
                {
                    ehdr.Read(magic, 0, 4);
                    ehdr.Read(machine_class, 0, 1);
                    ehdr.Read(data_encoding, 0, 1);
                    ehdr.Read(version, 0, 2);
                    ehdr.Read(os_abi, 0, 2);
                    ehdr.Read(abi_version, 0, 2);
                    ehdr.Read(nident_size, 0, 4);
                    ehdr.Read(type, 0, 2);
                    ehdr.Read(machine, 0, 2);
                    ehdr.Read(eversion, 0, 4);
                    ehdr.Read(entry, 0, 4);
                    ehdr.Read(phoff, 0, 4);
                    ehdr.Read(shoff, 0, 4);
                    ehdr.Read(flags, 0, 4);
                    ehdr.Read(ehsize, 0, 2);
                    ehdr.Read(phentsize, 0, 2);
                    ehdr.Read(phnum, 0, 2);
                    ehdr.Read(shentsize, 0, 2);
                    ehdr.Read(shnum, 0, 2);
                    ehdr.Read(shstridx, 0, 2);
                    Array.Reverse(entry);
                    //Array.Reverse(phoff);
                    // Array.Reverse(shoff);
                    Array.Reverse(flags);
                    Array.Reverse(ehsize);
                    //Array.Reverse(phentsize);
                    //Array.Reverse(phnum);
                    //Array.Reverse(shentsize);
                    //Array.Reverse(shnum);
                    Array.Reverse(shstridx);
                }

                public void set_has_segments()
                {
                    if (BitConverter.ToInt16(phentsize, 0) > 0 && BitConverter.ToInt16(phnum, 0) > 0)
                    {
                        has_segments = true;
                    }
                    else
                    {
                        has_segments = false;
                    }


                }

                public void set_has_sections()
                {
                    if (BitConverter.ToInt16(shentsize, 0) > 0 && BitConverter.ToInt16(shnum, 0) > 0)
                    {
                        has_sections = true;
                    }
                    else
                    {
                        has_sections = false;
                    }


                }
            }

            public class ElfSHdr
            {
                // byte[] idx = idx
                //public int name = new int();
                public int name_end = new int();
                public int name_len = new int();
                /* public int type = new int();
                 public int flags = new int();
                 public int addr = new int();
                 public int offset = new int();
                 public int size = new int();
                 public int link = new int();
                 public int info = new int();
                 public int align = new int();
                 public int entsize = new int();*/

                public byte[] name = new byte[4];
                //public byte[] name_end = new byte[1];
                // public byte[] name_len = new byte[1];
                public byte[] type = new byte[4];
                public byte[] flags = new byte[4];
                public byte[] addr = new byte[4];
                public byte[] offset = new byte[4];
                public byte[] size = new byte[4];
                public byte[] link = new byte[4];
                public byte[] info = new byte[4];
                public byte[] align = new byte[4];
                public byte[] entsize = new byte[4];

                public void load(FileStream ehdr)
                {
                    ehdr.Read(name, 0, 4);
                    //ehdr.Read(name_end, 0, 1);
                    //ehdr.Read(name_len, 0, 1);
                    ehdr.Read(type, 0, 4);
                    ehdr.Read(flags, 0, 4);
                    ehdr.Read(addr, 0, 4);
                    ehdr.Read(offset, 0, 4);
                    ehdr.Read(size, 0, 4);
                    ehdr.Read(link, 0, 4);
                    ehdr.Read(info, 0, 4);
                    ehdr.Read(align, 0, 4);
                    ehdr.Read(entsize, 0, 4);
                }

            }

            public class ElfPHdr
            {
                //byte[] idx = idx
                public byte[] type = new byte[1];
                public byte[] offset = new byte[1];
                public byte[] vaddr = new byte[1];
                public byte[] paddr = new byte[1];
                public byte[] filesz = new byte[1];
                public byte[] memsz = new byte[1];
                public byte[] flags = new byte[1];
                public byte[] align = new byte[1];

                public void load(FileStream ehdr)
                {
                    ehdr.Read(type, 0, 1);
                    ehdr.Read(offset, 0, 1);
                    ehdr.Read(vaddr, 0, 1);
                    ehdr.Read(paddr, 0, 1);
                    ehdr.Read(filesz, 0, 1);
                    ehdr.Read(memsz, 0, 1);
                    ehdr.Read(flags, 0, 1);
                    ehdr.Read(align, 0, 1);
                }

            }

            #endregion << Elf Header >>
        }
    }
}
