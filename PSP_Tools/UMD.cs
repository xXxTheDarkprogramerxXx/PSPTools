using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zlib;

namespace PSP_Tools
{
    /// <summary>
    /// This class represents a PSP UMD / ISO
    /// </summary>
    public class UMD
    {
        public class ISO
        {

            public enum ISOStatus
            {
                Busy = 99,
                Completed = 1,
                Failed = 3,
                None = 0
            }

            public ISO()
            {
                iso_creator = new IsoCreator.IsoCreator();
                iso_creator.Progress += new BER.CDCat.Export.ProgressDelegate(creator_Progress);
                iso_creator.Finish += new BER.CDCat.Export.FinishDelegate(creator_Finished);
                iso_creator.Abort += new BER.CDCat.Export.AbortDelegate(creator_Abort);
            }

            void creator_Abort(object sender, BER.CDCat.Export.AbortEventArgs e)
            {
                //MessageBox.Show(e.Message, "Abort", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Console.WriteLine("The ISO creating process has been stopped.");
            }

            void creator_Finished(object sender, BER.CDCat.Export.FinishEventArgs e)
            {
                Status = ISOStatus.Completed;
                Console.WriteLine(e.Message);
            }

            void creator_Progress(object sender, BER.CDCat.Export.ProgressEventArgs e)
            {
                Status = ISOStatus.Busy;
                if (e.Action != null)
                {

                    Console.WriteLine(e.Action);

                }

                if (e.Maximum != -1)
                {
                    Console.WriteLine(e.Maximum);
                }

                try
                {
                    int value = (e.Current <= e.Maximum) ? e.Current : e.Maximum;
                    Console.WriteLine(string.Format(@"Percentage :{0} / {1} ", value, e.Maximum));
                }
                catch (Exception ex)
                {

                }
            }


            private static Thread iso_thread = null;
            private static IsoCreator.IsoCreator iso_creator = null;

            public ISOStatus Status = ISOStatus.None;


            /// <summary>
            /// Represents ISO Volume Name
            /// </summary>
            public string PSPTitle { get; set; }

            /// <summary>
            /// Creates an ISO from a specific path
            /// </summary>
            /// <param name="FolderPath">Path to be converted to an ISO</param>
            /// <param name="SaveISOPath">Path to where ISO needs to be saved</param>
            public void CreateISO(string FolderPath, string SaveISOPath,bool FakeSign = false)
            {
                try
                {
                    if (iso_thread == null || !iso_thread.IsAlive)
                    {
                        if (PSPTitle == string.Empty)
                        {
                            //load from PSFO
                            PSP_Tools.PARAM_SFO psfo = new PARAM_SFO(FolderPath + @"\PSP_GAME\PARAM.SFO");
                            PSPTitle = psfo.Title;
                            //throw new Exception("Please set PSP Title first");
                        }
                        if (!Directory.Exists(FolderPath))
                        {
                            throw new Exception("Folder path is not valid");
                        }
                        ValidateFolderStructure(FolderPath,FakeSign);
                        iso_thread = new Thread(new ParameterizedThreadStart(iso_creator.Folder2Iso));
                        iso_thread.Start(new IsoCreator.IsoCreator.IsoCreatorFolderArgs(FolderPath, SaveISOPath, PSPTitle));
                        Status = ISOStatus.Busy;
                    }
                    else
                    {
                        iso_thread.Abort();
                    }
                }
                catch (Exception ex)
                {
                    Status = ISOStatus.Failed;
                    throw new Exception("Error while creating ISO", ex);
                }
            }

            /*
             * UCES-00044|7BD493FA3A73B67A|0001|G.............|
             * 55 43 45 53 2D 30 30 30 34 34 | 37 42 44 34 39 33 46 41 33 41 37 33 42 36 37 41 | 30 30 30 31 | 47 00 00 00 00 00 00 00 00 00 00 00 00 00 |
             */
             /// <summary>
             /// Class For UMD_DATA
             /// </summary>
            public class UMD_DATA
            {
                
                public byte[] DISCID = new byte[10]; // +00 : 'U','C','E','S','-','0','0','0','4','4'
                private byte Splitter = 0x7C; // Splitter |
                public byte[] signkey = new byte[10]; // sign key hashed disc id ?
                private byte Splitter1 = 0x7C; // Splitter |
                public byte[] version = new byte[4]; //Version info ?
                private byte Splitter2 = 0x7C; // Splitter |
                public byte[] bottominfo = new byte[14]; // region info gyme type ?
                private byte FinalSplitter = 0x7C; // Splitter |

                /// <summary>
                /// Write UMD Data To File Location
                /// </summary>
                /// <param name="file"></param>
                public void WriteAllToFile(string file)
                {
                    byte[] filebytes = new byte[48];
                    int newlenght = 0;
                    var array = filebytes.ToArray();

                    Array.Copy(DISCID, 0, filebytes, 0, DISCID.Length);
                    newlenght = DISCID.Length;

                    filebytes[newlenght] = Splitter;
                    newlenght++;

                    Array.Copy(signkey, 0, filebytes, newlenght, signkey.Length);
                    newlenght += signkey.Length;

                    filebytes[newlenght] = Splitter;
                    newlenght++;

                    Array.Copy(version, 0, filebytes, newlenght, version.Length);
                    newlenght += version.Length;

                    filebytes[newlenght] = Splitter;
                    newlenght++;

                    //bottominfo
                    Array.Copy(bottominfo, 0, filebytes, newlenght, bottominfo.Length);
                    newlenght += bottominfo.Length;

                    filebytes[newlenght] = Splitter;
                    newlenght++;

                    File.WriteAllBytes(file, filebytes);
                }
            }
            

            public void ValidateFolderStructure(string FolderPath,bool FakeSign)
            {

                PSP_Tools.PARAM_SFO psfo = new PARAM_SFO();

                if(!Directory.Exists(FolderPath + @"\PSP_GAME"))
                {
                    Console.WriteLine("PSP_GAME not found at folder path Please change the folder directory ");
                    throw new Exception("PSP_GAME not found at folder path Please change the folder directory ");
                }
                if(!File.Exists(FolderPath + @"\PSP_GAME\PARAM.SFO"))
                {
                    Console.WriteLine("PARAM.SFO not found at folder path !");
                    throw new Exception("PARAM.SFO not found at folder path !");
                }

                psfo = new PARAM_SFO(FolderPath + @"\PSP_GAME\PARAM.SFO");

                if (!File.Exists(FolderPath + @"\UMD_DATA.BIN"))
                {
                    Console.WriteLine("UMD_DATA.BIN not found pre creating for fake sign");
                    if (FakeSign == true)
                    {
                        UMD_DATA umddat = new UMD_DATA();
                        string discid = psfo.DISC_ID;
                        var builder = new StringBuilder();
                        int count = 0;
                        bool Appedned = false;
                        foreach (var c in discid)
                        {
                            builder.Append(c);
                            if ((++count % 4) == 0 && Appedned == false)
                            {
                                builder.Append('-');
                                Appedned = true;
                            }
                        }
                        discid = builder.ToString();
                        umddat.DISCID = Encoding.UTF8.GetBytes(discid);
                        umddat.signkey = new byte[] { 0x37, 0x42, 0x44, 0x34, 0x39, 0x33, 0x46, 0x41, 0x33, 0x41, 0x37, 0x33, 0x42, 0x36, 0x37, 0x41 };//temp sign
                        umddat.version = new byte[] { 0x30, 0x30, 0x30, 0x31 };//0 0 0 1
                        umddat.bottominfo = new byte[] { 0x47, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };//ive looked at a few games and they all work and are G............. so someone can change this if need be 


                        umddat.WriteAllToFile(FolderPath + @"\UMD_DATA.BIN");
                    }
                }

            }
        }

        public class Sign
        {

            //#include <curses.h>
            /*
            [0x8000 + 140`143] LpXe[u
            156-189:[gfBNgR[h
            0  :LENGTH
            1  :zoku-length
            2-9:extent pos
            33- file name
            */


            private static int fix_filename(byte[] buf, int size)
            {
                int i;
                int @fixed = 0;
                string ptr;
                byte[] fname_buf = new byte[256];

                for (i = 0; i < size - 36;)
                {
                    int len;
                    int flen;

                    len = buf[i];
                    if (len == 0)
                    {
                        break;
                    }

                    flen = BitConverter.ToInt32(buf, i + 32);

                    //TODO : Create Memset function

                    //memset can be used but not for this use array.copy
                    // Array.Copy(fname_buf,)

                    //MD_Util.ForMemset(fname_buf, 0, 256);

                    Array.Clear(fname_buf, 0, 256);

                    var temp = buf[71];

                    //UMD_Util.ForMemset(fname_buf, buf[i + 33], flen);

                    //Array.Copy(buf, i + 33, fname_buf, i + 33, flen);

                    Array.Copy( buf, fname_buf, i + 33);

                    Console.Write("file name '{0}'\n", fname_buf);
                    var tmp = Encoding.ASCII.GetString(fname_buf);
                    Console.Write("file name '{0}'\n", tmp);
                    ptr = StringFunctions.StrChr(Encoding.ASCII.GetString(fname_buf), ';');
                    if (ptr != null)
                    {
                        ptr = "0x00";
                        flen = fname_buf.Length;
                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                        //memcpy(buf[i + 33], fname_buf, flen + 1);
                        //buf[i + 32] = flen;
                        @fixed++;
                        Console.Write("FIX name\n");
                    }
                    i += len;
                }
                return @fixed;
            }

            private static int search_filename(byte[] buf, int size, string name)
            {
                int i;
                int @fixed = 0;
                int file_extnt;

                for (i = 0; i < size - 36;)
                {
                    int len;
                    int flen;

                    len = buf[i];
                    if (len == 0)
                    {
                        break;
                    }

                    file_extnt = buf[i + 2] + (buf[i + 3] << 8) + (buf[i + 4] << 16) + (buf[i + 5] << 24);
                    flen = buf[i + 32];

                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
                    //if (memcmp(name, buf[i + 33], name.Length) == 0)
                    //{
                    //    Console.Write("file '{0}' found {1:x8}\n", name, file_extnt);
                    //    return file_extnt;
                    //}
                    i += len;
                }
                return 0;
            }

            private static byte[] pvd_buf = new byte[0x800];
            private static byte[] root_buf = new byte[0x10000];
            private static byte[] game_buf = new byte[0x10000];
            private static byte[] sig_buf = new byte[33];

            //----------------------------------------------------------------------------------------
            //	Copyright © 2006 - 2018 Tangible Software Solutions, Inc.
            //	This class can be used by anyone provided that the copyright notice remains intact.
            //
            //	This class provides the ability to replicate various classic C string functions
            //	which don't have exact equivalents in the .NET Framework.
            //----------------------------------------------------------------------------------------
            internal static class StringFunctions
            {
                //------------------------------------------------------------------------------------
                //	This method allows replacing a single character in a string, to help convert
                //	C++ code where a single character in a character array is replaced.
                //------------------------------------------------------------------------------------
                public static string ChangeCharacter(string sourceString, int charIndex, char newChar)
                {
                    return (charIndex > 0 ? sourceString.Substring(0, charIndex) : "")
                        + newChar.ToString() + (charIndex < sourceString.Length - 1 ? sourceString.Substring(charIndex + 1) : "");
                }

                //------------------------------------------------------------------------------------
                //	This method replicates the classic C string function 'isxdigit' (and 'iswxdigit').
                //------------------------------------------------------------------------------------
                public static bool IsXDigit(char character)
                {
                    if (char.IsDigit(character))
                        return true;
                    else if ("ABCDEFabcdef".IndexOf(character) > -1)
                        return true;
                    else
                        return false;
                }

                //------------------------------------------------------------------------------------
                //	This method replicates the classic C string function 'strchr' (and 'wcschr').
                //------------------------------------------------------------------------------------
                public static string StrChr(string stringToSearch, char charToFind)
                {
                    int index = stringToSearch.IndexOf(charToFind);
                    if (index > -1)
                        return stringToSearch.Substring(index);
                    else
                        return null;
                }

                //------------------------------------------------------------------------------------
                //	This method replicates the classic C string function 'strrchr' (and 'wcsrchr').
                //------------------------------------------------------------------------------------
                public static string StrRChr(string stringToSearch, char charToFind)
                {
                    int index = stringToSearch.LastIndexOf(charToFind);
                    if (index > -1)
                        return stringToSearch.Substring(index);
                    else
                        return null;
                }

                //------------------------------------------------------------------------------------
                //	This method replicates the classic C string function 'strstr' (and 'wcsstr').
                //------------------------------------------------------------------------------------
                public static string StrStr(string stringToSearch, string stringToFind)
                {
                    int index = stringToSearch.IndexOf(stringToFind);
                    if (index > -1)
                        return stringToSearch.Substring(index);
                    else
                        return null;
                }

                //------------------------------------------------------------------------------------
                //	This method replicates the classic C string function 'strtok' (and 'wcstok').
                //	Note that the .NET string 'Split' method cannot be used to replicate 'strtok' since
                //	it doesn't allow changing the delimiters between each token retrieval.
                //------------------------------------------------------------------------------------
                private static string activeString;
                private static int activePosition;
                public static string StrTok(string stringToTokenize, string delimiters)
                {
                    if (stringToTokenize != null)
                    {
                        activeString = stringToTokenize;
                        activePosition = -1;
                    }

                    //the stringToTokenize was never set:
                    if (activeString == null)
                        return null;

                    //all tokens have already been extracted:
                    if (activePosition == activeString.Length)
                        return null;

                    //bypass delimiters:
                    activePosition++;
                    while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) > -1)
                    {
                        activePosition++;
                    }

                    //only delimiters were left, so return null:
                    if (activePosition == activeString.Length)
                        return null;

                    //get starting position of string to return:
                    int startingPosition = activePosition;

                    //read until next delimiter:
                    do
                    {
                        activePosition++;
                    } while (activePosition < activeString.Length && delimiters.IndexOf(activeString[activePosition]) == -1);

                    return activeString.Substring(startingPosition, activePosition - startingPosition);
                }
            }

            public static void UMDSIGN(string isofile)
            {
                FileStream fp; //FILE = FileStream in c#
                int i;
                int @fixed;
                int key;
                int root_lba;
                int game_lba;
                int umdbin_lba;

                if (!File.Exists(isofile))
                {
                    throw new Exception("ISO File Must be a path");
                }

                //fp = fopen(args[1], "r+b");
                fp = File.OpenRead(isofile);
                if (fp == null)
                {
                    Console.Write("Can't open {0}\n", isofile);
                }

                //we use a binarry reader to get all the info from the file
                using (BinaryReader br = new BinaryReader(fp))
                {
                    // get PVD

                    fp.Seek(0x8000, SeekOrigin.Begin);
                    br.Read(pvd_buf, 0, 0x800);

                    //orginal fseek is equal to file.seek c#
                    /*fseek(fp,0x8000,SEEK_SET);
                      fread(pvd_buf,1,0x800,fp);
                    */

                    // root entent
                    i = 156 + 2;
                    root_lba = BitConverter.ToInt32(pvd_buf,i + 0) + (BitConverter.ToInt32(pvd_buf,i + 1) << 8) + (BitConverter.ToInt32(pvd_buf,i + 2) << 16) + (BitConverter.ToInt32(pvd_buf,i + 3) << 24);
                    Console.Write("ROOT DIRECTRY LBA {0:X8}\n", root_lba);

                    // read root directry
                    fp.Seek(root_lba * 0x800, SeekOrigin.Begin);
                    fp.Read(root_buf, 0, 0x1000);

                    // seatch & fix UMD_DATA.BIN
                    @fixed = fix_filename(root_buf, 0x1000);
                    umdbin_lba = search_filename(root_buf, 0x1000, "UMD_DATA.BIN");
                    game_lba = search_filename(root_buf, 0x1000, "PSP_GAME");

                    if (game_lba == 0)
                    {
                        Console.Write("PSP_GAME not found\n");
                    }
                    else
                    {
                        // read
                        fp.Seek(game_lba * 0x800, SeekOrigin.Begin);
                        fp.Read(game_buf, 1, 0x1000);
                        @fixed += fix_filename(game_buf, 0x1000);
                    }

                    if (umdbin_lba == 0)
                    {
                        Console.Write("UMD_DATA.BIN not found\n");
                    }
                    else
                    {
                        // get signature data
                        fp.Seek(umdbin_lba * 0x800, SeekOrigin.Begin);
                        fp.Read(sig_buf, 1, 32);
                        sig_buf[33] = 0;
                        Console.Write("signature data = '{0}'\n", sig_buf);
                    }

                    if (game_lba != 0 && umdbin_lba != 0)
                    {

                    }
                    //{
                    //    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
                    //    if (pvd_buf[0x370] == 0x00 && pvd_buf[0x371] == 0x01 && pvd_buf[0x372] == 0x00 && (memcmp(pvd_buf[0x373], sig_buf, 32) == 0))
                    //    {
                    //        Console.Write("Already Signed\n");
                    //    }
                    //    else
                    //    {
                    //        Console.Write("Need Sign\n");
                    //        pvd_buf[0x370] = 0x00;
                    //        pvd_buf[0x371] = 0x01; // media mark ?
                    //        pvd_buf[0x372] = 0x00;
                    //        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //        memcpy(pvd_buf[0x373], sig_buf, 32);
                    //        @fixed++;
                    //    }

                    //    if (@fixed == 0)
                    //    {
                    //        Console.Write("Already Okay\n");
                    //    }
                    //    else
                    //    {
                    //        //printf("FIX & SIGNATURE UMD ISO, are you sure[y/n]?");
                    //        //key = getch()|0x20;
                    //        //printf("%c\n",key);
                    //        //if(key=='y')
                    //        {
                    //            Console.Write("write root directry\n");
                    //            fseek(fp, root_lba * 0x800, SEEK_SET);
                    //            fwrite(root_buf, 1, 0x1000, fp);

                    //            Console.Write("write PSP_GAME directry\n");
                    //            fseek(fp, game_lba * 0x800, SEEK_SET);
                    //            fwrite(game_buf, 1, 0x1000, fp);

                    //            Console.Write("write PVD\n");
                    //            fseek(fp, 0x8000, SEEK_SET);
                    //            fwrite(pvd_buf, 1, 0x800, fp);

                    //            Console.Write("Finish!");
                    //        }
                    //    }
                    //}
                    //fclose(fp);
                }
            }
        }

        public class CISO
        {
            /*
               complessed ISO(9660) header format
            */

            public class ciso_header
            {
                public byte[] magic = new byte[4]; // +00 : 'C','I','S','O'
                public uint header_size; // +04 : header size (==0x18)
                public ulong total_bytes; // +08 : number of original data size
                public uint block_size; // +10 : number of compressed block size
                public byte ver; // +14 : version 01
                public byte align; // +15 : align of index value
                public byte[] rsv_06 = new byte[2]; // +16 : reserved
#if false
        //// INDEX BLOCK
        //	unsigned int index[0];			// +18 : block[0] index                  
        //	unsigned int index[1];			// +1C : block[1] index                  
        //             :
        //             :
        //	unsigned int index[last];		// +?? : block[last]                     
        //	unsigned int index[last+1];		// +?? : end of last data point          
        //// DATA BLOCK
        //	unsigned char data[];			// +?? : compressed or plain sector data 
#endif
                public void Read(BinaryReader input)
                {
                    //read the ciso header
                    input.BaseStream.Seek(0, SeekOrigin.Begin);
                    input.Read(magic, 0, 4);//4 bytes
                    //input.Read(rsv_06, 16, 2); //+16 size 2
                    header_size = input.ReadUInt32();
                    total_bytes = input.ReadUInt64();//long
                    block_size = input.ReadUInt32();
                    ver = input.ReadByte();
                    align = input.ReadByte();
                    rsv_06 = input.ReadBytes(2);
                }
            }

            public static class GlobalMembers
            {
                /*
                note:

                file_pos_sector[n]  = (index[n]&0x7fffffff) << CISO_H.align
                file_size_sector[n] = ( (index[n+1]&0x7fffffff) << CISO_H.align) - file_pos_sector[n]

                if(index[n]&0x80000000)
                  // read 0x800 without compress
                else
                  // read file_size_sector[n] bytes and decompress data
                */

                public static string fname_in;
                public static string fname_out;
                public static FileStream fin;
                public static FileStream fout;
                public static ZStream z = new ZStream();

                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                //ORIGINAL LINE: uint *index_buf = null;
                public static byte[] index_buf  ;
                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                //ORIGINAL LINE: uint *crc_buf = null;
                public static byte[] crc_buf = null;
                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                //ORIGINAL LINE: byte *block_buf1 = null;
                public static byte[] block_buf1 = null;
                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                //ORIGINAL LINE: byte *block_buf2 = null;
                public static byte[] block_buf2 = null;

                /****************************************************************************
                    compress ISO to CSO
                ****************************************************************************/

                public static ciso_header ciso = new ciso_header();
                public static int ciso_total_block;

                public static ulong check_file_size(FileStream fp)
                {
                    ulong pos;

                    if (fp.Seek(0, SeekOrigin.End) < 0)
                    {
                        throw new Exception("File size issue");
                    }
                    pos = (ulong)fp.Position;



                    //read csio header
                    ciso.Read(new BinaryReader(fp));

                    ciso.magic[0] = (byte)'C';
                    ciso.magic[1] = (byte)'I';
                    ciso.magic[2] = (byte)'S';
                    ciso.magic[3] = (byte)'O';
                    ciso.ver = 0x01;

                    ciso.block_size = 0x800; // ISO9660 one of sector
                    ciso.total_bytes = pos;
#if false
        	// /* align >0 has bug */
        	//	for(ciso.align = 0 ; (ciso.total_bytes >> ciso.align) >0x80000000LL ; ciso.align++);
#endif

                    ciso_total_block = Convert.ToInt32((pos / ciso.block_size));

                    fp.Seek( 0, SeekOrigin.Begin);//reset file stream to beginning of file

                    return pos;
                }

                /****************************************************************************
                        decompress CSO to ISO
                ****************************************************************************/
                public static int decomp_ciso()
                {
                    ulong file_size;
                    uint index;
                    uint index2;
                    ulong read_pos;
                    ulong read_size;
                    int total_sectors;
                    int index_size;
                    int block;
                    byte[] buf4 = new byte[4];
                    int cmp_size;
                    int status;
                    int percent_period;
                    int percent_cnt;
                    int plain;


                    // read header
                    try
                    {
                        ciso.Read(new BinaryReader(fin));
                    }
                    catch (Exception ex)
                    {
                        Console.Write("file read error\n");
                        throw new Exception("File Read Error", ex);
                    }

                    // check header
                    if (Encoding.UTF8.GetString(ciso.magic) != "CISO" || ciso.block_size == 0 || ciso.total_bytes == 0)
                    {
                        Console.Write("CISO file format error\n");
                        throw new Exception("CISO File Format Error");
                    }
                    // 
                    ciso_total_block = (int)(ciso.total_bytes / ciso.block_size);

                    // allocate index block
                    index_size = (ciso_total_block + 1) * 4; 

                    //index_buf  = malloc(index_size);
                    index_buf = new byte[index_size];

                    IntPtr memory = Marshal.AllocCoTaskMem(index_size); //malloc
                    unsafe
                    {

                        var somepointer = memory.ToPointer();
                        var someother = memory.ToInt64();
                        var someint = memory.ToInt32();

                    }
                    //index_buf = memory.ToPointer()[0];
                    //block_buf1 = malloc(ciso.block_size);
                    block_buf1 = new byte[ciso.block_size];

                    //block_buf2 = malloc(ciso.block_size*2);
                    block_buf2 = new byte[(ciso.block_size * 2)];

                    //c# doesn't have memset no need to resolve this as we know the address is clean
                    // memset(index_buf, 0, index_size);

                    // read index block
                    if (new BinaryReader(fin).Read(index_buf, 0, index_size) !=index_size)
                    {
                        Console.Write("file read error\n");
                        return 1;
                    }

                    // show info
                    Console.Write("Decompress '{0}' to '{1}'\n", fname_in, fname_out);
                    Console.Write("Total File Size {0:D} bytes\n", ciso.total_bytes);
                    Console.Write("block size      {0:D}  bytes\n", ciso.block_size);
                    Console.Write("total blocks    {0:D}  blocks\n", ciso_total_block);
                    Console.Write("index align     {0:D}\n", 1 << ciso.align);

                    // init zlib
                    //z.deflateInit(0);//decompress
                    //z.zalloc = Z_NULL;
                    //z.zfree = Z_NULL;
                    //z.opaque = Z_NULL;
                    //z.free();


                    // decompress data
                    percent_period = ciso_total_block / 100;
                    percent_cnt = 0;

                    for (block = 0; block < ciso_total_block; block++)
                    {
                        if (--percent_cnt <= 0)
                        {
                            percent_cnt = percent_period;
                            Console.Write("decompress {0:D}%\r", block / percent_period);
                        }
                        if (z.inflateInit(-15) != zlibConst.Z_OK)
                        {
                            //printf("deflateInit : %s\n", (z.msg) ? z.msg : "???");
                            return 1;
                        }


                        //index needs to be index = 373768 how i dont know yet

                        //lets try writing this to a byte array

                        index = (uint)BitConverter.ToInt32(index_buf, (block * 4));
                        plain = (int)(index & 0x80000000);
                        index &= 0x7fffffff;
                        read_pos = index << (ciso.align);
                        if (plain != 0)
                        {
                            read_size = ciso.block_size;
                        }
                        else
                        {
                            index2 = (uint)((uint)BitConverter.ToInt32(index_buf, (block * 4 + 4)) & 0x7fffffff);
                            read_size = (index2 - index) << (ciso.align);
                        }
                        fin.Seek((long)(read_pos), SeekOrigin.Begin);

                        z.avail_in = fin.Read(block_buf2, 0, (int)(read_size));
                        if (z.avail_in != (int)(read_size))
                        {
                            Console.Write("block={0:D} : read error\n", block);
                            return 1;
                        }

                        if (plain != 0)
                        {
                            //No memcpy in c# 
                            //memcpy(block_buf1,block_buf2,read_size);
                            Array.Copy( block_buf2, block_buf1, (int)(read_size));
                            cmp_size = (int)(read_size);
                        }
                        else
                        {
                            z.next_out = block_buf1;
                            z.avail_out = (int)(ciso.block_size);
                            z.next_in = block_buf2;
                            status = z.inflate(zlibConst.Z_FULL_FLUSH); //inflate(&z, Z_FULL_FLUSH);
                            if (status != zlibConst.Z_STREAM_END)
                            {
                                if (status != zlibConst.Z_OK)
                                {
                                    Console.Write("block {0:D}:inflate : {1}[{2:D}]\n", block, z.msg, status);
                                    return 1;
                                }
                            }
                            cmp_size = Convert.ToInt32(ciso.block_size - z.avail_out);
                            if (cmp_size != ciso.block_size)
                            {
                                Console.Write("block {0:D} : block size error {1:D} != {2:D}\n", block, cmp_size, ciso.block_size);
                                return 1;
                            }
                        }
                        // write decompressed block
                        fout.Write(block_buf1, 0, cmp_size);
                        

                        // term zlib
                        if (z.inflateEnd() != zlibConst.Z_OK)
                        {
                            //Console.Write("inflateEnd : {0}\n", (z.msg) ? z.msg : "error");
                            return 1;
                        }
                    }

                    Console.Write("ciso decompress completed\n");
                    return 0;
                }
                
                public enum CompresionLevel
                {
                    Decompress,
                    Compress_Level1,
                    Compress_Level2,
                    Compress_Level3,
                    Compress_Level4,
                    Compress_Level5,
                    Compress_Level6,
                    Compress_Level7,
                    Compress_Level8,
                    Compress_Level9
                }

                /****************************************************************************
                            Create_CSIO 
                            Give ISO Path 
                            Enum Compression Level
                ****************************************************************************/
                ///// <summary>
                ///// Creates a ciso from iso
                ///// </summary>
                ///// <param name="iso_File">ISO to use</param>
                ///// <param name="cso_file">save location of cso</param>
                ///// <param name="compress">Compression Level</param>
                ///// <returns></returns>
                //static void Create_CSIO(string iso_File,string cso_file,CompresionLevel compress)
                //{
                //    int level;
                //    int result;

                //    //Console.Error.Write("Compressed ISO9660 converter Ver.1.01 by BOOSTER\n");
                    
                //    //    Console.Write("Usage: ciso level infile outfile\n");
                //    //   Console.Write("  level: 1-9 compress ISO to CSO (1=fast/large - 9=small/slow\n");
                //    //    Console.Write("         0   decompress CSO to ISO\n");
                    
                //    level = (int)compress;
                    
                    
                //    fname_in = iso_File;
                //    fname_out = cso_file;

                    

                //    if ((fin = new FileStream(fname_in, FileMode.Open)) == null)
                //    {
                //        Console.Write("Can't open {0}\n", fname_in);
                //        throw new Exception(string.Format("Can't open {0}\n", fname_in));
                //    }
                //    if ((fin = new FileStream(fname_out, FileMode.OpenOrCreate)) == null)
                //    {
                //        Console.Write("Can't create {0}\n", fname_out);
                //        throw new Exception(string.Format("Can't create {0}\n", fname_out));
                //    }

                //    if (level == 0)
                //    {
                //        Decompress_CISO(iso_File, cso_file);
                //    }
                //    else
                //    {
                //        result = comp_ciso(level);
                //    }

                //    // free memory
                //    if (index_buf != 0)
                //    {
                //        index_buf = 0;
                //    }
                //    if (crc_buf != 0)
                //    {
                //        crc_buf = 0;
                //    }
                //    if (block_buf1 != 0)
                //    {
                //        block_buf1 = 0;
                //    }
                //    if (block_buf2 != 0)
                //    {
                //        block_buf2 = 0;
                //    }

                //    // close files
                //    fin.Close();
                //    fout.Close();
                //}

                /// <summary>
                /// Decompresses CSIO To ISO
                /// </summary>
                /// <param name="iso_File">Save Location of ISO</param>
                /// <param name="cso_file">Location of CSO</param>
               public  static void Decompress_CISO(string iso_File,string cso_file)
                {
                    int level;
                    int result;

                    //Console.Error.Write("Compressed ISO9660 converter Ver.1.01 by BOOSTER\n");

                    //    Console.Write("Usage: ciso level infile outfile\n");
                    //   Console.Write("  level: 1-9 compress ISO to CSO (1=fast/large - 9=small/slow\n");
                    //    Console.Write("         0   decompress CSO to ISO\n");

                    level = (int)CompresionLevel.Decompress;


                    fname_in = cso_file;
                    fname_out = iso_File;



                    if ((fin = new FileStream(fname_in, FileMode.Open)) == null)
                    {
                        Console.Write("Can't open {0}\n", fname_in);
                        throw new Exception(string.Format("Can't open {0}\n", fname_in));
                    }
                    if ((fout = new FileStream(fname_out, FileMode.OpenOrCreate)) == null)
                    {
                        Console.Write("Can't create {0}\n", fname_out);
                        throw new Exception(string.Format("Can't create {0}\n", fname_out));
                    }

                    if (level == 0)
                    {
                        result = decomp_ciso();
                    }
                    fin.Close();
                    fout.Close();
                }
            }

        }
    }

    public static class UMD_Util
    {
        static UMD_Util()
        {
            var dynamicMethod = new DynamicMethod("Memset", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                null, new[] { typeof(IntPtr), typeof(byte), typeof(int) }, typeof(Util), true);

            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Initblk);
            generator.Emit(OpCodes.Ret);

            MemsetDelegate = (Action<IntPtr, byte, int>)dynamicMethod.CreateDelegate(typeof(Action<IntPtr, byte, int>));
        }

        public static void Memset(byte[] array, byte what, int length)
        {
            var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            MemsetDelegate(gcHandle.AddrOfPinnedObject(), what, length);
            gcHandle.Free();
        }

        public static void ForMemset(byte[] array, byte what, int length)
        {
            for (var i = 0; i < length; i++)
            {
                array[i] = what;
            }
        }

        private static Action<IntPtr, byte, int> MemsetDelegate;

    }
}
