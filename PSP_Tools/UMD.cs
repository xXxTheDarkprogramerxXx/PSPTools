using DiscUtils.Iso9660;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
            public void CreateISO(string FolderPath, string SaveISOPath, bool FakeSign = false)
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
                        ValidateFolderStructure(FolderPath, FakeSign);
                        //iso_thread = new Thread(new ParameterizedThreadStart(iso_creator.Folder2Iso));
                        //iso_thread.Start(new IsoCreator.IsoCreator.IsoCreatorFolderArgs(FolderPath, SaveISOPath, PSPTitle));
                        //Status = ISOStatus.Busy;

                        CDBuilder builder = new CDBuilder();
                        builder.UseJoliet = true;
                        builder.VolumeIdentifier = PSPTitle;
                        
                        builder.UseJoliet = true;
                        
                        //builder.UpdateIsolinuxBootTable = true;
                        //builder.AddFile(@"Folder\Hello.txt", Encoding.ASCII.GetBytes("Hello World!"));

                        //string s = FolderPath.Substring(FolderPath.IndexOf("PSP_GAME"));

                        //builder.AddDirectory(FolderPath);
                        string[] filePaths = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories);
                        //foreach (string file in Directory.EnumerateFiles(FolderPath, "*.*", SearchOption.AllDirectories))
                        //{
                        //    builder.AddFile(file, File.ReadAllBytes(file));
                        //}
                      
                            for (int i = 0; i < filePaths.Length; i++)
                            {
                            try
                            {
                                if (!filePaths[i].Contains("PSP_GAME"))
                                {
                                    builder.AddFile(new FileInfo(filePaths[i]).Name, new FileStream(filePaths[i],FileMode.Open));
                                }
                                else
                                {
                                    builder.AddFile(filePaths[i].Substring(filePaths[i].IndexOf("PSP_GAME")), new FileStream(filePaths[i], FileMode.Open));
                                }
                            }
                            catch (Exception bex)
                            {

                            }
                        }
                        
                        //DirSearch(FolderPath);
                        builder.Build(SaveISOPath);
                        Status = ISOStatus.Completed;
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

            static void DirSearch(string sDir)
            {
                try
                {
                    foreach (string d in Directory.GetDirectories(sDir))
                    {
                        foreach (string f in Directory.GetFiles(d))
                        {
                            Console.WriteLine(f);
                        }
                        DirSearch(d);
                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
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


            public void ValidateFolderStructure(string FolderPath, bool FakeSign)
            {

                PSP_Tools.PARAM_SFO psfo = new PARAM_SFO();

                if (!Directory.Exists(FolderPath + @"\PSP_GAME"))
                {
                    Console.WriteLine("PSP_GAME not found at folder path Please change the folder directory ");
                    throw new Exception("PSP_GAME not found at folder path Please change the folder directory ");
                }
                if (!File.Exists(FolderPath + @"\PSP_GAME\PARAM.SFO"))
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


            public string ReadISO(string Path)
            {
                using (FileStream isoStream = File.Open(Path,FileMode.Open,FileAccess.Read))
                {
                    
                    CDReader cd = new CDReader(isoStream, true);
                    //var item = cd.ActiveVariant;
                    return cd.VolumeLabel;
                    //Stream fileStream = cd.OpenFile(@"Folder\Hello.txt", FileMode.Open);
                    // Use fileStream...
                }
                //return "";
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

                    Array.Copy(buf, fname_buf, i + 33);

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
                    root_lba = BitConverter.ToInt32(pvd_buf, i + 0) + (BitConverter.ToInt32(pvd_buf, i + 1) << 8) + (BitConverter.ToInt32(pvd_buf, i + 2) << 16) + (BitConverter.ToInt32(pvd_buf, i + 3) << 24);
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

            #region << Uncomment for Kirk Engine and PSN Fake Signing WIP >>

            //            public unsafe class PSN
            //            {
            //                /// <summary>
            //                /// Signs a PSP ISO and Creates the PBP file
            //                /// </summary>
            //                /// <param name="PSPISO">PSP ISO File Location</param>
            //                /// <param name="SignedPBP">PSP Signed PBP Save Location</param>
            //                public static void Sign_NP_PBP(string PSPISO, string SignedPBP, bool Compress = false, string startdat_name = null, string opnssmp_name = null)
            //                {
            //                    //Get PSP ISO
            //                    FileStream iso_name = File.Open(PSPISO, FileMode.Open);
            //                    FileStream pbp_name = File.Open(SignedPBP, FileMode.OpenOrCreate);

            //                    //Load ContentID from ISO 
            //                    CDReader cdr = new CDReader(iso_name, true);
            //                    Stream ParamSfo = cdr.OpenFile(@"PSP_GAME\PARAM.SFO", FileMode.Open);

            //                    Param_SFO.PARAM_SFO psfo = new Param_SFO.PARAM_SFO(ParamSfo);
            //                    string content_id = psfo.ContentID;

            //                    byte[] version_key = new byte[0x10];
            //                    byte[] header_key = new byte[0x10];
            //                    byte[] data_key = new byte[0x10];


            //                    //Get ISO Size
            //                    long iso_size = iso_name.Length;

            //                    // Initialize KIRK.
            //                    Console.Write("Initializing KIRK engine...\n\n");
            //                    GlobalMembers.kirk_init();

            //                    byte[] png_magic = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // %PNG
            //                    byte[] psp_magic = new byte[] { 0x7E, 0x50, 0x53, 0x50 }; // ~PSP

            //                    byte[] pgd_buf = null;
            //                    int pgd_size = 0;

            //                    // Set keys' context.
            //                    MAC_KEY mkey = new MAC_KEY();
            //                    CIPHER_KEY ckey = new CIPHER_KEY();

            //                    int use_version_key = 0;

            //                    // Set flags and block size data.
            //                    int np_flags = (use_version_key) != 0 ? 0x2 : (0x3 | (0x01000000));
            //                    int block_basis = 0x10;
            //                    int block_size = block_basis * 2048;
            //                    long iso_blocks = (iso_size + block_size - 1) / block_size;

            //                    // Generate random header key.
            //                    byte[] inbuf = new byte[1];
            //                    GlobalMembers.sceUtilsBufferCopyWithRange(ref header_key, 0x10, ref inbuf, 0, DefineConstants.KIRK_CMD_PRNG);

            //                    // Generate fixed key, if necessary.
            //                    if (use_version_key == 0)
            //                    {
            //                        GlobalMembers.sceNpDrmGetFixedKey(ref version_key, ref content_id, np_flags);
            //                    }

            //                    // Write PBP data.
            //                    Console.Write("Writing PBP data...\n");
            //                    long table_offset = GlobalMembers.write_pbp(pbp, ref iso_name, ref content_id, np_flags, ref startdat_buf, startdat_size, ref pgd_buf, pgd_size);
            //                    long table_size = iso_blocks * 0x20;
            //                    long np_offset = table_offset - 0x100;
            //                    int np_size = 0x100;
            //                }

            //                internal static class DefineConstants
            //                {
            //                    public const int KIRK_OPERATION_SUCCESS = 0;
            //                    public const int KIRK_NOT_ENABLED = 1;
            //                    public const int KIRK_INVALID_MODE = 2;
            //                    public const int KIRK_HEADER_HASH_INVALID = 3;
            //                    public const int KIRK_DATA_HASH_INVALID = 4;
            //                    public const int KIRK_SIG_CHECK_INVALID = 5;
            //                    public const int KIRK_UNK_1 = 6;
            //                    public const int KIRK_UNK_2 = 7;
            //                    public const int KIRK_UNK_3 = 8;
            //                    public const int KIRK_UNK_4 = 9;
            //                    public const int KIRK_UNK_5 = 0xA;
            //                    public const int KIRK_UNK_6 = 0xB;
            //                    public const int KIRK_NOT_INITIALIZED = 0xC;
            //                    public const int KIRK_INVALID_OPERATION = 0xD;
            //                    public const int KIRK_INVALID_SEED_CODE = 0xE;
            //                    public const int KIRK_INVALID_SIZE = 0xF;
            //                    public const int KIRK_DATA_SIZE_ZERO = 0x10;
            //                    public const int KIRK_CMD_DECRYPT_PRIVATE = 1;
            //                    public const int KIRK_CMD_2 = 2;
            //                    public const int KIRK_CMD_3 = 3;
            //                    public const int KIRK_CMD_ENCRYPT_IV_0 = 4;
            //                    public const int KIRK_CMD_ENCRYPT_IV_FUSE = 5;
            //                    public const int KIRK_CMD_ENCRYPT_IV_USER = 6;
            //                    public const int KIRK_CMD_DECRYPT_IV_0 = 7;
            //                    public const int KIRK_CMD_DECRYPT_IV_FUSE = 8;
            //                    public const int KIRK_CMD_DECRYPT_IV_USER = 9;
            //                    public const int KIRK_CMD_PRIV_SIGN_CHECK = 10;
            //                    public const int KIRK_CMD_SHA1_HASH = 11;
            //                    public const int KIRK_CMD_ECDSA_GEN_KEYS = 12;
            //                    public const int KIRK_CMD_ECDSA_MULTIPLY_POINT = 13;
            //                    public const int KIRK_CMD_PRNG = 14;
            //                    public const int KIRK_CMD_15 = 15;
            //                    public const int KIRK_CMD_ECDSA_SIGN = 16;
            //                    public const int KIRK_CMD_ECDSA_VERIFY = 17;
            //                    public const int KIRK_MODE_CMD1 = 1;
            //                    public const int KIRK_MODE_CMD2 = 2;
            //                    public const int KIRK_MODE_CMD3 = 3;
            //                    public const int KIRK_MODE_ENCRYPT_CBC = 4;
            //                    public const int KIRK_MODE_DECRYPT_CBC = 5;
            //                    public const int SUBCWR_NOT_16_ALGINED = 0x90A;
            //                    public const int SUBCWR_HEADER_HASH_INVALID = 0x920;
            //                    public const int SUBCWR_BUFFER_TOO_SMALL = 0x1000;
            //                    public const int AES_KEY_LEN_128 = 128;
            //                    public const int AES_KEY_LEN_192 = 192;
            //                    public const int AES_KEY_LEN_256 = 256;
            //                    public const int AES_BUFFER_SIZE = 16;
            //                    public const int AES_MAXKEYBITS = 256;
            //                    public const int AES_MAXROUNDS = 14;
            //                    public const int AES_128 = 0;
            //                    public const int _GLOBAL_H_ = 1;
            //                    public const int FALSE = 0;
            //                    public const int _SHA_H_ = 1;
            //                    public const int _ENDIAN_H_ = 1;
            //                    public const int SHS_DATASIZE = 64;
            //                    public const int SHS_DIGESTSIZE = 20;
            //                    public const int K1 = 0x5A827999; // Rounds  0-19
            //                    public const int K2 = 0x6ED9EBA1; // Rounds 20-39
            //                    public const uint K3 = 0x8F1BBCDC; // Rounds 40-59
            //                    public const uint K4 = 0xCA62C1D6; // Rounds 60-79
            //                    public const int h0init = 0x67452301;
            //                    public const uint h1init = 0xEFCDAB89;
            //                    public const uint h2init = 0x98BADCFE;
            //                    public const int h3init = 0x10325476;
            //                    public const uint h4init = 0xC3D2E1F0;
            //                    public const int PT_LOAD = 1; // Loadable segment.
            //                    public const int PF_X = 0x1; // Executable.
            //                    public const int PF_W = 0x2; // Writable.
            //                    public const int PF_R = 0x4; // Readable.
            //                    public const int SECTOR_SIZE = 0x800;
            //                    public const int ISO9660_FILEFLAGS_FILE = 1;
            //                    public const int ISO9660_FILEFLAGS_DIR = 2;
            //                    public const int MAX_RETRIES = 1;
            //                    public const int MAX_DIR_LEVEL = 8;
            //                    public const int CISO_IDX_BUFFER_SIZE = 0x200;
            //                    public const int CISO_DEC_BUFFER_SIZE = 0x2000;
            //                    public const string ISO_STANDARD_ID = "CD001";
            //                    public const int RATIO_LIMIT = 90;
            //                    public const int PSF_MAGIC = 0x46535000;
            //                }

            //                #region << eas >>

            //                const int AesKeyLen128 = 128;
            //                const int AesKeyLen192 = 192;
            //                const int AesKeyLen256 = 256;

            //                const int AesBufferSize = 16;

            //                const int AesMaxkeybits = 256;

            //                const int AesMaxkeybytes = AesMaxkeybits / 8;

            //                // for 256-bit keys, fewer for less
            //                const int AesMaxrounds = 14;
            //                //const int pwuAESContextBuffer RijndaelCtx

            //                /// <summary>
            //                /// The structure for key information
            //                /// </summary>
            //                public struct RijndaelCtx
            //                {
            //                    /// <summary>
            //                    /// context contains only encrypt schedule
            //                    /// </summary>
            //                    public int EncOnly;

            //                    /// <summary>
            //                    /// key-length-dependent number of rounds
            //                    /// </summary>
            //                    public int Nr;

            //                    /// <summary>
            //                    /// encrypt key schedule
            //                    /// </summary>
            //                    public fixed uint Ek[4 * (AesMaxrounds + 1)];

            //                    /// <summary>
            //                    /// decrypt key schedule
            //                    /// </summary>
            //                    public fixed uint Dk[4 * (AesMaxrounds + 1)];
            //                }

            //                public struct AesCtx
            //                {
            //                    /// <summary>
            //                    /// context contains only encrypt schedule
            //                    /// </summary>
            //                    public int EncOnly;

            //                    /// <summary>
            //                    /// key-length-dependent number of rounds
            //                    /// </summary>
            //                    public int Nr;

            //                    /// <summary>
            //                    /// encrypt key schedule
            //                    /// </summary>
            //                    public fixed uint Ek[4 * (AesMaxrounds + 1)];

            //                    /// <summary>
            //                    /// decrypt key schedule
            //                    /// </summary>
            //                    public fixed uint Dk[4 * (AesMaxrounds + 1)];
            //                };

            //                public struct Sha1Context
            //                {
            //                    /// <summary>
            //                    /// Message Digest (output)
            //                    /// </summary>
            //                    public fixed uint MessageDigest[5];

            //                    /// <summary>
            //                    /// Message length in bits
            //                    /// </summary>
            //                    public uint LengthLow;

            //                    /// <summary>
            //                    /// Message length in bits
            //                    /// </summary>
            //                    public uint LengthHigh;

            //                    /// <summary>
            //                    /// 512-bit message blocks
            //                    /// </summary>
            //                    public fixed byte MessageBlock[64];

            //                    /// <summary>
            //                    /// Index into message block array
            //                    /// </summary>
            //                    public int MessageBlockIndex;

            //                    /// <summary>
            //                    /// Is the digest computed?
            //                    /// </summary>
            //                    public bool Computed;

            //                    /// <summary>
            //                    /// Is the message digest corruped?
            //                    /// </summary>
            //                    public bool Corrupted;
            //                }



            //            #endregion << eas >>

            //            #region << amctrl >>

            //            public class MAC_KEY
            //                {
            //                    public int type;
            //                    public byte[] key = new byte[16];
            //                    public byte[] pad = new byte[16];
            //                    public int pad_size;
            //                }

            //                public class CIPHER_KEY
            //                {
            //                    public uint type;
            //                    public uint seed;
            //                    public byte[] key = new byte[16];
            //                }


            //                #endregion << amctrl >>

            //                #region << EC >>
            //                public class point
            //                {
            //                    public byte[] x = new byte[20];
            //                    public byte[] y = new byte[20];
            //                }

            //                #endregion << EC >>

            //                #region << Kirk Engine >>

            //                /*
            //    Draan proudly presents:

            //    With huge help from community:
            //    coyotebean, Davee, hitchhikr, kgsws, liquidzigong, Mathieulh, Proxima, SilverSpring

            //    ******************** KIRK-ENGINE ********************
            //    An Open-Source implementation of KIRK (PSP crypto engine) algorithms and keys.
            //    Includes also additional routines for hash forging.

            //    ********************

            //    This program is free software: you can redistribute it and/or modify
            //    it under the terms of the GNU General Public License as published by
            //    the Free Software Foundation, either version 3 of the License, or
            //    (at your option) any later version.

            //    This program is distributed in the hope that it will be useful,
            //    but WITHOUT ANY WARRANTY; without even the implied warranty of
            //    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
            //    GNU General Public License for more details.

            //    You should have received a copy of the GNU General Public License
            //    along with this program.  If not, see <http://www.gnu.org/licenses/>.
            //*/


            //                /*
            //                    Draan proudly presents:

            //                    With huge help from community:
            //                    coyotebean, Davee, hitchhikr, kgsws, liquidzigong, Mathieulh, Proxima, SilverSpring

            //                    ******************** KIRK-ENGINE ********************
            //                    An Open-Source implementation of KIRK (PSP crypto engine) algorithms and keys.
            //                    Includes also additional routines for hash forging.

            //                    ********************

            //                    This program is free software: you can redistribute it and/or modify
            //                    it under the terms of the GNU General Public License as published by
            //                    the Free Software Foundation, either version 3 of the License, or
            //                    (at your option) any later version.

            //                    This program is distributed in the hope that it will be useful,
            //                    but WITHOUT ANY WARRANTY; without even the implied warranty of
            //                    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
            //                    GNU General Public License for more details.

            //                    You should have received a copy of the GNU General Public License
            //                    along with this program.  If not, see <http://www.gnu.org/licenses/>.
            //                */

            //                // Macros
            //                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                //ORIGINAL LINE: #define round_up(x,n) (-(-(x) & -(n)))
            //                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                //ORIGINAL LINE: #define array_size(x) (sizeof(x) / sizeof(*(x)))

            //                // KIRK return values

            //                // sceUtilsBufferCopyWithRange modes

            //                // KIRK header modes

            //                // sceUtilsBufferCopyWithRange errors

            //                // Structs
            //                public class KIRK_AES128CBC_HEADER
            //                {
            //                    public int mode;
            //                    public int unk_4;
            //                    public int unk_8;
            //                    public int keyseed;
            //                    public int data_size;
            //                }

            //                public class KIRK_CMD1_HEADER
            //                {
            //                    public byte[] AES_key = new byte[16];
            //                    public byte[] CMAC_key = new byte[16];
            //                    public byte[] CMAC_header_hash = new byte[16];
            //                    public byte[] CMAC_data_hash = new byte[16];
            //                    public byte[] unused = new byte[32];
            //                    public uint mode;
            //                    public byte ecdsa_hash;
            //                    public byte[] unk3 = new byte[11];
            //                    public uint data_size;
            //                    public uint data_offset;
            //                    public byte[] unk4 = new byte[8];
            //                    public byte[] unk5 = new byte[16];
            //                }

            //                public class KIRK_CMD1_ECDSA_HEADER
            //                {
            //                    public byte[] AES_key = new byte[16];
            //                    public byte[] header_sig_r = new byte[20];
            //                    public byte[] header_sig_s = new byte[20];
            //                    public byte[] data_sig_r = new byte[20];
            //                    public byte[] data_sig_s = new byte[20];
            //                    public uint mode;
            //                    public byte ecdsa_hash;
            //                    public byte[] unk3 = new byte[11];
            //                    public uint data_size;
            //                    public uint data_offset;
            //                    public byte[] unk4 = new byte[8];
            //                    public byte[] unk5 = new byte[16];
            //                }

            //                public class ECDSA_SIG
            //                {
            //                    public byte[] r = new byte[0x14];
            //                    public byte[] s = new byte[0x14];
            //                }

            //                public class ECDSA_POINT
            //                {
            //                    public byte[] x = new byte[0x14];
            //                    public byte[] y = new byte[0x14];
            //                }

            //                public class KIRK_SHA1_HEADER
            //                {
            //                    public uint data_size;
            //                }

            //                public class KIRK_CMD12_BUFFER
            //                {
            //                    public byte[] private_key = new byte[0x14];
            //                    public ECDSA_POINT public_key = new ECDSA_POINT();
            //                }

            //                public class KIRK_CMD13_BUFFER
            //                {
            //                    public byte[] multiplier = new byte[0x14];
            //                    public ECDSA_POINT public_key = new ECDSA_POINT();
            //                }

            //                public class KIRK_CMD16_BUFFER
            //                {
            //                    public byte[] enc_private = new byte[0x20];
            //                    public byte[] message_hash = new byte[0x14];
            //                }

            //                public class KIRK_CMD17_BUFFER
            //                {
            //                    public ECDSA_POINT public_key = new ECDSA_POINT();
            //                    public byte[] message_hash = new byte[0x14];
            //                    public ECDSA_SIG signature = new ECDSA_SIG();
            //                }


            //                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                //ORIGINAL LINE: #define AES_MAXKEYBYTES (AES_MAXKEYBITS/8)
            //                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                //ORIGINAL LINE: #define pwuAESContextBuffer RijndaelCtx
            //                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                //ORIGINAL LINE: #define TRUE ( !FALSE )

            //                // Internal variables
            //                public class kirk16_data
            //                {
            //                    public byte[] fuseid = new byte[8];
            //                    public byte[] mesh = new byte[0x40];
            //                }

            //                public class header_keys
            //                {
            //                    public byte[] AES = new byte[16];
            //                    public byte[] CMAC = new byte[16];
            //                }

            //                #endregion << Kirk Engine >>

            //                #region << PSP Header >>

            //                // Copyright(C) 2013       tpu
            //                // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                // Licensed under the terms of the GNU GPL, version 3
            //                // http://www.gnu.org/licenses/gpl-3.0.txt

            //                /* Values for p_type. */

            //                /* Values for p_flags. */
            //                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                //ORIGINAL LINE: #define PF_RW (PF_R|PF_W)

            //                public class Elf32_Ehdr
            //                {
            //                    public uint e_magic;
            //                    public byte e_class;
            //                    public byte e_data;
            //                    public byte e_idver;
            //                    public byte[] e_pad = new byte[9];
            //                    public ushort e_type;
            //                    public ushort e_machine;
            //                    public uint e_version;
            //                    public uint e_entry;
            //                    public uint e_phoff;
            //                    public uint e_shoff;
            //                    public uint e_flags;
            //                    public ushort e_ehsize;
            //                    public ushort e_phentsize;
            //                    public ushort e_phnum;
            //                    public ushort e_shentsize;
            //                    public ushort e_shnum;
            //                    public ushort e_shstrndx;
            //                }

            //                public class Elf32_Phdr
            //                {
            //                    public uint p_type;
            //                    public uint p_offset;
            //                    public uint p_vaddr;
            //                    public uint p_paddr;
            //                    public uint p_filesz;
            //                    public uint p_memsz;
            //                    public uint p_flags;
            //                    public uint p_align;
            //                }

            //                public class Elf32_Shdr
            //                {
            //                    public uint sh_name;
            //                    public uint sh_type;
            //                    public uint sh_flags;
            //                    public uint sh_addr;
            //                    public uint sh_offset;
            //                    public uint sh_size;
            //                    public uint sh_link;
            //                    public uint sh_info;
            //                    public uint sh_addralign;
            //                    public uint sh_entsize;
            //                }

            //                public class Elf32_Rel
            //                {
            //                    public uint r_offset;
            //                    public uint r_info; // sym, type: ELF32_R_...
            //                }

            //                public class SceModuleInfo
            //                {
            //                    public ushort modattribute;
            //                    public byte[] modversion = new byte[2]; // minor, major, etc...
            //                    public string modname = new string(new char[28]);
            //                    public object gp_value;
            //                    public object ent_top;
            //                    public object ent_end;
            //                    public object stub_top;
            //                    public object stub_end;
            //                }

            //                public class PSP_Header2
            //                {
            //                    public uint signature; //0
            //                    public ushort mod_attribute; //4
            //                    public ushort comp_attribute; //6 compress method:
            //                                                  //        0x0001=PRX Compress
            //                                                  //        0x0002=ELF Packed
            //                                                  //        0x0008=GZIP overlap
            //                                                  //        0x0200=KL4E(if not set, GZIP)
            //                    public byte module_ver_lo; //8
            //                    public byte module_ver_hi; //9
            //                    public string modname = new string(new char[28]); //0xA
            //                    public byte mod_version; //0x26
            //                    public byte nsegments; //0x27
            //                    public uint elf_size; //0x28
            //                    public uint psp_size; //0x2C
            //                    public uint boot_entry; //0x30
            //                    public uint modinfo_offset; //0x34
            //                    public int bss_size; //0x38
            //                    public ushort[] seg_align = new ushort[4]; //0x3C
            //                    public uint[] seg_address = new uint[4]; //0x44
            //                    public int[] seg_size = new int[4]; //0x54
            //                    public uint[] reserved = new uint[5]; //0x64
            //                    public uint devkit_version; //0x78
            //                    public byte decrypt_mode; //0x7C
            //                    public byte padding; //0x7D
            //                    public ushort overlap_size; //0x7E
            //                    public byte[] key_data = new byte[0x30]; //0x80
            //                    public uint comp_size; //0xB0  kirk data_size
            //                    public int _80; //0xB4  kirk data_offset
            //                    public uint unk_B8; //0xB8
            //                    public uint unk_BC; //0xBC
            //                    public byte[] key_data2 = new byte[0x10]; //0xC0
            //                    public uint tag; //0xD0
            //                    public byte[] scheck = new byte[0x58]; //0xD4
            //                    public byte[] sha1_hash = new byte[0x14]; //0x12C
            //                    public byte[] key_data4 = new byte[0x10]; //0x140
            //                }

            //                #endregion << PSP Header >>

            //                #region << SHA1 >>

            //                /* sha1.c : Implementation of the Secure Hash Algorithm */

            //                /* SHA: NIST's Secure Hash Algorithm */

            //                /*	This version written November 2000 by David Ireland of 
            //                    DI Management Services Pty Limited <code@di-mgt.com.au>

            //                    Adapted from code in the Python Cryptography Toolkit, 
            //                    version 1.0.0 by A.M. Kuchling 1995.
            //                */

            //                /* AM Kuchling's posting:- 
            //                   Based on SHA code originally posted to sci.crypt by Peter Gutmann
            //                   in message <30ajo5$oe8@ccu2.auckland.ac.nz>.
            //                   Modified to test for endianness on creation of SHA objects by AMK.
            //                   Also, the original specification of SHA was found to have a weakness
            //                   by NSA/NIST.  This code implements the fixed version of SHA.
            //                */

            //                /* Here's the first paragraph of Peter Gutmann's posting:

            //                The following is my SHA (FIPS 180) code updated to allow use of the "fixed"
            //                SHA, thanks to Jim Gillogly and an anonymous contributor for the information on
            //                what's changed in the new version.  The fix is a simple change which involves
            //                adding a single rotate in the initial expansion function.  It is unknown
            //                whether this is an optimal solution to the problem which was discovered in the
            //                SHA or whether it's simply a bandaid which fixes the problem with a minimum of
            //                effort (for example the reengineering of a great many Capstone chips).
            //                */

            //                /* h files included here to make this just one file ... */

            //                /* sha.c */

            //                /* POINTER defines a generic pointer type */

            //                /* UINT4 defines a four byte word */

            //                /* BYTE defines a unsigned character */

            //#if !TRUE
            //#define FALSE
            //                //C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
            //                //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                //ORIGINAL LINE: #define TRUE ( !FALSE )
            //#define TRUE
            //#endif


            //                /* sha.h */


            //                /* #include "global.h" */

            //                /* The structure for storing SHS info */

            //                public class SHA_CTX
            //                {
            //                    public uint[] digest = new uint[5]; // Message digest
            //                    public uint countLo; // 64-bit bit count
            //                    public uint countHi;
            //                    public uint[] data = new uint[16]; // SHS data buffer
            //                    public int Endianness;
            //                }

            //                #endregion << SHA 1 >>

            //                #region << ISO Reader >>

            //                public class Iso9660DirectoryRecord
            //                {
            //                    /* Directory record length. */
            //                    public byte len_dr;
            //                    /* Extended attribute record length. */
            //                    public byte XARlength;
            //                    /* First logical block where file starts. */
            //                    public uint lsbStart;
            //                    public uint msbStart;
            //                    /* Number of bytes in file. */
            //                    public uint lsbDataLength;
            //                    public uint msbDataLength;
            //                    /* Since 1900. */
            //                    public byte year;
            //                    public byte month;
            //                    public byte day;
            //                    public byte hour;
            //                    public byte minute;
            //                    public byte second;
            //                    /* 15-minute offset from Universal Time. */
            //                    public byte gmtOffse;
            //                    /* Attributes of a file or directory. */
            //                    public byte fileFlags;
            //                    /* Used for interleaved files. */
            //                    public byte interleaveSize;
            //                    /* Used for interleaved files. */
            //                    public byte interleaveSkip;
            //                    /* Which volume in volume set contains this file. */
            //                    public ushort lsbVolSetSeqNum;
            //                    public ushort msbVolSetSeqNum;
            //                    /* Length of file identifier that follows. */
            //                    public byte len_fi;
            //                    /* File identifier: actual is len_fi. */
            //                    /* Contains extra blank byte if len_fi odd. */
            //                    public char fi;
            //                }

            //                public class _CISOHeader
            //                {
            //                    public byte[] magic = new byte[4]; // +00 : 'C','I','S','O'
            //                    public uint header_size;
            //                    public ulong total_bytes; // +08 : number of original data size
            //                    public uint block_size; // +10 : number of compressed block size
            //                    public byte ver; // +14 : version 01
            //                    public byte align; // +15 : align of index (offset = index[n]<<align)
            //                    public byte[] rsv_06 = new byte[2]; // +16 : reserved
            //                }

            //                #endregion << ISO Reader >>

            //                #region << LZRC Decode >>

            //                // Copyright (C) 2013       tpu
            //                // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                // Licensed under the terms of the GNU GPL, version 3
            //                // http://www.gnu.org/licenses/gpl-3.0.txt

            //                // Copyright (C) 2013       tpu
            //                // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                // Licensed under the terms of the GNU GPL, version 3
            //                // http://www.gnu.org/licenses/gpl-3.0.txt

            //                public class LZRC_DECODE
            //                {
            //                    public byte[] input;
            //                    public int in_ptr;
            //                    public int in_len;

            //                    public byte[] output;
            //                    public int out_ptr;
            //                    public int out_len;

            //                    public uint range;
            //                    public uint code;
            //                    public uint out_code;
            //                    public byte lc;

            //                    public byte[,] bm_literal = new byte[8, 256];
            //                    public byte[,] bm_dist_bits = new byte[8, 39];
            //                    public byte[,] bm_dist = new byte[18, 8];
            //                    public byte[,] bm_match = new byte[8, 8];
            //                    public byte[,] bm_len = new byte[8, 31];
            //                }


            //                #endregion << LZRC Decode >>

            //                #region << PGD >>

            //                public class PGD_HEADER
            //                {
            //                    public byte[] vkey = new byte[16];

            //                    public int open_flag;
            //                    public int key_index;
            //                    public int drm_type;
            //                    public int mac_type;
            //                    public int cipher_type;

            //                    public int data_size;
            //                    public int align_size;
            //                    public int block_size;
            //                    public int block_nr;
            //                    public int data_offset;
            //                    public int table_offset;

            //                    public byte[] buf;
            //                }

            //                #endregion << PGD >>

            //                #region << Sign Up >>

            //                public class SFO_Header
            //                {
            //                    public uint magic;
            //                    public uint version;
            //                    public uint key_offset;
            //                    public uint val_offset;
            //                    public uint key_count;
            //                }

            //                public class SFO_Entry
            //                {
            //                    public ushort name_offset;
            //                    public byte align;
            //                    public byte type;
            //                    public uint val_size;
            //                    public uint align_size;
            //                    public uint data_offset;
            //                }

            //                public class STARTDAT_HEADER
            //                {
            //                    public byte[] magic = new byte[8]; // STARTDAT
            //                    public uint unk1; // 0x01
            //                    public uint unk2; // 0x01
            //                    public uint header_size;
            //                    public uint data_size;
            //                }

            //                public class NPUMDIMG_HEADER_BODY
            //                {
            //                    public ushort sector_size; // 0x0800
            //                    public ushort unk_2; // 0xE000
            //                    public uint unk_4;
            //                    public uint unk_8;
            //                    public uint unk_12;
            //                    public uint unk_16;
            //                    public uint lba_start;
            //                    public uint unk_24;
            //                    public uint nsectors;
            //                    public uint unk_32;
            //                    public uint lba_end;
            //                    public uint unk_40;
            //                    public uint block_entry_offset;
            //                    public string disc_id = new string(new char[0x10]);
            //                    public uint header_start_offset;
            //                    public uint unk_68;
            //                    public byte unk_72;
            //                    public byte bbmac_param;
            //                    public byte unk_74;
            //                    public byte unk_75;
            //                    public uint unk_76;
            //                    public uint unk_80;
            //                    public uint unk_84;
            //                    public uint unk_88;
            //                    public uint unk_92;
            //                }

            //                public class NPUMDIMG_HEADER
            //                {
            //                    public byte[] magic = new byte[0x08]; // NPUMDIMG
            //                    public uint np_flags;
            //                    public uint block_basis;
            //                    public byte[] content_id = new byte[0x30];
            //                    public NPUMDIMG_HEADER_BODY body = new NPUMDIMG_HEADER_BODY();
            //                    public byte[] header_key = new byte[0x10];
            //                    public byte[] data_key = new byte[0x10];
            //                    public byte[] header_hash = new byte[0x10];
            //                    public byte[] padding = new byte[0x8];
            //                    public byte[] ecdsa_sig = new byte[0x28];
            //                }


            //                #endregion << Sign Up >>

            //                #region << eboot >>
            //                public class TAG_KEY
            //                {
            //                    public uint tag;
            //                    public byte[] key = new byte[16];
            //                    public uint code;
            //                    public uint type;

            //                    public TAG_KEY(uint Tag, byte[] Key, uint Code, uint Type)
            //                    {
            //                        this.tag = Tag;
            //                        this.key = Key;
            //                        this.code = Code;
            //                        this.type = Type;
            //                    }
            //                }

            //                #endregion << eboot >>



            //                public static class GlobalMembers
            //                {

            //                    //#define GETuint(pt) (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] <<  8) ^ ((uint)(pt)[3]))
            //                    //#define PUTuint(ct, st) { (ct)[0] = (u8)((st) >> 24); (ct)[1] = (u8)((st) >> 16); (ct)[2] = (u8)((st) >>  8); (ct)[3] = (u8)(st); }
            //                    //private static uint GETuint(byte[] pt) => (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] <<  8) ^ ((uint)(pt)[3]));

            //                    private static uint GeTuint(byte* pt) =>
            //                        (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] << 8) ^ (pt)[3]);

            //                    //private static void PUTuint(byte[] ct, uint st) {
            //                    //	(ct)[0] = (byte)((st) >> 24);
            //                    //	(ct)[1] = (byte)((st) >> 16);
            //                    //	(ct)[2] = (byte)((st) >>  8);
            //                    //	(ct)[3] = (byte)(st);
            //                    //}

            //                    private static void PuTuint(byte* ct, uint st)
            //                    {
            //                        (ct)[0] = (byte)((st) >> 24);
            //                        (ct)[1] = (byte)((st) >> 16);
            //                        (ct)[2] = (byte)((st) >> 8);
            //                        (ct)[3] = (byte)(st);
            //                    }

            //                    /* setup key context for both encryption and decryption */

            //                    public static int rijndael_set_key(RijndaelCtx ctx, byte key, int bits)
            //                    {
            //                        int rounds;

            //                        rounds = rijndaelKeySetupEnc(ctx.ek, key, bits);
            //                        if (rounds == 0)
            //                        {
            //                            return -1;
            //                        }
            //                        if (rijndaelKeySetupDec(ctx.dk, key, bits) != rounds)
            //                        {
            //                            return -1;
            //                        }

            //                        ctx.Nr = rounds;
            //                        ctx.enc_only = 0;

            //                        return 0;
            //                    }

            //                    /* setup key context for encryption only */
            //                    public static int rijndael_set_key_enc_only(RijndaelCtx ctx, byte key, int bits)
            //                    {
            //                        int rounds;

            //                        rounds = rijndaelKeySetupEnc(ctx.ek, key, bits);
            //                        if (rounds == 0)
            //                        {
            //                            return -1;
            //                        }

            //                        ctx.Nr = rounds;
            //                        ctx.enc_only = 1;

            //                        return 0;
            //                    }
            //                    public static void rijndael_decrypt(RijndaelCtx ctx, byte src, ref byte dst)
            //                    {
            //                        rijndaelDecrypt(ctx.dk, ctx.Nr, src, dst);
            //                    }
            //                    public static void rijndael_encrypt(RijndaelCtx ctx, byte src, ref byte dst)
            //                    {
            //                        rijndaelEncrypt(ctx.ek, ctx.Nr, src, dst);
            //                    }

            //                    public static int AES_set_key(AesCtx ctx, byte key, int bits)
            //                    {
            //                        return rijndael_set_key((RijndaelCtx)ctx, key, bits);
            //                    }
            //                    public static void AES_encrypt(AesCtx ctx, byte src, ref byte dst)
            //                    {
            //                        rijndaelEncrypt(ctx.ek, ctx.Nr, src, dst);
            //                    }
            //                    public static void AES_decrypt(AesCtx ctx, byte src, ref byte dst)
            //                    {
            //                        rijndaelDecrypt(ctx.dk, ctx.Nr, src, dst);
            //                    }

            //                    //No IV support!
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'src', so pointers on this parameter are left unchanged:
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'dst', so pointers on this parameter are left unchanged:
            //                    public static void AES_cbc_encrypt(AesCtx ctx, byte src, byte dst, int size)
            //                    {
            //                        byte[] block_buff = new byte[16];

            //                        int i;
            //                        for (i = 0; i < size; i += 16)
            //                        {
            //                            //step 1: copy block to dst
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(dst, src, 16);
            //                            //step 2: XOR with previous block
            //                            if (i != 0)
            //                            {
            //                                xor_128(dst, block_buff, dst);
            //                            }
            //                            //step 3: encrypt the block -> it land in block buffer
            //                            AES_encrypt(ctx, dst, ref block_buff);
            //                            //step 4: copy back the encrypted block to destination
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(dst, block_buff, 16);

            //                            dst += 16;
            //                            src += 16;
            //                        }
            //                    }
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'src', so pointers on this parameter are left unchanged:
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'dst', so pointers on this parameter are left unchanged:
            //                    public static void AES_cbc_decrypt(AesCtx ctx, byte src, byte dst, int size)
            //                    {
            //                        byte[] block_buff = new byte[16];
            //                        byte[] block_buff_previous = new byte[16];
            //                        int i;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(block_buff, src, 16);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(block_buff_previous, src, 16);
            //                        AES_decrypt(ctx, src, ref dst);

            //                        dst += 16;
            //                        src += 16;

            //                        for (i = 16; i < size; i += 16)
            //                        {
            //                            //step1: backup current block for next block decrypt
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(block_buff, src, 16);
            //                            //step2: copy current block to destination
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(dst, src, 16);
            //                            //step3: decrypt current buffer in place
            //                            AES_decrypt(ctx, dst, ref dst);
            //                            //step4: XOR current buffer with previous buffer
            //                            xor_128(dst, block_buff_previous, dst);
            //                            //step5: swap buffers
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(block_buff_previous, block_buff, 16);

            //                            dst += 16;
            //                            src += 16;
            //                        }
            //                    }
            //                    public static void AES_CMAC(AesCtx ctx, byte[] input, int length, byte[] mac)
            //                    {
            //                        byte[] X = new byte[16];
            //                        byte[] Y = new byte[16];
            //                        byte[] M_last = new byte[16];
            //                        byte[] padded = new byte[16];
            //                        byte[] K1 = new byte[16];
            //                        byte[] K2 = new byte[16];
            //                        int n;
            //                        int i;
            //                        int flag;
            //                        generate_subkey(ctx, K1, ref K2);

            //                        n = (length + 15) / 16; // n is number of rounds

            //                        if (n == 0)
            //                        {
            //                            n = 1;
            //                            flag = 0;
            //                        }
            //                        else
            //                        {
            //                            if ((length % 16) == 0)
            //                            { // last block is a complete block
            //                                flag = 1;
            //                            }
            //                            else
            //                            { // last block is not complete block
            //                                flag = 0;
            //                            }

            //                        }

            //                        if (flag != 0)
            //                        { // last block is complete block
            //                            xor_128(input[16 * (n - 1)], K1, M_last);
            //                        }
            //                        else
            //                        {
            //                            padding(input[16 * (n - 1)], padded, length % 16);
            //                            xor_128(padded, K2, M_last);
            //                        }

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            X[i] = 0;
            //                        }
            //                        for (i = 0; i < n - 1; i++)
            //                        {
            //                            xor_128(X, input[16 * i], Y); // Y := Mi (+) X
            //                            AES_encrypt(ctx, Y, ref X); // X := AES-128(KEY, Y);
            //                        }

            //                        xor_128(X, M_last, Y);
            //                        AES_encrypt(ctx, Y, ref X);

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            mac[i] = X[i];
            //                        }
            //                    }

            //                    /**
            //                     * Expand the cipher key into the encryption key schedule.
            //                     *
            //                     * @return	the number of rounds for the given cipher key size.
            //                     */

            //                    public static int rijndaelKeySetupEnc(uint[] rk, byte[] cipherKey, int keyBits)
            //                    {
            //                        int i = 0;
            //                        uint temp;

            //                        rk[0] = (((uint)(cipherKey)[0] << 24) ^ ((uint)(cipherKey)[1] << 16) ^ ((uint)(cipherKey)[2] << 8) ^ ((uint)(cipherKey)[3]));
            //                        rk[1] = (((uint)(cipherKey + 4)[0] << 24) ^ ((uint)(cipherKey + 4)[1] << 16) ^ ((uint)(cipherKey + 4)[2] << 8) ^ ((uint)(cipherKey + 4)[3]));
            //                        rk[2] = (((uint)(cipherKey + 8)[0] << 24) ^ ((uint)(cipherKey + 8)[1] << 16) ^ ((uint)(cipherKey + 8)[2] << 8) ^ ((uint)(cipherKey + 8)[3]));
            //                        rk[3] = (((uint)(cipherKey + 12)[0] << 24) ^ ((uint)(cipherKey + 12)[1] << 16) ^ ((uint)(cipherKey + 12)[2] << 8) ^ ((uint)(cipherKey + 12)[3]));
            //                        if (keyBits == 128)
            //                        {
            //                            for (;;)
            //                            {
            //                                temp = rk[3];
            //                                rk[4] = rk[0] ^ (Te4[(temp >> 16) & 0xff] & 0xff000000) ^ (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^ (Te4[(temp) & 0xff] & 0x0000ff00) ^ (Te4[(temp >> 24)] & 0x000000ff) ^ rcon[i];
            //                                rk[5] = rk[1] ^ rk[4];
            //                                rk[6] = rk[2] ^ rk[5];
            //                                rk[7] = rk[3] ^ rk[6];
            //                                if (++i == 10)
            //                                {
            //                                    return 10;
            //                                }
            //                                rk += 4;
            //                            }
            //                        }
            //                        rk[4] = (((uint)(cipherKey + 16)[0] << 24) ^ ((uint)(cipherKey + 16)[1] << 16) ^ ((uint)(cipherKey + 16)[2] << 8) ^ ((uint)(cipherKey + 16)[3]));
            //                        rk[5] = (((uint)(cipherKey + 20)[0] << 24) ^ ((uint)(cipherKey + 20)[1] << 16) ^ ((uint)(cipherKey + 20)[2] << 8) ^ ((uint)(cipherKey + 20)[3]));
            //                        if (keyBits == 192)
            //                        {
            //                            for (;;)
            //                            {
            //                                temp = rk[5];
            //                                rk[6] = rk[0] ^ (Te4[(temp >> 16) & 0xff] & 0xff000000) ^ (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^ (Te4[(temp) & 0xff] & 0x0000ff00) ^ (Te4[(temp >> 24)] & 0x000000ff) ^ rcon[i];
            //                                rk[7] = rk[1] ^ rk[6];
            //                                rk[8] = rk[2] ^ rk[7];
            //                                rk[9] = rk[3] ^ rk[8];
            //                                if (++i == 8)
            //                                {
            //                                    return 12;
            //                                }
            //                                rk[10] = rk[4] ^ rk[9];
            //                                rk[11] = rk[5] ^ rk[10];
            //                                rk += 6;
            //                            }
            //                        }
            //                        rk[6] = (((uint)(cipherKey + 24)[0] << 24) ^ ((uint)(cipherKey + 24)[1] << 16) ^ ((uint)(cipherKey + 24)[2] << 8) ^ ((uint)(cipherKey + 24)[3]));
            //                        rk[7] = (((uint)(cipherKey + 28)[0] << 24) ^ ((uint)(cipherKey + 28)[1] << 16) ^ ((uint)(cipherKey + 28)[2] << 8) ^ ((uint)(cipherKey + 28)[3]));
            //                        if (keyBits == 256)
            //                        {
            //                            for (;;)
            //                            {
            //                                temp = rk[7];
            //                                rk[8] = rk[0] ^ (Te4[(temp >> 16) & 0xff] & 0xff000000) ^ (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^ (Te4[(temp) & 0xff] & 0x0000ff00) ^ (Te4[(temp >> 24)] & 0x000000ff) ^ rcon[i];
            //                                rk[9] = rk[1] ^ rk[8];
            //                                rk[10] = rk[2] ^ rk[9];
            //                                rk[11] = rk[3] ^ rk[10];
            //                                if (++i == 7)
            //                                {
            //                                    return 14;
            //                                }
            //                                temp = rk[11];
            //                                rk[12] = rk[4] ^ (Te4[(temp >> 24)] & 0xff000000) ^ (Te4[(temp >> 16) & 0xff] & 0x00ff0000) ^ (Te4[(temp >> 8) & 0xff] & 0x0000ff00) ^ (Te4[(temp) & 0xff] & 0x000000ff);
            //                                rk[13] = rk[5] ^ rk[12];
            //                                rk[14] = rk[6] ^ rk[13];
            //                                rk[15] = rk[7] ^ rk[14];
            //                                rk += 8;
            //                            }
            //                        }
            //                        return 0;
            //                    }

            //                    /// <summary>
            //                    /// Expand the cipher key into the encryption key schedule.
            //                    /// </summary>
            //                    /// <param name="rk"></param>
            //                    /// <param name="cipherKey"></param>
            //                    /// <param name="keyBits"></param>
            //                    /// <returns>the number of rounds for the given cipher key size.</returns>
            //                    public static int RijndaelKeySetupEnc(uint* rk /*4*(Nr + 1)*/, byte* cipherKey, int keyBits)
            //                    {
            //                        var i = 0;
            //                        uint temp;

            //                        rk[0] = GeTuint(cipherKey);
            //                        rk[1] = GeTuint(cipherKey + 4);
            //                        rk[2] = GeTuint(cipherKey + 8);
            //                        rk[3] = GeTuint(cipherKey + 12);
            //                        if (keyBits == 128)
            //                        {
            //                            for (;;)
            //                            {
            //                                temp = rk[3];
            //                                rk[4] = rk[0] ^
            //                                        (Te4[(temp >> 16) & 0xff] & 0xff000000) ^
            //                                        (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^
            //                                        (Te4[(temp) & 0xff] & 0x0000ff00) ^
            //                                        (Te4[(temp >> 24)] & 0x000000ff) ^
            //                                        Rcon[i];
            //                                rk[5] = rk[1] ^ rk[4];
            //                                rk[6] = rk[2] ^ rk[5];
            //                                rk[7] = rk[3] ^ rk[6];
            //                                if (++i == 10)
            //                                {
            //                                    return 10;
            //                                }
            //                                rk += 4;
            //                            }
            //                        }
            //                        rk[4] = GeTuint(cipherKey + 16);
            //                        rk[5] = GeTuint(cipherKey + 20);
            //                        if (keyBits == 192)
            //                        {
            //                            for (;;)
            //                            {
            //                                temp = rk[5];
            //                                rk[6] = rk[0] ^
            //                                        (Te4[(temp >> 16) & 0xff] & 0xff000000) ^
            //                                        (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^
            //                                        (Te4[(temp) & 0xff] & 0x0000ff00) ^
            //                                        (Te4[(temp >> 24)] & 0x000000ff) ^
            //                                        Rcon[i];
            //                                rk[7] = rk[1] ^ rk[6];
            //                                rk[8] = rk[2] ^ rk[7];
            //                                rk[9] = rk[3] ^ rk[8];
            //                                if (++i == 8)
            //                                {
            //                                    return 12;
            //                                }
            //                                rk[10] = rk[4] ^ rk[9];
            //                                rk[11] = rk[5] ^ rk[10];
            //                                rk += 6;
            //                            }
            //                        }
            //                        rk[6] = GeTuint(cipherKey + 24);
            //                        rk[7] = GeTuint(cipherKey + 28);
            //                        if (keyBits != 256) return 0;
            //                        for (;;)
            //                        {
            //                            temp = rk[7];
            //                            rk[8] = rk[0] ^
            //                                    (Te4[(temp >> 16) & 0xff] & 0xff000000) ^
            //                                    (Te4[(temp >> 8) & 0xff] & 0x00ff0000) ^
            //                                    (Te4[(temp) & 0xff] & 0x0000ff00) ^
            //                                    (Te4[(temp >> 24)] & 0x000000ff) ^
            //                                    Rcon[i];
            //                            rk[9] = rk[1] ^ rk[8];
            //                            rk[10] = rk[2] ^ rk[9];
            //                            rk[11] = rk[3] ^ rk[10];
            //                            if (++i == 7)
            //                            {
            //                                return 14;
            //                            }
            //                            temp = rk[11];
            //                            rk[12] = rk[4] ^
            //                                     (Te4[(temp >> 24)] & 0xff000000) ^
            //                                     (Te4[(temp >> 16) & 0xff] & 0x00ff0000) ^
            //                                     (Te4[(temp >> 8) & 0xff] & 0x0000ff00) ^
            //                                     (Te4[(temp) & 0xff] & 0x000000ff);
            //                            rk[13] = rk[5] ^ rk[12];
            //                            rk[14] = rk[6] ^ rk[13];
            //                            rk[15] = rk[7] ^ rk[14];
            //                            rk += 8;
            //                        }
            //                    }

            //                    /// <summary>
            //                    /// Expand the cipher key into the decryption key schedule.
            //                    /// </summary>
            //                    /// <param name="rk"></param>
            //                    /// <param name="cipherKey"></param>
            //                    /// <param name="keyBits"></param>
            //                    /// <returns>the number of rounds for the given cipher key size.</returns>
            //                    public static int RijndaelKeySetupDec(uint* rk /*4*(Nr + 1)*/, byte* cipherKey, int keyBits)
            //                    {
            //                        int i, j;

            //                        /* expand the cipher key: */
            //                        var nr = RijndaelKeySetupEnc(rk, cipherKey, keyBits);

            //                        /* invert the order of the round keys: */
            //                        for (i = 0, j = 4 * nr; i < j; i += 4, j -= 4)
            //                        {
            //                            var temp = rk[i];
            //                            rk[i] = rk[j];
            //                            rk[j] = temp;
            //                            temp = rk[i + 1];
            //                            rk[i + 1] = rk[j + 1];
            //                            rk[j + 1] = temp;
            //                            temp = rk[i + 2];
            //                            rk[i + 2] = rk[j + 2];
            //                            rk[j + 2] = temp;
            //                            temp = rk[i + 3];
            //                            rk[i + 3] = rk[j + 3];
            //                            rk[j + 3] = temp;
            //                        }
            //                        /* apply the inverse MixColumn transform to all round keys but the first and the last: */
            //                        for (i = 1; i < nr; i++)
            //                        {
            //                            rk += 4;
            //                            rk[0] =
            //                                Td0[Te4[(rk[0] >> 24)] & 0xff] ^
            //                                Td1[Te4[(rk[0] >> 16) & 0xff] & 0xff] ^
            //                                Td2[Te4[(rk[0] >> 8) & 0xff] & 0xff] ^
            //                                Td3[Te4[(rk[0]) & 0xff] & 0xff];
            //                            rk[1] =
            //                                Td0[Te4[(rk[1] >> 24)] & 0xff] ^
            //                                Td1[Te4[(rk[1] >> 16) & 0xff] & 0xff] ^
            //                                Td2[Te4[(rk[1] >> 8) & 0xff] & 0xff] ^
            //                                Td3[Te4[(rk[1]) & 0xff] & 0xff];
            //                            rk[2] =
            //                                Td0[Te4[(rk[2] >> 24)] & 0xff] ^
            //                                Td1[Te4[(rk[2] >> 16) & 0xff] & 0xff] ^
            //                                Td2[Te4[(rk[2] >> 8) & 0xff] & 0xff] ^
            //                                Td3[Te4[(rk[2]) & 0xff] & 0xff];
            //                            rk[3] =
            //                                Td0[Te4[(rk[3] >> 24)] & 0xff] ^
            //                                Td1[Te4[(rk[3] >> 16) & 0xff] & 0xff] ^
            //                                Td2[Te4[(rk[3] >> 8) & 0xff] & 0xff] ^
            //                                Td3[Te4[(rk[3]) & 0xff] & 0xff];
            //                        }
            //                        return nr;
            //                    }


            //                    /**
            //                     * Expand the cipher key into the decryption key schedule.
            //                     *
            //                     * @return	the number of rounds for the given cipher key size.
            //                     */
            //                    public static int rijndaelKeySetupDec(uint[] rk, byte[] cipherKey, int keyBits)
            //                    {
            //                        int Nr;
            //                        int i;
            //                        int j;
            //                        uint temp;

            //                        /* expand the cipher key: */
            //                        Nr = rijndaelKeySetupEnc(rk, cipherKey, keyBits);

            //                        /* invert the order of the round keys: */
            //                        for (i = 0, j = 4 * Nr; i < j; i += 4, j -= 4)
            //                        {
            //                            temp = rk[i];
            //                            rk[i] = rk[j];
            //                            rk[j] = temp;
            //                            temp = rk[i + 1];
            //                            rk[i + 1] = rk[j + 1];
            //                            rk[j + 1] = temp;
            //                            temp = rk[i + 2];
            //                            rk[i + 2] = rk[j + 2];
            //                            rk[j + 2] = temp;
            //                            temp = rk[i + 3];
            //                            rk[i + 3] = rk[j + 3];
            //                            rk[j + 3] = temp;
            //                        }
            //                        /* apply the inverse MixColumn transform to all round keys but the first and the last: */
            //                        for (i = 1; i < Nr; i++)
            //                        {
            //                            rk += 4;
            //                            rk[0] = Td0[Te4[(rk[0] >> 24)] & 0xff] ^ Td1[Te4[(rk[0] >> 16) & 0xff] & 0xff] ^ Td2[Te4[(rk[0] >> 8) & 0xff] & 0xff] ^ Td3[Te4[(rk[0]) & 0xff] & 0xff];
            //                            rk[1] = Td0[Te4[(rk[1] >> 24)] & 0xff] ^ Td1[Te4[(rk[1] >> 16) & 0xff] & 0xff] ^ Td2[Te4[(rk[1] >> 8) & 0xff] & 0xff] ^ Td3[Te4[(rk[1]) & 0xff] & 0xff];
            //                            rk[2] = Td0[Te4[(rk[2] >> 24)] & 0xff] ^ Td1[Te4[(rk[2] >> 16) & 0xff] & 0xff] ^ Td2[Te4[(rk[2] >> 8) & 0xff] & 0xff] ^ Td3[Te4[(rk[2]) & 0xff] & 0xff];
            //                            rk[3] = Td0[Te4[(rk[3] >> 24)] & 0xff] ^ Td1[Te4[(rk[3] >> 16) & 0xff] & 0xff] ^ Td2[Te4[(rk[3] >> 8) & 0xff] & 0xff] ^ Td3[Te4[(rk[3]) & 0xff] & 0xff];
            //                        }
            //                        return Nr;
            //                    }
            //                    public static void rijndaelEncrypt(uint[] rk, int Nr, byte[] pt, byte[] ct)
            //                    {
            //                        uint s0;
            //                        uint s1;
            //                        uint s2;
            //                        uint s3;
            //                        uint t0;
            //                        uint t1;
            //                        uint t2;
            //                        uint t3;
            //#if !FULL_UNROLL
            //                        int r;
            //#endif

            //                        /*
            //                         * map byte array block to cipher state
            //                         * and add initial round key:
            //                         */
            //                        s0 = (((uint)(pt)[0] << 24) ^ ((uint)(pt)[1] << 16) ^ ((uint)(pt)[2] << 8) ^ ((uint)(pt)[3])) ^ rk[0];
            //                        s1 = (((uint)(pt + 4)[0] << 24) ^ ((uint)(pt + 4)[1] << 16) ^ ((uint)(pt + 4)[2] << 8) ^ ((uint)(pt + 4)[3])) ^ rk[1];
            //                        s2 = (((uint)(pt + 8)[0] << 24) ^ ((uint)(pt + 8)[1] << 16) ^ ((uint)(pt + 8)[2] << 8) ^ ((uint)(pt + 8)[3])) ^ rk[2];
            //                        s3 = (((uint)(pt + 12)[0] << 24) ^ ((uint)(pt + 12)[1] << 16) ^ ((uint)(pt + 12)[2] << 8) ^ ((uint)(pt + 12)[3])) ^ rk[3];
            //#if FULL_UNROLL
            //		/* round 1: */
            //		   t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[4];
            //		   t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[5];
            //		   t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[6];
            //		   t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[7];
            //		   /* round 2: */
            //		   s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >> 8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[8];
            //		   s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >> 8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[9];
            //		   s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >> 8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[10];
            //		   s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >> 8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[11];
            //		/* round 3: */
            //		   t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[12];
            //		   t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[13];
            //		   t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[14];
            //		   t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[15];
            //		   /* round 4: */
            //		   s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >> 8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[16];
            //		   s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >> 8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[17];
            //		   s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >> 8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[18];
            //		   s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >> 8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[19];
            //		/* round 5: */
            //		   t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[20];
            //		   t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[21];
            //		   t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[22];
            //		   t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[23];
            //		   /* round 6: */
            //		   s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >> 8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[24];
            //		   s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >> 8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[25];
            //		   s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >> 8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[26];
            //		   s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >> 8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[27];
            //		/* round 7: */
            //		   t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[28];
            //		   t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[29];
            //		   t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[30];
            //		   t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[31];
            //		   /* round 8: */
            //		   s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >> 8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[32];
            //		   s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >> 8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[33];
            //		   s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >> 8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[34];
            //		   s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >> 8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[35];
            //		/* round 9: */
            //		   t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[36];
            //		   t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[37];
            //		   t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[38];
            //		   t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[39];
            //		if (Nr > 10)
            //		{
            //		/* round 10: */
            //		s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >> 8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[40];
            //		s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >> 8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[41];
            //		s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >> 8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[42];
            //		s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >> 8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[43];
            //		/* round 11: */
            //		t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[44];
            //		t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[45];
            //		t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[46];
            //		t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[47];
            //		if (Nr > 12)
            //		{
            //			/* round 12: */
            //			s0 = Te0[t0 >> 24] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >> 8) & 0xff] ^ Te3[t3 & 0xff] ^ rk[48];
            //			s1 = Te0[t1 >> 24] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >> 8) & 0xff] ^ Te3[t0 & 0xff] ^ rk[49];
            //			s2 = Te0[t2 >> 24] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >> 8) & 0xff] ^ Te3[t1 & 0xff] ^ rk[50];
            //			s3 = Te0[t3 >> 24] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >> 8) & 0xff] ^ Te3[t2 & 0xff] ^ rk[51];
            //			/* round 13: */
            //			t0 = Te0[s0 >> 24] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[s3 & 0xff] ^ rk[52];
            //			t1 = Te0[s1 >> 24] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[s0 & 0xff] ^ rk[53];
            //			t2 = Te0[s2 >> 24] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[s1 & 0xff] ^ rk[54];
            //			t3 = Te0[s3 >> 24] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[s2 & 0xff] ^ rk[55];
            //		}
            //		}
            //		rk += Nr << 2;
            //#else
            //                        /*
            //                         * Nr - 1 full rounds:
            //                         */
            //                        r = Nr >> 1;
            //                        for (;;)
            //                        {
            //                            t0 = Te0[(s0 >> 24)] ^ Te1[(s1 >> 16) & 0xff] ^ Te2[(s2 >> 8) & 0xff] ^ Te3[(s3) & 0xff] ^ rk[4];
            //                            t1 = Te0[(s1 >> 24)] ^ Te1[(s2 >> 16) & 0xff] ^ Te2[(s3 >> 8) & 0xff] ^ Te3[(s0) & 0xff] ^ rk[5];
            //                            t2 = Te0[(s2 >> 24)] ^ Te1[(s3 >> 16) & 0xff] ^ Te2[(s0 >> 8) & 0xff] ^ Te3[(s1) & 0xff] ^ rk[6];
            //                            t3 = Te0[(s3 >> 24)] ^ Te1[(s0 >> 16) & 0xff] ^ Te2[(s1 >> 8) & 0xff] ^ Te3[(s2) & 0xff] ^ rk[7];

            //                            rk += 8;
            //                            if (--r == 0)
            //                            {
            //                                break;
            //                            }

            //                            s0 = Te0[(t0 >> 24)] ^ Te1[(t1 >> 16) & 0xff] ^ Te2[(t2 >> 8) & 0xff] ^ Te3[(t3) & 0xff] ^ rk[0];
            //                            s1 = Te0[(t1 >> 24)] ^ Te1[(t2 >> 16) & 0xff] ^ Te2[(t3 >> 8) & 0xff] ^ Te3[(t0) & 0xff] ^ rk[1];
            //                            s2 = Te0[(t2 >> 24)] ^ Te1[(t3 >> 16) & 0xff] ^ Te2[(t0 >> 8) & 0xff] ^ Te3[(t1) & 0xff] ^ rk[2];
            //                            s3 = Te0[(t3 >> 24)] ^ Te1[(t0 >> 16) & 0xff] ^ Te2[(t1 >> 8) & 0xff] ^ Te3[(t2) & 0xff] ^ rk[3];
            //                        }
            //#endif
            //                        /*
            //                         * apply last round and
            //                         * map cipher state to byte array block:
            //                         */
            //                        s0 = (Te4[(t0 >> 24)] & 0xff000000) ^ (Te4[(t1 >> 16) & 0xff] & 0x00ff0000) ^ (Te4[(t2 >> 8) & 0xff] & 0x0000ff00) ^ (Te4[(t3) & 0xff] & 0x000000ff) ^ rk[0];
            //                        {
            //                            (ct)[0] = (byte)((s0) >> 24);
            //                            (ct)[1] = (byte)((s0) >> 16);
            //                            (ct)[2] = (byte)((s0) >> 8);
            //                            (ct)[3] = (byte)(s0);
            //                        };
            //                        s1 = (Te4[(t1 >> 24)] & 0xff000000) ^ (Te4[(t2 >> 16) & 0xff] & 0x00ff0000) ^ (Te4[(t3 >> 8) & 0xff] & 0x0000ff00) ^ (Te4[(t0) & 0xff] & 0x000000ff) ^ rk[1];
            //                        {
            //                            (ct + 4)[0] = (byte)((s1) >> 24);
            //                            (ct + 4)[1] = (byte)((s1) >> 16);
            //                            (ct + 4)[2] = (byte)((s1) >> 8);
            //                            (ct + 4)[3] = (byte)(s1);
            //                        };
            //                        s2 = (Te4[(t2 >> 24)] & 0xff000000) ^ (Te4[(t3 >> 16) & 0xff] & 0x00ff0000) ^ (Te4[(t0 >> 8) & 0xff] & 0x0000ff00) ^ (Te4[(t1) & 0xff] & 0x000000ff) ^ rk[2];
            //                        {
            //                            (ct + 8)[0] = (byte)((s2) >> 24);
            //                            (ct + 8)[1] = (byte)((s2) >> 16);
            //                            (ct + 8)[2] = (byte)((s2) >> 8);
            //                            (ct + 8)[3] = (byte)(s2);
            //                        };
            //                        s3 = (Te4[(t3 >> 24)] & 0xff000000) ^ (Te4[(t0 >> 16) & 0xff] & 0x00ff0000) ^ (Te4[(t1 >> 8) & 0xff] & 0x0000ff00) ^ (Te4[(t2) & 0xff] & 0x000000ff) ^ rk[3];
            //                        {
            //                            (ct + 12)[0] = (byte)((s3) >> 24);
            //                            (ct + 12)[1] = (byte)((s3) >> 16);
            //                            (ct + 12)[2] = (byte)((s3) >> 8);
            //                            (ct + 12)[3] = (byte)(s3);
            //                        };
            //                    }


            //                    //CMAC GLOBS
            //                    public static byte[] const_Rb = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x87 };
            //                    public static byte[] const_Zero = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //                    //END

            //                    /*
            //                    Te0[x] = S [x].[02, 01, 01, 03];
            //                    Te1[x] = S [x].[03, 02, 01, 01];
            //                    Te2[x] = S [x].[01, 03, 02, 01];
            //                    Te3[x] = S [x].[01, 01, 03, 02];
            //                    Te4[x] = S [x].[01, 01, 01, 01];

            //                    Td0[x] = Si[x].[0e, 09, 0d, 0b];
            //                    Td1[x] = Si[x].[0b, 0e, 09, 0d];
            //                    Td2[x] = Si[x].[0d, 0b, 0e, 09];
            //                    Td3[x] = Si[x].[09, 0d, 0b, 0e];
            //                    Td4[x] = Si[x].[01, 01, 01, 01];
            //                    */

            //                    internal static uint[] Te0 = { 0xc66363a5U, 0xf87c7c84U, 0xee777799U, 0xf67b7b8dU, 0xfff2f20dU, 0xd66b6bbdU, 0xde6f6fb1U, 0x91c5c554U, 0x60303050U, 0x02010103U, 0xce6767a9U, 0x562b2b7dU, 0xe7fefe19U, 0xb5d7d762U, 0x4dababe6U, 0xec76769aU, 0x8fcaca45U, 0x1f82829dU, 0x89c9c940U, 0xfa7d7d87U, 0xeffafa15U, 0xb25959ebU, 0x8e4747c9U, 0xfbf0f00bU, 0x41adadecU, 0xb3d4d467U, 0x5fa2a2fdU, 0x45afafeaU, 0x239c9cbfU, 0x53a4a4f7U, 0xe4727296U, 0x9bc0c05bU, 0x75b7b7c2U, 0xe1fdfd1cU, 0x3d9393aeU, 0x4c26266aU, 0x6c36365aU, 0x7e3f3f41U, 0xf5f7f702U, 0x83cccc4fU, 0x6834345cU, 0x51a5a5f4U, 0xd1e5e534U, 0xf9f1f108U, 0xe2717193U, 0xabd8d873U, 0x62313153U, 0x2a15153fU, 0x0804040cU, 0x95c7c752U, 0x46232365U, 0x9dc3c35eU, 0x30181828U, 0x379696a1U, 0x0a05050fU, 0x2f9a9ab5U, 0x0e070709U, 0x24121236U, 0x1b80809bU, 0xdfe2e23dU, 0xcdebeb26U, 0x4e272769U, 0x7fb2b2cdU, 0xea75759fU, 0x1209091bU, 0x1d83839eU, 0x582c2c74U, 0x341a1a2eU, 0x361b1b2dU, 0xdc6e6eb2U, 0xb45a5aeeU, 0x5ba0a0fbU, 0xa45252f6U, 0x763b3b4dU, 0xb7d6d661U, 0x7db3b3ceU, 0x5229297bU, 0xdde3e33eU, 0x5e2f2f71U, 0x13848497U, 0xa65353f5U, 0xb9d1d168U, 0x00000000U, 0xc1eded2cU, 0x40202060U, 0xe3fcfc1fU, 0x79b1b1c8U, 0xb65b5bedU, 0xd46a6abeU, 0x8dcbcb46U, 0x67bebed9U, 0x7239394bU, 0x944a4adeU, 0x984c4cd4U, 0xb05858e8U, 0x85cfcf4aU, 0xbbd0d06bU, 0xc5efef2aU, 0x4faaaae5U, 0xedfbfb16U, 0x864343c5U, 0x9a4d4dd7U, 0x66333355U, 0x11858594U, 0x8a4545cfU, 0xe9f9f910U, 0x04020206U, 0xfe7f7f81U, 0xa05050f0U, 0x783c3c44U, 0x259f9fbaU, 0x4ba8a8e3U, 0xa25151f3U, 0x5da3a3feU, 0x804040c0U, 0x058f8f8aU, 0x3f9292adU, 0x219d9dbcU, 0x70383848U, 0xf1f5f504U, 0x63bcbcdfU, 0x77b6b6c1U, 0xafdada75U, 0x42212163U, 0x20101030U, 0xe5ffff1aU, 0xfdf3f30eU, 0xbfd2d26dU, 0x81cdcd4cU, 0x180c0c14U, 0x26131335U, 0xc3ecec2fU, 0xbe5f5fe1U, 0x359797a2U, 0x884444ccU, 0x2e171739U, 0x93c4c457U, 0x55a7a7f2U, 0xfc7e7e82U, 0x7a3d3d47U, 0xc86464acU, 0xba5d5de7U, 0x3219192bU, 0xe6737395U, 0xc06060a0U, 0x19818198U, 0x9e4f4fd1U, 0xa3dcdc7fU, 0x44222266U, 0x542a2a7eU, 0x3b9090abU, 0x0b888883U, 0x8c4646caU, 0xc7eeee29U, 0x6bb8b8d3U, 0x2814143cU, 0xa7dede79U, 0xbc5e5ee2U, 0x160b0b1dU, 0xaddbdb76U, 0xdbe0e03bU, 0x64323256U, 0x743a3a4eU, 0x140a0a1eU, 0x924949dbU, 0x0c06060aU, 0x4824246cU, 0xb85c5ce4U, 0x9fc2c25dU, 0xbdd3d36eU, 0x43acacefU, 0xc46262a6U, 0x399191a8U, 0x319595a4U, 0xd3e4e437U, 0xf279798bU, 0xd5e7e732U, 0x8bc8c843U, 0x6e373759U, 0xda6d6db7U, 0x018d8d8cU, 0xb1d5d564U, 0x9c4e4ed2U, 0x49a9a9e0U, 0xd86c6cb4U, 0xac5656faU, 0xf3f4f407U, 0xcfeaea25U, 0xca6565afU, 0xf47a7a8eU, 0x47aeaee9U, 0x10080818U, 0x6fbabad5U, 0xf0787888U, 0x4a25256fU, 0x5c2e2e72U, 0x381c1c24U, 0x57a6a6f1U, 0x73b4b4c7U, 0x97c6c651U, 0xcbe8e823U, 0xa1dddd7cU, 0xe874749cU, 0x3e1f1f21U, 0x964b4bddU, 0x61bdbddcU, 0x0d8b8b86U, 0x0f8a8a85U, 0xe0707090U, 0x7c3e3e42U, 0x71b5b5c4U, 0xcc6666aaU, 0x904848d8U, 0x06030305U, 0xf7f6f601U, 0x1c0e0e12U, 0xc26161a3U, 0x6a35355fU, 0xae5757f9U, 0x69b9b9d0U, 0x17868691U, 0x99c1c158U, 0x3a1d1d27U, 0x279e9eb9U, 0xd9e1e138U, 0xebf8f813U, 0x2b9898b3U, 0x22111133U, 0xd26969bbU, 0xa9d9d970U, 0x078e8e89U, 0x339494a7U, 0x2d9b9bb6U, 0x3c1e1e22U, 0x15878792U, 0xc9e9e920U, 0x87cece49U, 0xaa5555ffU, 0x50282878U, 0xa5dfdf7aU, 0x038c8c8fU, 0x59a1a1f8U, 0x09898980U, 0x1a0d0d17U, 0x65bfbfdaU, 0xd7e6e631U, 0x844242c6U, 0xd06868b8U, 0x824141c3U, 0x299999b0U, 0x5a2d2d77U, 0x1e0f0f11U, 0x7bb0b0cbU, 0xa85454fcU, 0x6dbbbbd6U, 0x2c16163aU };

            //                    internal static uint[] Te1 = { 0xa5c66363U, 0x84f87c7cU, 0x99ee7777U, 0x8df67b7bU, 0x0dfff2f2U, 0xbdd66b6bU, 0xb1de6f6fU, 0x5491c5c5U, 0x50603030U, 0x03020101U, 0xa9ce6767U, 0x7d562b2bU, 0x19e7fefeU, 0x62b5d7d7U, 0xe64dababU, 0x9aec7676U, 0x458fcacaU, 0x9d1f8282U, 0x4089c9c9U, 0x87fa7d7dU, 0x15effafaU, 0xebb25959U, 0xc98e4747U, 0x0bfbf0f0U, 0xec41adadU, 0x67b3d4d4U, 0xfd5fa2a2U, 0xea45afafU, 0xbf239c9cU, 0xf753a4a4U, 0x96e47272U, 0x5b9bc0c0U, 0xc275b7b7U, 0x1ce1fdfdU, 0xae3d9393U, 0x6a4c2626U, 0x5a6c3636U, 0x417e3f3fU, 0x02f5f7f7U, 0x4f83ccccU, 0x5c683434U, 0xf451a5a5U, 0x34d1e5e5U, 0x08f9f1f1U, 0x93e27171U, 0x73abd8d8U, 0x53623131U, 0x3f2a1515U, 0x0c080404U, 0x5295c7c7U, 0x65462323U, 0x5e9dc3c3U, 0x28301818U, 0xa1379696U, 0x0f0a0505U, 0xb52f9a9aU, 0x090e0707U, 0x36241212U, 0x9b1b8080U, 0x3ddfe2e2U, 0x26cdebebU, 0x694e2727U, 0xcd7fb2b2U, 0x9fea7575U, 0x1b120909U, 0x9e1d8383U, 0x74582c2cU, 0x2e341a1aU, 0x2d361b1bU, 0xb2dc6e6eU, 0xeeb45a5aU, 0xfb5ba0a0U, 0xf6a45252U, 0x4d763b3bU, 0x61b7d6d6U, 0xce7db3b3U, 0x7b522929U, 0x3edde3e3U, 0x715e2f2fU, 0x97138484U, 0xf5a65353U, 0x68b9d1d1U, 0x00000000U, 0x2cc1ededU, 0x60402020U, 0x1fe3fcfcU, 0xc879b1b1U, 0xedb65b5bU, 0xbed46a6aU, 0x468dcbcbU, 0xd967bebeU, 0x4b723939U, 0xde944a4aU, 0xd4984c4cU, 0xe8b05858U, 0x4a85cfcfU, 0x6bbbd0d0U, 0x2ac5efefU, 0xe54faaaaU, 0x16edfbfbU, 0xc5864343U, 0xd79a4d4dU, 0x55663333U, 0x94118585U, 0xcf8a4545U, 0x10e9f9f9U, 0x06040202U, 0x81fe7f7fU, 0xf0a05050U, 0x44783c3cU, 0xba259f9fU, 0xe34ba8a8U, 0xf3a25151U, 0xfe5da3a3U, 0xc0804040U, 0x8a058f8fU, 0xad3f9292U, 0xbc219d9dU, 0x48703838U, 0x04f1f5f5U, 0xdf63bcbcU, 0xc177b6b6U, 0x75afdadaU, 0x63422121U, 0x30201010U, 0x1ae5ffffU, 0x0efdf3f3U, 0x6dbfd2d2U, 0x4c81cdcdU, 0x14180c0cU, 0x35261313U, 0x2fc3ececU, 0xe1be5f5fU, 0xa2359797U, 0xcc884444U, 0x392e1717U, 0x5793c4c4U, 0xf255a7a7U, 0x82fc7e7eU, 0x477a3d3dU, 0xacc86464U, 0xe7ba5d5dU, 0x2b321919U, 0x95e67373U, 0xa0c06060U, 0x98198181U, 0xd19e4f4fU, 0x7fa3dcdcU, 0x66442222U, 0x7e542a2aU, 0xab3b9090U, 0x830b8888U, 0xca8c4646U, 0x29c7eeeeU, 0xd36bb8b8U, 0x3c281414U, 0x79a7dedeU, 0xe2bc5e5eU, 0x1d160b0bU, 0x76addbdbU, 0x3bdbe0e0U, 0x56643232U, 0x4e743a3aU, 0x1e140a0aU, 0xdb924949U, 0x0a0c0606U, 0x6c482424U, 0xe4b85c5cU, 0x5d9fc2c2U, 0x6ebdd3d3U, 0xef43acacU, 0xa6c46262U, 0xa8399191U, 0xa4319595U, 0x37d3e4e4U, 0x8bf27979U, 0x32d5e7e7U, 0x438bc8c8U, 0x596e3737U, 0xb7da6d6dU, 0x8c018d8dU, 0x64b1d5d5U, 0xd29c4e4eU, 0xe049a9a9U, 0xb4d86c6cU, 0xfaac5656U, 0x07f3f4f4U, 0x25cfeaeaU, 0xafca6565U, 0x8ef47a7aU, 0xe947aeaeU, 0x18100808U, 0xd56fbabaU, 0x88f07878U, 0x6f4a2525U, 0x725c2e2eU, 0x24381c1cU, 0xf157a6a6U, 0xc773b4b4U, 0x5197c6c6U, 0x23cbe8e8U, 0x7ca1ddddU, 0x9ce87474U, 0x213e1f1fU, 0xdd964b4bU, 0xdc61bdbdU, 0x860d8b8bU, 0x850f8a8aU, 0x90e07070U, 0x427c3e3eU, 0xc471b5b5U, 0xaacc6666U, 0xd8904848U, 0x05060303U, 0x01f7f6f6U, 0x121c0e0eU, 0xa3c26161U, 0x5f6a3535U, 0xf9ae5757U, 0xd069b9b9U, 0x91178686U, 0x5899c1c1U, 0x273a1d1dU, 0xb9279e9eU, 0x38d9e1e1U, 0x13ebf8f8U, 0xb32b9898U, 0x33221111U, 0xbbd26969U, 0x70a9d9d9U, 0x89078e8eU, 0xa7339494U, 0xb62d9b9bU, 0x223c1e1eU, 0x92158787U, 0x20c9e9e9U, 0x4987ceceU, 0xffaa5555U, 0x78502828U, 0x7aa5dfdfU, 0x8f038c8cU, 0xf859a1a1U, 0x80098989U, 0x171a0d0dU, 0xda65bfbfU, 0x31d7e6e6U, 0xc6844242U, 0xb8d06868U, 0xc3824141U, 0xb0299999U, 0x775a2d2dU, 0x111e0f0fU, 0xcb7bb0b0U, 0xfca85454U, 0xd66dbbbbU, 0x3a2c1616U };

            //                    internal static uint[] Te2 = { 0x63a5c663U, 0x7c84f87cU, 0x7799ee77U, 0x7b8df67bU, 0xf20dfff2U, 0x6bbdd66bU, 0x6fb1de6fU, 0xc55491c5U, 0x30506030U, 0x01030201U, 0x67a9ce67U, 0x2b7d562bU, 0xfe19e7feU, 0xd762b5d7U, 0xabe64dabU, 0x769aec76U, 0xca458fcaU, 0x829d1f82U, 0xc94089c9U, 0x7d87fa7dU, 0xfa15effaU, 0x59ebb259U, 0x47c98e47U, 0xf00bfbf0U, 0xadec41adU, 0xd467b3d4U, 0xa2fd5fa2U, 0xafea45afU, 0x9cbf239cU, 0xa4f753a4U, 0x7296e472U, 0xc05b9bc0U, 0xb7c275b7U, 0xfd1ce1fdU, 0x93ae3d93U, 0x266a4c26U, 0x365a6c36U, 0x3f417e3fU, 0xf702f5f7U, 0xcc4f83ccU, 0x345c6834U, 0xa5f451a5U, 0xe534d1e5U, 0xf108f9f1U, 0x7193e271U, 0xd873abd8U, 0x31536231U, 0x153f2a15U, 0x040c0804U, 0xc75295c7U, 0x23654623U, 0xc35e9dc3U, 0x18283018U, 0x96a13796U, 0x050f0a05U, 0x9ab52f9aU, 0x07090e07U, 0x12362412U, 0x809b1b80U, 0xe23ddfe2U, 0xeb26cdebU, 0x27694e27U, 0xb2cd7fb2U, 0x759fea75U, 0x091b1209U, 0x839e1d83U, 0x2c74582cU, 0x1a2e341aU, 0x1b2d361bU, 0x6eb2dc6eU, 0x5aeeb45aU, 0xa0fb5ba0U, 0x52f6a452U, 0x3b4d763bU, 0xd661b7d6U, 0xb3ce7db3U, 0x297b5229U, 0xe33edde3U, 0x2f715e2fU, 0x84971384U, 0x53f5a653U, 0xd168b9d1U, 0x00000000U, 0xed2cc1edU, 0x20604020U, 0xfc1fe3fcU, 0xb1c879b1U, 0x5bedb65bU, 0x6abed46aU, 0xcb468dcbU, 0xbed967beU, 0x394b7239U, 0x4ade944aU, 0x4cd4984cU, 0x58e8b058U, 0xcf4a85cfU, 0xd06bbbd0U, 0xef2ac5efU, 0xaae54faaU, 0xfb16edfbU, 0x43c58643U, 0x4dd79a4dU, 0x33556633U, 0x85941185U, 0x45cf8a45U, 0xf910e9f9U, 0x02060402U, 0x7f81fe7fU, 0x50f0a050U, 0x3c44783cU, 0x9fba259fU, 0xa8e34ba8U, 0x51f3a251U, 0xa3fe5da3U, 0x40c08040U, 0x8f8a058fU, 0x92ad3f92U, 0x9dbc219dU, 0x38487038U, 0xf504f1f5U, 0xbcdf63bcU, 0xb6c177b6U, 0xda75afdaU, 0x21634221U, 0x10302010U, 0xff1ae5ffU, 0xf30efdf3U, 0xd26dbfd2U, 0xcd4c81cdU, 0x0c14180cU, 0x13352613U, 0xec2fc3ecU, 0x5fe1be5fU, 0x97a23597U, 0x44cc8844U, 0x17392e17U, 0xc45793c4U, 0xa7f255a7U, 0x7e82fc7eU, 0x3d477a3dU, 0x64acc864U, 0x5de7ba5dU, 0x192b3219U, 0x7395e673U, 0x60a0c060U, 0x81981981U, 0x4fd19e4fU, 0xdc7fa3dcU, 0x22664422U, 0x2a7e542aU, 0x90ab3b90U, 0x88830b88U, 0x46ca8c46U, 0xee29c7eeU, 0xb8d36bb8U, 0x143c2814U, 0xde79a7deU, 0x5ee2bc5eU, 0x0b1d160bU, 0xdb76addbU, 0xe03bdbe0U, 0x32566432U, 0x3a4e743aU, 0x0a1e140aU, 0x49db9249U, 0x060a0c06U, 0x246c4824U, 0x5ce4b85cU, 0xc25d9fc2U, 0xd36ebdd3U, 0xacef43acU, 0x62a6c462U, 0x91a83991U, 0x95a43195U, 0xe437d3e4U, 0x798bf279U, 0xe732d5e7U, 0xc8438bc8U, 0x37596e37U, 0x6db7da6dU, 0x8d8c018dU, 0xd564b1d5U, 0x4ed29c4eU, 0xa9e049a9U, 0x6cb4d86cU, 0x56faac56U, 0xf407f3f4U, 0xea25cfeaU, 0x65afca65U, 0x7a8ef47aU, 0xaee947aeU, 0x08181008U, 0xbad56fbaU, 0x7888f078U, 0x256f4a25U, 0x2e725c2eU, 0x1c24381cU, 0xa6f157a6U, 0xb4c773b4U, 0xc65197c6U, 0xe823cbe8U, 0xdd7ca1ddU, 0x749ce874U, 0x1f213e1fU, 0x4bdd964bU, 0xbddc61bdU, 0x8b860d8bU, 0x8a850f8aU, 0x7090e070U, 0x3e427c3eU, 0xb5c471b5U, 0x66aacc66U, 0x48d89048U, 0x03050603U, 0xf601f7f6U, 0x0e121c0eU, 0x61a3c261U, 0x355f6a35U, 0x57f9ae57U, 0xb9d069b9U, 0x86911786U, 0xc15899c1U, 0x1d273a1dU, 0x9eb9279eU, 0xe138d9e1U, 0xf813ebf8U, 0x98b32b98U, 0x11332211U, 0x69bbd269U, 0xd970a9d9U, 0x8e89078eU, 0x94a73394U, 0x9bb62d9bU, 0x1e223c1eU, 0x87921587U, 0xe920c9e9U, 0xce4987ceU, 0x55ffaa55U, 0x28785028U, 0xdf7aa5dfU, 0x8c8f038cU, 0xa1f859a1U, 0x89800989U, 0x0d171a0dU, 0xbfda65bfU, 0xe631d7e6U, 0x42c68442U, 0x68b8d068U, 0x41c38241U, 0x99b02999U, 0x2d775a2dU, 0x0f111e0fU, 0xb0cb7bb0U, 0x54fca854U, 0xbbd66dbbU, 0x163a2c16U };

            //                    internal static uint[] Te3 = { 0x6363a5c6U, 0x7c7c84f8U, 0x777799eeU, 0x7b7b8df6U, 0xf2f20dffU, 0x6b6bbdd6U, 0x6f6fb1deU, 0xc5c55491U, 0x30305060U, 0x01010302U, 0x6767a9ceU, 0x2b2b7d56U, 0xfefe19e7U, 0xd7d762b5U, 0xababe64dU, 0x76769aecU, 0xcaca458fU, 0x82829d1fU, 0xc9c94089U, 0x7d7d87faU, 0xfafa15efU, 0x5959ebb2U, 0x4747c98eU, 0xf0f00bfbU, 0xadadec41U, 0xd4d467b3U, 0xa2a2fd5fU, 0xafafea45U, 0x9c9cbf23U, 0xa4a4f753U, 0x727296e4U, 0xc0c05b9bU, 0xb7b7c275U, 0xfdfd1ce1U, 0x9393ae3dU, 0x26266a4cU, 0x36365a6cU, 0x3f3f417eU, 0xf7f702f5U, 0xcccc4f83U, 0x34345c68U, 0xa5a5f451U, 0xe5e534d1U, 0xf1f108f9U, 0x717193e2U, 0xd8d873abU, 0x31315362U, 0x15153f2aU, 0x04040c08U, 0xc7c75295U, 0x23236546U, 0xc3c35e9dU, 0x18182830U, 0x9696a137U, 0x05050f0aU, 0x9a9ab52fU, 0x0707090eU, 0x12123624U, 0x80809b1bU, 0xe2e23ddfU, 0xebeb26cdU, 0x2727694eU, 0xb2b2cd7fU, 0x75759feaU, 0x09091b12U, 0x83839e1dU, 0x2c2c7458U, 0x1a1a2e34U, 0x1b1b2d36U, 0x6e6eb2dcU, 0x5a5aeeb4U, 0xa0a0fb5bU, 0x5252f6a4U, 0x3b3b4d76U, 0xd6d661b7U, 0xb3b3ce7dU, 0x29297b52U, 0xe3e33eddU, 0x2f2f715eU, 0x84849713U, 0x5353f5a6U, 0xd1d168b9U, 0x00000000U, 0xeded2cc1U, 0x20206040U, 0xfcfc1fe3U, 0xb1b1c879U, 0x5b5bedb6U, 0x6a6abed4U, 0xcbcb468dU, 0xbebed967U, 0x39394b72U, 0x4a4ade94U, 0x4c4cd498U, 0x5858e8b0U, 0xcfcf4a85U, 0xd0d06bbbU, 0xefef2ac5U, 0xaaaae54fU, 0xfbfb16edU, 0x4343c586U, 0x4d4dd79aU, 0x33335566U, 0x85859411U, 0x4545cf8aU, 0xf9f910e9U, 0x02020604U, 0x7f7f81feU, 0x5050f0a0U, 0x3c3c4478U, 0x9f9fba25U, 0xa8a8e34bU, 0x5151f3a2U, 0xa3a3fe5dU, 0x4040c080U, 0x8f8f8a05U, 0x9292ad3fU, 0x9d9dbc21U, 0x38384870U, 0xf5f504f1U, 0xbcbcdf63U, 0xb6b6c177U, 0xdada75afU, 0x21216342U, 0x10103020U, 0xffff1ae5U, 0xf3f30efdU, 0xd2d26dbfU, 0xcdcd4c81U, 0x0c0c1418U, 0x13133526U, 0xecec2fc3U, 0x5f5fe1beU, 0x9797a235U, 0x4444cc88U, 0x1717392eU, 0xc4c45793U, 0xa7a7f255U, 0x7e7e82fcU, 0x3d3d477aU, 0x6464acc8U, 0x5d5de7baU, 0x19192b32U, 0x737395e6U, 0x6060a0c0U, 0x81819819U, 0x4f4fd19eU, 0xdcdc7fa3U, 0x22226644U, 0x2a2a7e54U, 0x9090ab3bU, 0x8888830bU, 0x4646ca8cU, 0xeeee29c7U, 0xb8b8d36bU, 0x14143c28U, 0xdede79a7U, 0x5e5ee2bcU, 0x0b0b1d16U, 0xdbdb76adU, 0xe0e03bdbU, 0x32325664U, 0x3a3a4e74U, 0x0a0a1e14U, 0x4949db92U, 0x06060a0cU, 0x24246c48U, 0x5c5ce4b8U, 0xc2c25d9fU, 0xd3d36ebdU, 0xacacef43U, 0x6262a6c4U, 0x9191a839U, 0x9595a431U, 0xe4e437d3U, 0x79798bf2U, 0xe7e732d5U, 0xc8c8438bU, 0x3737596eU, 0x6d6db7daU, 0x8d8d8c01U, 0xd5d564b1U, 0x4e4ed29cU, 0xa9a9e049U, 0x6c6cb4d8U, 0x5656faacU, 0xf4f407f3U, 0xeaea25cfU, 0x6565afcaU, 0x7a7a8ef4U, 0xaeaee947U, 0x08081810U, 0xbabad56fU, 0x787888f0U, 0x25256f4aU, 0x2e2e725cU, 0x1c1c2438U, 0xa6a6f157U, 0xb4b4c773U, 0xc6c65197U, 0xe8e823cbU, 0xdddd7ca1U, 0x74749ce8U, 0x1f1f213eU, 0x4b4bdd96U, 0xbdbddc61U, 0x8b8b860dU, 0x8a8a850fU, 0x707090e0U, 0x3e3e427cU, 0xb5b5c471U, 0x6666aaccU, 0x4848d890U, 0x03030506U, 0xf6f601f7U, 0x0e0e121cU, 0x6161a3c2U, 0x35355f6aU, 0x5757f9aeU, 0xb9b9d069U, 0x86869117U, 0xc1c15899U, 0x1d1d273aU, 0x9e9eb927U, 0xe1e138d9U, 0xf8f813ebU, 0x9898b32bU, 0x11113322U, 0x6969bbd2U, 0xd9d970a9U, 0x8e8e8907U, 0x9494a733U, 0x9b9bb62dU, 0x1e1e223cU, 0x87879215U, 0xe9e920c9U, 0xcece4987U, 0x5555ffaaU, 0x28287850U, 0xdfdf7aa5U, 0x8c8c8f03U, 0xa1a1f859U, 0x89898009U, 0x0d0d171aU, 0xbfbfda65U, 0xe6e631d7U, 0x4242c684U, 0x6868b8d0U, 0x4141c382U, 0x9999b029U, 0x2d2d775aU, 0x0f0f111eU, 0xb0b0cb7bU, 0x5454fca8U, 0xbbbbd66dU, 0x16163a2cU };

            //                    internal static uint[] Te4 = { 0x63636363U, 0x7c7c7c7cU, 0x77777777U, 0x7b7b7b7bU, 0xf2f2f2f2U, 0x6b6b6b6bU, 0x6f6f6f6fU, 0xc5c5c5c5U, 0x30303030U, 0x01010101U, 0x67676767U, 0x2b2b2b2bU, 0xfefefefeU, 0xd7d7d7d7U, 0xababababU, 0x76767676U, 0xcacacacaU, 0x82828282U, 0xc9c9c9c9U, 0x7d7d7d7dU, 0xfafafafaU, 0x59595959U, 0x47474747U, 0xf0f0f0f0U, 0xadadadadU, 0xd4d4d4d4U, 0xa2a2a2a2U, 0xafafafafU, 0x9c9c9c9cU, 0xa4a4a4a4U, 0x72727272U, 0xc0c0c0c0U, 0xb7b7b7b7U, 0xfdfdfdfdU, 0x93939393U, 0x26262626U, 0x36363636U, 0x3f3f3f3fU, 0xf7f7f7f7U, 0xccccccccU, 0x34343434U, 0xa5a5a5a5U, 0xe5e5e5e5U, 0xf1f1f1f1U, 0x71717171U, 0xd8d8d8d8U, 0x31313131U, 0x15151515U, 0x04040404U, 0xc7c7c7c7U, 0x23232323U, 0xc3c3c3c3U, 0x18181818U, 0x96969696U, 0x05050505U, 0x9a9a9a9aU, 0x07070707U, 0x12121212U, 0x80808080U, 0xe2e2e2e2U, 0xebebebebU, 0x27272727U, 0xb2b2b2b2U, 0x75757575U, 0x09090909U, 0x83838383U, 0x2c2c2c2cU, 0x1a1a1a1aU, 0x1b1b1b1bU, 0x6e6e6e6eU, 0x5a5a5a5aU, 0xa0a0a0a0U, 0x52525252U, 0x3b3b3b3bU, 0xd6d6d6d6U, 0xb3b3b3b3U, 0x29292929U, 0xe3e3e3e3U, 0x2f2f2f2fU, 0x84848484U, 0x53535353U, 0xd1d1d1d1U, 0x00000000U, 0xededededU, 0x20202020U, 0xfcfcfcfcU, 0xb1b1b1b1U, 0x5b5b5b5bU, 0x6a6a6a6aU, 0xcbcbcbcbU, 0xbebebebeU, 0x39393939U, 0x4a4a4a4aU, 0x4c4c4c4cU, 0x58585858U, 0xcfcfcfcfU, 0xd0d0d0d0U, 0xefefefefU, 0xaaaaaaaaU, 0xfbfbfbfbU, 0x43434343U, 0x4d4d4d4dU, 0x33333333U, 0x85858585U, 0x45454545U, 0xf9f9f9f9U, 0x02020202U, 0x7f7f7f7fU, 0x50505050U, 0x3c3c3c3cU, 0x9f9f9f9fU, 0xa8a8a8a8U, 0x51515151U, 0xa3a3a3a3U, 0x40404040U, 0x8f8f8f8fU, 0x92929292U, 0x9d9d9d9dU, 0x38383838U, 0xf5f5f5f5U, 0xbcbcbcbcU, 0xb6b6b6b6U, 0xdadadadaU, 0x21212121U, 0x10101010U, 0xffffffffU, 0xf3f3f3f3U, 0xd2d2d2d2U, 0xcdcdcdcdU, 0x0c0c0c0cU, 0x13131313U, 0xececececU, 0x5f5f5f5fU, 0x97979797U, 0x44444444U, 0x17171717U, 0xc4c4c4c4U, 0xa7a7a7a7U, 0x7e7e7e7eU, 0x3d3d3d3dU, 0x64646464U, 0x5d5d5d5dU, 0x19191919U, 0x73737373U, 0x60606060U, 0x81818181U, 0x4f4f4f4fU, 0xdcdcdcdcU, 0x22222222U, 0x2a2a2a2aU, 0x90909090U, 0x88888888U, 0x46464646U, 0xeeeeeeeeU, 0xb8b8b8b8U, 0x14141414U, 0xdedededeU, 0x5e5e5e5eU, 0x0b0b0b0bU, 0xdbdbdbdbU, 0xe0e0e0e0U, 0x32323232U, 0x3a3a3a3aU, 0x0a0a0a0aU, 0x49494949U, 0x06060606U, 0x24242424U, 0x5c5c5c5cU, 0xc2c2c2c2U, 0xd3d3d3d3U, 0xacacacacU, 0x62626262U, 0x91919191U, 0x95959595U, 0xe4e4e4e4U, 0x79797979U, 0xe7e7e7e7U, 0xc8c8c8c8U, 0x37373737U, 0x6d6d6d6dU, 0x8d8d8d8dU, 0xd5d5d5d5U, 0x4e4e4e4eU, 0xa9a9a9a9U, 0x6c6c6c6cU, 0x56565656U, 0xf4f4f4f4U, 0xeaeaeaeaU, 0x65656565U, 0x7a7a7a7aU, 0xaeaeaeaeU, 0x08080808U, 0xbabababaU, 0x78787878U, 0x25252525U, 0x2e2e2e2eU, 0x1c1c1c1cU, 0xa6a6a6a6U, 0xb4b4b4b4U, 0xc6c6c6c6U, 0xe8e8e8e8U, 0xddddddddU, 0x74747474U, 0x1f1f1f1fU, 0x4b4b4b4bU, 0xbdbdbdbdU, 0x8b8b8b8bU, 0x8a8a8a8aU, 0x70707070U, 0x3e3e3e3eU, 0xb5b5b5b5U, 0x66666666U, 0x48484848U, 0x03030303U, 0xf6f6f6f6U, 0x0e0e0e0eU, 0x61616161U, 0x35353535U, 0x57575757U, 0xb9b9b9b9U, 0x86868686U, 0xc1c1c1c1U, 0x1d1d1d1dU, 0x9e9e9e9eU, 0xe1e1e1e1U, 0xf8f8f8f8U, 0x98989898U, 0x11111111U, 0x69696969U, 0xd9d9d9d9U, 0x8e8e8e8eU, 0x94949494U, 0x9b9b9b9bU, 0x1e1e1e1eU, 0x87878787U, 0xe9e9e9e9U, 0xcecececeU, 0x55555555U, 0x28282828U, 0xdfdfdfdfU, 0x8c8c8c8cU, 0xa1a1a1a1U, 0x89898989U, 0x0d0d0d0dU, 0xbfbfbfbfU, 0xe6e6e6e6U, 0x42424242U, 0x68686868U, 0x41414141U, 0x99999999U, 0x2d2d2d2dU, 0x0f0f0f0fU, 0xb0b0b0b0U, 0x54545454U, 0xbbbbbbbbU, 0x16161616U };

            //                    internal static uint[] Td0 = { 0x51f4a750U, 0x7e416553U, 0x1a17a4c3U, 0x3a275e96U, 0x3bab6bcbU, 0x1f9d45f1U, 0xacfa58abU, 0x4be30393U, 0x2030fa55U, 0xad766df6U, 0x88cc7691U, 0xf5024c25U, 0x4fe5d7fcU, 0xc52acbd7U, 0x26354480U, 0xb562a38fU, 0xdeb15a49U, 0x25ba1b67U, 0x45ea0e98U, 0x5dfec0e1U, 0xc32f7502U, 0x814cf012U, 0x8d4697a3U, 0x6bd3f9c6U, 0x038f5fe7U, 0x15929c95U, 0xbf6d7aebU, 0x955259daU, 0xd4be832dU, 0x587421d3U, 0x49e06929U, 0x8ec9c844U, 0x75c2896aU, 0xf48e7978U, 0x99583e6bU, 0x27b971ddU, 0xbee14fb6U, 0xf088ad17U, 0xc920ac66U, 0x7dce3ab4U, 0x63df4a18U, 0xe51a3182U, 0x97513360U, 0x62537f45U, 0xb16477e0U, 0xbb6bae84U, 0xfe81a01cU, 0xf9082b94U, 0x70486858U, 0x8f45fd19U, 0x94de6c87U, 0x527bf8b7U, 0xab73d323U, 0x724b02e2U, 0xe31f8f57U, 0x6655ab2aU, 0xb2eb2807U, 0x2fb5c203U, 0x86c57b9aU, 0xd33708a5U, 0x302887f2U, 0x23bfa5b2U, 0x02036abaU, 0xed16825cU, 0x8acf1c2bU, 0xa779b492U, 0xf307f2f0U, 0x4e69e2a1U, 0x65daf4cdU, 0x0605bed5U, 0xd134621fU, 0xc4a6fe8aU, 0x342e539dU, 0xa2f355a0U, 0x058ae132U, 0xa4f6eb75U, 0x0b83ec39U, 0x4060efaaU, 0x5e719f06U, 0xbd6e1051U, 0x3e218af9U, 0x96dd063dU, 0xdd3e05aeU, 0x4de6bd46U, 0x91548db5U, 0x71c45d05U, 0x0406d46fU, 0x605015ffU, 0x1998fb24U, 0xd6bde997U, 0x894043ccU, 0x67d99e77U, 0xb0e842bdU, 0x07898b88U, 0xe7195b38U, 0x79c8eedbU, 0xa17c0a47U, 0x7c420fe9U, 0xf8841ec9U, 0x00000000U, 0x09808683U, 0x322bed48U, 0x1e1170acU, 0x6c5a724eU, 0xfd0efffbU, 0x0f853856U, 0x3daed51eU, 0x362d3927U, 0x0a0fd964U, 0x685ca621U, 0x9b5b54d1U, 0x24362e3aU, 0x0c0a67b1U, 0x9357e70fU, 0xb4ee96d2U, 0x1b9b919eU, 0x80c0c54fU, 0x61dc20a2U, 0x5a774b69U, 0x1c121a16U, 0xe293ba0aU, 0xc0a02ae5U, 0x3c22e043U, 0x121b171dU, 0x0e090d0bU, 0xf28bc7adU, 0x2db6a8b9U, 0x141ea9c8U, 0x57f11985U, 0xaf75074cU, 0xee99ddbbU, 0xa37f60fdU, 0xf701269fU, 0x5c72f5bcU, 0x44663bc5U, 0x5bfb7e34U, 0x8b432976U, 0xcb23c6dcU, 0xb6edfc68U, 0xb8e4f163U, 0xd731dccaU, 0x42638510U, 0x13972240U, 0x84c61120U, 0x854a247dU, 0xd2bb3df8U, 0xaef93211U, 0xc729a16dU, 0x1d9e2f4bU, 0xdcb230f3U, 0x0d8652ecU, 0x77c1e3d0U, 0x2bb3166cU, 0xa970b999U, 0x119448faU, 0x47e96422U, 0xa8fc8cc4U, 0xa0f03f1aU, 0x567d2cd8U, 0x223390efU, 0x87494ec7U, 0xd938d1c1U, 0x8ccaa2feU, 0x98d40b36U, 0xa6f581cfU, 0xa57ade28U, 0xdab78e26U, 0x3fadbfa4U, 0x2c3a9de4U, 0x5078920dU, 0x6a5fcc9bU, 0x547e4662U, 0xf68d13c2U, 0x90d8b8e8U, 0x2e39f75eU, 0x82c3aff5U, 0x9f5d80beU, 0x69d0937cU, 0x6fd52da9U, 0xcf2512b3U, 0xc8ac993bU, 0x10187da7U, 0xe89c636eU, 0xdb3bbb7bU, 0xcd267809U, 0x6e5918f4U, 0xec9ab701U, 0x834f9aa8U, 0xe6956e65U, 0xaaffe67eU, 0x21bccf08U, 0xef15e8e6U, 0xbae79bd9U, 0x4a6f36ceU, 0xea9f09d4U, 0x29b07cd6U, 0x31a4b2afU, 0x2a3f2331U, 0xc6a59430U, 0x35a266c0U, 0x744ebc37U, 0xfc82caa6U, 0xe090d0b0U, 0x33a7d815U, 0xf104984aU, 0x41ecdaf7U, 0x7fcd500eU, 0x1791f62fU, 0x764dd68dU, 0x43efb04dU, 0xccaa4d54U, 0xe49604dfU, 0x9ed1b5e3U, 0x4c6a881bU, 0xc12c1fb8U, 0x4665517fU, 0x9d5eea04U, 0x018c355dU, 0xfa877473U, 0xfb0b412eU, 0xb3671d5aU, 0x92dbd252U, 0xe9105633U, 0x6dd64713U, 0x9ad7618cU, 0x37a10c7aU, 0x59f8148eU, 0xeb133c89U, 0xcea927eeU, 0xb761c935U, 0xe11ce5edU, 0x7a47b13cU, 0x9cd2df59U, 0x55f2733fU, 0x1814ce79U, 0x73c737bfU, 0x53f7cdeaU, 0x5ffdaa5bU, 0xdf3d6f14U, 0x7844db86U, 0xcaaff381U, 0xb968c43eU, 0x3824342cU, 0xc2a3405fU, 0x161dc372U, 0xbce2250cU, 0x283c498bU, 0xff0d9541U, 0x39a80171U, 0x080cb3deU, 0xd8b4e49cU, 0x6456c190U, 0x7bcb8461U, 0xd532b670U, 0x486c5c74U, 0xd0b85742U };

            //                    internal static uint[] Td1 = { 0x5051f4a7U, 0x537e4165U, 0xc31a17a4U, 0x963a275eU, 0xcb3bab6bU, 0xf11f9d45U, 0xabacfa58U, 0x934be303U, 0x552030faU, 0xf6ad766dU, 0x9188cc76U, 0x25f5024cU, 0xfc4fe5d7U, 0xd7c52acbU, 0x80263544U, 0x8fb562a3U, 0x49deb15aU, 0x6725ba1bU, 0x9845ea0eU, 0xe15dfec0U, 0x02c32f75U, 0x12814cf0U, 0xa38d4697U, 0xc66bd3f9U, 0xe7038f5fU, 0x9515929cU, 0xebbf6d7aU, 0xda955259U, 0x2dd4be83U, 0xd3587421U, 0x2949e069U, 0x448ec9c8U, 0x6a75c289U, 0x78f48e79U, 0x6b99583eU, 0xdd27b971U, 0xb6bee14fU, 0x17f088adU, 0x66c920acU, 0xb47dce3aU, 0x1863df4aU, 0x82e51a31U, 0x60975133U, 0x4562537fU, 0xe0b16477U, 0x84bb6baeU, 0x1cfe81a0U, 0x94f9082bU, 0x58704868U, 0x198f45fdU, 0x8794de6cU, 0xb7527bf8U, 0x23ab73d3U, 0xe2724b02U, 0x57e31f8fU, 0x2a6655abU, 0x07b2eb28U, 0x032fb5c2U, 0x9a86c57bU, 0xa5d33708U, 0xf2302887U, 0xb223bfa5U, 0xba02036aU, 0x5ced1682U, 0x2b8acf1cU, 0x92a779b4U, 0xf0f307f2U, 0xa14e69e2U, 0xcd65daf4U, 0xd50605beU, 0x1fd13462U, 0x8ac4a6feU, 0x9d342e53U, 0xa0a2f355U, 0x32058ae1U, 0x75a4f6ebU, 0x390b83ecU, 0xaa4060efU, 0x065e719fU, 0x51bd6e10U, 0xf93e218aU, 0x3d96dd06U, 0xaedd3e05U, 0x464de6bdU, 0xb591548dU, 0x0571c45dU, 0x6f0406d4U, 0xff605015U, 0x241998fbU, 0x97d6bde9U, 0xcc894043U, 0x7767d99eU, 0xbdb0e842U, 0x8807898bU, 0x38e7195bU, 0xdb79c8eeU, 0x47a17c0aU, 0xe97c420fU, 0xc9f8841eU, 0x00000000U, 0x83098086U, 0x48322bedU, 0xac1e1170U, 0x4e6c5a72U, 0xfbfd0effU, 0x560f8538U, 0x1e3daed5U, 0x27362d39U, 0x640a0fd9U, 0x21685ca6U, 0xd19b5b54U, 0x3a24362eU, 0xb10c0a67U, 0x0f9357e7U, 0xd2b4ee96U, 0x9e1b9b91U, 0x4f80c0c5U, 0xa261dc20U, 0x695a774bU, 0x161c121aU, 0x0ae293baU, 0xe5c0a02aU, 0x433c22e0U, 0x1d121b17U, 0x0b0e090dU, 0xadf28bc7U, 0xb92db6a8U, 0xc8141ea9U, 0x8557f119U, 0x4caf7507U, 0xbbee99ddU, 0xfda37f60U, 0x9ff70126U, 0xbc5c72f5U, 0xc544663bU, 0x345bfb7eU, 0x768b4329U, 0xdccb23c6U, 0x68b6edfcU, 0x63b8e4f1U, 0xcad731dcU, 0x10426385U, 0x40139722U, 0x2084c611U, 0x7d854a24U, 0xf8d2bb3dU, 0x11aef932U, 0x6dc729a1U, 0x4b1d9e2fU, 0xf3dcb230U, 0xec0d8652U, 0xd077c1e3U, 0x6c2bb316U, 0x99a970b9U, 0xfa119448U, 0x2247e964U, 0xc4a8fc8cU, 0x1aa0f03fU, 0xd8567d2cU, 0xef223390U, 0xc787494eU, 0xc1d938d1U, 0xfe8ccaa2U, 0x3698d40bU, 0xcfa6f581U, 0x28a57adeU, 0x26dab78eU, 0xa43fadbfU, 0xe42c3a9dU, 0x0d507892U, 0x9b6a5fccU, 0x62547e46U, 0xc2f68d13U, 0xe890d8b8U, 0x5e2e39f7U, 0xf582c3afU, 0xbe9f5d80U, 0x7c69d093U, 0xa96fd52dU, 0xb3cf2512U, 0x3bc8ac99U, 0xa710187dU, 0x6ee89c63U, 0x7bdb3bbbU, 0x09cd2678U, 0xf46e5918U, 0x01ec9ab7U, 0xa8834f9aU, 0x65e6956eU, 0x7eaaffe6U, 0x0821bccfU, 0xe6ef15e8U, 0xd9bae79bU, 0xce4a6f36U, 0xd4ea9f09U, 0xd629b07cU, 0xaf31a4b2U, 0x312a3f23U, 0x30c6a594U, 0xc035a266U, 0x37744ebcU, 0xa6fc82caU, 0xb0e090d0U, 0x1533a7d8U, 0x4af10498U, 0xf741ecdaU, 0x0e7fcd50U, 0x2f1791f6U, 0x8d764dd6U, 0x4d43efb0U, 0x54ccaa4dU, 0xdfe49604U, 0xe39ed1b5U, 0x1b4c6a88U, 0xb8c12c1fU, 0x7f466551U, 0x049d5eeaU, 0x5d018c35U, 0x73fa8774U, 0x2efb0b41U, 0x5ab3671dU, 0x5292dbd2U, 0x33e91056U, 0x136dd647U, 0x8c9ad761U, 0x7a37a10cU, 0x8e59f814U, 0x89eb133cU, 0xeecea927U, 0x35b761c9U, 0xede11ce5U, 0x3c7a47b1U, 0x599cd2dfU, 0x3f55f273U, 0x791814ceU, 0xbf73c737U, 0xea53f7cdU, 0x5b5ffdaaU, 0x14df3d6fU, 0x867844dbU, 0x81caaff3U, 0x3eb968c4U, 0x2c382434U, 0x5fc2a340U, 0x72161dc3U, 0x0cbce225U, 0x8b283c49U, 0x41ff0d95U, 0x7139a801U, 0xde080cb3U, 0x9cd8b4e4U, 0x906456c1U, 0x617bcb84U, 0x70d532b6U, 0x74486c5cU, 0x42d0b857U };

            //                    internal static uint[] Td2 = { 0xa75051f4U, 0x65537e41U, 0xa4c31a17U, 0x5e963a27U, 0x6bcb3babU, 0x45f11f9dU, 0x58abacfaU, 0x03934be3U, 0xfa552030U, 0x6df6ad76U, 0x769188ccU, 0x4c25f502U, 0xd7fc4fe5U, 0xcbd7c52aU, 0x44802635U, 0xa38fb562U, 0x5a49deb1U, 0x1b6725baU, 0x0e9845eaU, 0xc0e15dfeU, 0x7502c32fU, 0xf012814cU, 0x97a38d46U, 0xf9c66bd3U, 0x5fe7038fU, 0x9c951592U, 0x7aebbf6dU, 0x59da9552U, 0x832dd4beU, 0x21d35874U, 0x692949e0U, 0xc8448ec9U, 0x896a75c2U, 0x7978f48eU, 0x3e6b9958U, 0x71dd27b9U, 0x4fb6bee1U, 0xad17f088U, 0xac66c920U, 0x3ab47dceU, 0x4a1863dfU, 0x3182e51aU, 0x33609751U, 0x7f456253U, 0x77e0b164U, 0xae84bb6bU, 0xa01cfe81U, 0x2b94f908U, 0x68587048U, 0xfd198f45U, 0x6c8794deU, 0xf8b7527bU, 0xd323ab73U, 0x02e2724bU, 0x8f57e31fU, 0xab2a6655U, 0x2807b2ebU, 0xc2032fb5U, 0x7b9a86c5U, 0x08a5d337U, 0x87f23028U, 0xa5b223bfU, 0x6aba0203U, 0x825ced16U, 0x1c2b8acfU, 0xb492a779U, 0xf2f0f307U, 0xe2a14e69U, 0xf4cd65daU, 0xbed50605U, 0x621fd134U, 0xfe8ac4a6U, 0x539d342eU, 0x55a0a2f3U, 0xe132058aU, 0xeb75a4f6U, 0xec390b83U, 0xefaa4060U, 0x9f065e71U, 0x1051bd6eU, 0x8af93e21U, 0x063d96ddU, 0x05aedd3eU, 0xbd464de6U, 0x8db59154U, 0x5d0571c4U, 0xd46f0406U, 0x15ff6050U, 0xfb241998U, 0xe997d6bdU, 0x43cc8940U, 0x9e7767d9U, 0x42bdb0e8U, 0x8b880789U, 0x5b38e719U, 0xeedb79c8U, 0x0a47a17cU, 0x0fe97c42U, 0x1ec9f884U, 0x00000000U, 0x86830980U, 0xed48322bU, 0x70ac1e11U, 0x724e6c5aU, 0xfffbfd0eU, 0x38560f85U, 0xd51e3daeU, 0x3927362dU, 0xd9640a0fU, 0xa621685cU, 0x54d19b5bU, 0x2e3a2436U, 0x67b10c0aU, 0xe70f9357U, 0x96d2b4eeU, 0x919e1b9bU, 0xc54f80c0U, 0x20a261dcU, 0x4b695a77U, 0x1a161c12U, 0xba0ae293U, 0x2ae5c0a0U, 0xe0433c22U, 0x171d121bU, 0x0d0b0e09U, 0xc7adf28bU, 0xa8b92db6U, 0xa9c8141eU, 0x198557f1U, 0x074caf75U, 0xddbbee99U, 0x60fda37fU, 0x269ff701U, 0xf5bc5c72U, 0x3bc54466U, 0x7e345bfbU, 0x29768b43U, 0xc6dccb23U, 0xfc68b6edU, 0xf163b8e4U, 0xdccad731U, 0x85104263U, 0x22401397U, 0x112084c6U, 0x247d854aU, 0x3df8d2bbU, 0x3211aef9U, 0xa16dc729U, 0x2f4b1d9eU, 0x30f3dcb2U, 0x52ec0d86U, 0xe3d077c1U, 0x166c2bb3U, 0xb999a970U, 0x48fa1194U, 0x642247e9U, 0x8cc4a8fcU, 0x3f1aa0f0U, 0x2cd8567dU, 0x90ef2233U, 0x4ec78749U, 0xd1c1d938U, 0xa2fe8ccaU, 0x0b3698d4U, 0x81cfa6f5U, 0xde28a57aU, 0x8e26dab7U, 0xbfa43fadU, 0x9de42c3aU, 0x920d5078U, 0xcc9b6a5fU, 0x4662547eU, 0x13c2f68dU, 0xb8e890d8U, 0xf75e2e39U, 0xaff582c3U, 0x80be9f5dU, 0x937c69d0U, 0x2da96fd5U, 0x12b3cf25U, 0x993bc8acU, 0x7da71018U, 0x636ee89cU, 0xbb7bdb3bU, 0x7809cd26U, 0x18f46e59U, 0xb701ec9aU, 0x9aa8834fU, 0x6e65e695U, 0xe67eaaffU, 0xcf0821bcU, 0xe8e6ef15U, 0x9bd9bae7U, 0x36ce4a6fU, 0x09d4ea9fU, 0x7cd629b0U, 0xb2af31a4U, 0x23312a3fU, 0x9430c6a5U, 0x66c035a2U, 0xbc37744eU, 0xcaa6fc82U, 0xd0b0e090U, 0xd81533a7U, 0x984af104U, 0xdaf741ecU, 0x500e7fcdU, 0xf62f1791U, 0xd68d764dU, 0xb04d43efU, 0x4d54ccaaU, 0x04dfe496U, 0xb5e39ed1U, 0x881b4c6aU, 0x1fb8c12cU, 0x517f4665U, 0xea049d5eU, 0x355d018cU, 0x7473fa87U, 0x412efb0bU, 0x1d5ab367U, 0xd25292dbU, 0x5633e910U, 0x47136dd6U, 0x618c9ad7U, 0x0c7a37a1U, 0x148e59f8U, 0x3c89eb13U, 0x27eecea9U, 0xc935b761U, 0xe5ede11cU, 0xb13c7a47U, 0xdf599cd2U, 0x733f55f2U, 0xce791814U, 0x37bf73c7U, 0xcdea53f7U, 0xaa5b5ffdU, 0x6f14df3dU, 0xdb867844U, 0xf381caafU, 0xc43eb968U, 0x342c3824U, 0x405fc2a3U, 0xc372161dU, 0x250cbce2U, 0x498b283cU, 0x9541ff0dU, 0x017139a8U, 0xb3de080cU, 0xe49cd8b4U, 0xc1906456U, 0x84617bcbU, 0xb670d532U, 0x5c74486cU, 0x5742d0b8U };

            //                    internal static uint[] Td3 = { 0xf4a75051U, 0x4165537eU, 0x17a4c31aU, 0x275e963aU, 0xab6bcb3bU, 0x9d45f11fU, 0xfa58abacU, 0xe303934bU, 0x30fa5520U, 0x766df6adU, 0xcc769188U, 0x024c25f5U, 0xe5d7fc4fU, 0x2acbd7c5U, 0x35448026U, 0x62a38fb5U, 0xb15a49deU, 0xba1b6725U, 0xea0e9845U, 0xfec0e15dU, 0x2f7502c3U, 0x4cf01281U, 0x4697a38dU, 0xd3f9c66bU, 0x8f5fe703U, 0x929c9515U, 0x6d7aebbfU, 0x5259da95U, 0xbe832dd4U, 0x7421d358U, 0xe0692949U, 0xc9c8448eU, 0xc2896a75U, 0x8e7978f4U, 0x583e6b99U, 0xb971dd27U, 0xe14fb6beU, 0x88ad17f0U, 0x20ac66c9U, 0xce3ab47dU, 0xdf4a1863U, 0x1a3182e5U, 0x51336097U, 0x537f4562U, 0x6477e0b1U, 0x6bae84bbU, 0x81a01cfeU, 0x082b94f9U, 0x48685870U, 0x45fd198fU, 0xde6c8794U, 0x7bf8b752U, 0x73d323abU, 0x4b02e272U, 0x1f8f57e3U, 0x55ab2a66U, 0xeb2807b2U, 0xb5c2032fU, 0xc57b9a86U, 0x3708a5d3U, 0x2887f230U, 0xbfa5b223U, 0x036aba02U, 0x16825cedU, 0xcf1c2b8aU, 0x79b492a7U, 0x07f2f0f3U, 0x69e2a14eU, 0xdaf4cd65U, 0x05bed506U, 0x34621fd1U, 0xa6fe8ac4U, 0x2e539d34U, 0xf355a0a2U, 0x8ae13205U, 0xf6eb75a4U, 0x83ec390bU, 0x60efaa40U, 0x719f065eU, 0x6e1051bdU, 0x218af93eU, 0xdd063d96U, 0x3e05aeddU, 0xe6bd464dU, 0x548db591U, 0xc45d0571U, 0x06d46f04U, 0x5015ff60U, 0x98fb2419U, 0xbde997d6U, 0x4043cc89U, 0xd99e7767U, 0xe842bdb0U, 0x898b8807U, 0x195b38e7U, 0xc8eedb79U, 0x7c0a47a1U, 0x420fe97cU, 0x841ec9f8U, 0x00000000U, 0x80868309U, 0x2bed4832U, 0x1170ac1eU, 0x5a724e6cU, 0x0efffbfdU, 0x8538560fU, 0xaed51e3dU, 0x2d392736U, 0x0fd9640aU, 0x5ca62168U, 0x5b54d19bU, 0x362e3a24U, 0x0a67b10cU, 0x57e70f93U, 0xee96d2b4U, 0x9b919e1bU, 0xc0c54f80U, 0xdc20a261U, 0x774b695aU, 0x121a161cU, 0x93ba0ae2U, 0xa02ae5c0U, 0x22e0433cU, 0x1b171d12U, 0x090d0b0eU, 0x8bc7adf2U, 0xb6a8b92dU, 0x1ea9c814U, 0xf1198557U, 0x75074cafU, 0x99ddbbeeU, 0x7f60fda3U, 0x01269ff7U, 0x72f5bc5cU, 0x663bc544U, 0xfb7e345bU, 0x4329768bU, 0x23c6dccbU, 0xedfc68b6U, 0xe4f163b8U, 0x31dccad7U, 0x63851042U, 0x97224013U, 0xc6112084U, 0x4a247d85U, 0xbb3df8d2U, 0xf93211aeU, 0x29a16dc7U, 0x9e2f4b1dU, 0xb230f3dcU, 0x8652ec0dU, 0xc1e3d077U, 0xb3166c2bU, 0x70b999a9U, 0x9448fa11U, 0xe9642247U, 0xfc8cc4a8U, 0xf03f1aa0U, 0x7d2cd856U, 0x3390ef22U, 0x494ec787U, 0x38d1c1d9U, 0xcaa2fe8cU, 0xd40b3698U, 0xf581cfa6U, 0x7ade28a5U, 0xb78e26daU, 0xadbfa43fU, 0x3a9de42cU, 0x78920d50U, 0x5fcc9b6aU, 0x7e466254U, 0x8d13c2f6U, 0xd8b8e890U, 0x39f75e2eU, 0xc3aff582U, 0x5d80be9fU, 0xd0937c69U, 0xd52da96fU, 0x2512b3cfU, 0xac993bc8U, 0x187da710U, 0x9c636ee8U, 0x3bbb7bdbU, 0x267809cdU, 0x5918f46eU, 0x9ab701ecU, 0x4f9aa883U, 0x956e65e6U, 0xffe67eaaU, 0xbccf0821U, 0x15e8e6efU, 0xe79bd9baU, 0x6f36ce4aU, 0x9f09d4eaU, 0xb07cd629U, 0xa4b2af31U, 0x3f23312aU, 0xa59430c6U, 0xa266c035U, 0x4ebc3774U, 0x82caa6fcU, 0x90d0b0e0U, 0xa7d81533U, 0x04984af1U, 0xecdaf741U, 0xcd500e7fU, 0x91f62f17U, 0x4dd68d76U, 0xefb04d43U, 0xaa4d54ccU, 0x9604dfe4U, 0xd1b5e39eU, 0x6a881b4cU, 0x2c1fb8c1U, 0x65517f46U, 0x5eea049dU, 0x8c355d01U, 0x877473faU, 0x0b412efbU, 0x671d5ab3U, 0xdbd25292U, 0x105633e9U, 0xd647136dU, 0xd7618c9aU, 0xa10c7a37U, 0xf8148e59U, 0x133c89ebU, 0xa927eeceU, 0x61c935b7U, 0x1ce5ede1U, 0x47b13c7aU, 0xd2df599cU, 0xf2733f55U, 0x14ce7918U, 0xc737bf73U, 0xf7cdea53U, 0xfdaa5b5fU, 0x3d6f14dfU, 0x44db8678U, 0xaff381caU, 0x68c43eb9U, 0x24342c38U, 0xa3405fc2U, 0x1dc37216U, 0xe2250cbcU, 0x3c498b28U, 0x0d9541ffU, 0xa8017139U, 0x0cb3de08U, 0xb4e49cd8U, 0x56c19064U, 0xcb84617bU, 0x32b670d5U, 0x6c5c7448U, 0xb85742d0U };

            //                    internal static uint[] Td4 = { 0x52525252U, 0x09090909U, 0x6a6a6a6aU, 0xd5d5d5d5U, 0x30303030U, 0x36363636U, 0xa5a5a5a5U, 0x38383838U, 0xbfbfbfbfU, 0x40404040U, 0xa3a3a3a3U, 0x9e9e9e9eU, 0x81818181U, 0xf3f3f3f3U, 0xd7d7d7d7U, 0xfbfbfbfbU, 0x7c7c7c7cU, 0xe3e3e3e3U, 0x39393939U, 0x82828282U, 0x9b9b9b9bU, 0x2f2f2f2fU, 0xffffffffU, 0x87878787U, 0x34343434U, 0x8e8e8e8eU, 0x43434343U, 0x44444444U, 0xc4c4c4c4U, 0xdedededeU, 0xe9e9e9e9U, 0xcbcbcbcbU, 0x54545454U, 0x7b7b7b7bU, 0x94949494U, 0x32323232U, 0xa6a6a6a6U, 0xc2c2c2c2U, 0x23232323U, 0x3d3d3d3dU, 0xeeeeeeeeU, 0x4c4c4c4cU, 0x95959595U, 0x0b0b0b0bU, 0x42424242U, 0xfafafafaU, 0xc3c3c3c3U, 0x4e4e4e4eU, 0x08080808U, 0x2e2e2e2eU, 0xa1a1a1a1U, 0x66666666U, 0x28282828U, 0xd9d9d9d9U, 0x24242424U, 0xb2b2b2b2U, 0x76767676U, 0x5b5b5b5bU, 0xa2a2a2a2U, 0x49494949U, 0x6d6d6d6dU, 0x8b8b8b8bU, 0xd1d1d1d1U, 0x25252525U, 0x72727272U, 0xf8f8f8f8U, 0xf6f6f6f6U, 0x64646464U, 0x86868686U, 0x68686868U, 0x98989898U, 0x16161616U, 0xd4d4d4d4U, 0xa4a4a4a4U, 0x5c5c5c5cU, 0xccccccccU, 0x5d5d5d5dU, 0x65656565U, 0xb6b6b6b6U, 0x92929292U, 0x6c6c6c6cU, 0x70707070U, 0x48484848U, 0x50505050U, 0xfdfdfdfdU, 0xededededU, 0xb9b9b9b9U, 0xdadadadaU, 0x5e5e5e5eU, 0x15151515U, 0x46464646U, 0x57575757U, 0xa7a7a7a7U, 0x8d8d8d8dU, 0x9d9d9d9dU, 0x84848484U, 0x90909090U, 0xd8d8d8d8U, 0xababababU, 0x00000000U, 0x8c8c8c8cU, 0xbcbcbcbcU, 0xd3d3d3d3U, 0x0a0a0a0aU, 0xf7f7f7f7U, 0xe4e4e4e4U, 0x58585858U, 0x05050505U, 0xb8b8b8b8U, 0xb3b3b3b3U, 0x45454545U, 0x06060606U, 0xd0d0d0d0U, 0x2c2c2c2cU, 0x1e1e1e1eU, 0x8f8f8f8fU, 0xcacacacaU, 0x3f3f3f3fU, 0x0f0f0f0fU, 0x02020202U, 0xc1c1c1c1U, 0xafafafafU, 0xbdbdbdbdU, 0x03030303U, 0x01010101U, 0x13131313U, 0x8a8a8a8aU, 0x6b6b6b6bU, 0x3a3a3a3aU, 0x91919191U, 0x11111111U, 0x41414141U, 0x4f4f4f4fU, 0x67676767U, 0xdcdcdcdcU, 0xeaeaeaeaU, 0x97979797U, 0xf2f2f2f2U, 0xcfcfcfcfU, 0xcecececeU, 0xf0f0f0f0U, 0xb4b4b4b4U, 0xe6e6e6e6U, 0x73737373U, 0x96969696U, 0xacacacacU, 0x74747474U, 0x22222222U, 0xe7e7e7e7U, 0xadadadadU, 0x35353535U, 0x85858585U, 0xe2e2e2e2U, 0xf9f9f9f9U, 0x37373737U, 0xe8e8e8e8U, 0x1c1c1c1cU, 0x75757575U, 0xdfdfdfdfU, 0x6e6e6e6eU, 0x47474747U, 0xf1f1f1f1U, 0x1a1a1a1aU, 0x71717171U, 0x1d1d1d1dU, 0x29292929U, 0xc5c5c5c5U, 0x89898989U, 0x6f6f6f6fU, 0xb7b7b7b7U, 0x62626262U, 0x0e0e0e0eU, 0xaaaaaaaaU, 0x18181818U, 0xbebebebeU, 0x1b1b1b1bU, 0xfcfcfcfcU, 0x56565656U, 0x3e3e3e3eU, 0x4b4b4b4bU, 0xc6c6c6c6U, 0xd2d2d2d2U, 0x79797979U, 0x20202020U, 0x9a9a9a9aU, 0xdbdbdbdbU, 0xc0c0c0c0U, 0xfefefefeU, 0x78787878U, 0xcdcdcdcdU, 0x5a5a5a5aU, 0xf4f4f4f4U, 0x1f1f1f1fU, 0xddddddddU, 0xa8a8a8a8U, 0x33333333U, 0x88888888U, 0x07070707U, 0xc7c7c7c7U, 0x31313131U, 0xb1b1b1b1U, 0x12121212U, 0x10101010U, 0x59595959U, 0x27272727U, 0x80808080U, 0xececececU, 0x5f5f5f5fU, 0x60606060U, 0x51515151U, 0x7f7f7f7fU, 0xa9a9a9a9U, 0x19191919U, 0xb5b5b5b5U, 0x4a4a4a4aU, 0x0d0d0d0dU, 0x2d2d2d2dU, 0xe5e5e5e5U, 0x7a7a7a7aU, 0x9f9f9f9fU, 0x93939393U, 0xc9c9c9c9U, 0x9c9c9c9cU, 0xefefefefU, 0xa0a0a0a0U, 0xe0e0e0e0U, 0x3b3b3b3bU, 0x4d4d4d4dU, 0xaeaeaeaeU, 0x2a2a2a2aU, 0xf5f5f5f5U, 0xb0b0b0b0U, 0xc8c8c8c8U, 0xebebebebU, 0xbbbbbbbbU, 0x3c3c3c3cU, 0x83838383U, 0x53535353U, 0x99999999U, 0x61616161U, 0x17171717U, 0x2b2b2b2bU, 0x04040404U, 0x7e7e7e7eU, 0xbabababaU, 0x77777777U, 0xd6d6d6d6U, 0x26262626U, 0xe1e1e1e1U, 0x69696969U, 0x14141414U, 0x63636363U, 0x55555555U, 0x21212121U, 0x0c0c0c0cU, 0x7d7d7d7dU };

            //                    internal static uint[] rcon = { 0x01000000, 0x02000000, 0x04000000, 0x08000000, 0x10000000, 0x20000000, 0x40000000, 0x80000000, 0x1B000000, 0x36000000 };

            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define GETU32(pt) (((u32)(pt)[0] << 24) ^ ((u32)(pt)[1] << 16) ^ ((u32)(pt)[2] << 8) ^ ((u32)(pt)[3]))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define PUTU32(ct, st) { (ct)[0] = (u8)((st) >> 24); (ct)[1] = (u8)((st) >> 16); (ct)[2] = (u8)((st) >> 8); (ct)[3] = (u8)(st); }

            //                    internal static void rijndaelDecrypt(uint[] rk, int Nr, byte[] ct, byte[] pt)
            //                    {
            //                        uint s0;
            //                        uint s1;
            //                        uint s2;
            //                        uint s3;
            //                        uint t0;
            //                        uint t1;
            //                        uint t2;
            //                        uint t3;
            //#if !FULL_UNROLL
            //                        int r;
            //#endif

            //                        /*
            //                         * map byte array block to cipher state
            //                         * and add initial round key:
            //                         */
            //                        s0 = (((uint)(ct)[0] << 24) ^ ((uint)(ct)[1] << 16) ^ ((uint)(ct)[2] << 8) ^ ((uint)(ct)[3])) ^ rk[0];
            //                        s1 = (((uint)(ct + 4)[0] << 24) ^ ((uint)(ct + 4)[1] << 16) ^ ((uint)(ct + 4)[2] << 8) ^ ((uint)(ct + 4)[3])) ^ rk[1];
            //                        s2 = (((uint)(ct + 8)[0] << 24) ^ ((uint)(ct + 8)[1] << 16) ^ ((uint)(ct + 8)[2] << 8) ^ ((uint)(ct + 8)[3])) ^ rk[2];
            //                        s3 = (((uint)(ct + 12)[0] << 24) ^ ((uint)(ct + 12)[1] << 16) ^ ((uint)(ct + 12)[2] << 8) ^ ((uint)(ct + 12)[3])) ^ rk[3];
            //#if FULL_UNROLL
            //		/* round 1: */
            //		t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[4];
            //		t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[5];
            //		t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[6];
            //		t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[7];
            //		/* round 2: */
            //		s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >> 8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[8];
            //		s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >> 8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[9];
            //		s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >> 8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[10];
            //		s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >> 8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[11];
            //		/* round 3: */
            //		t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[12];
            //		t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[13];
            //		t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[14];
            //		t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[15];
            //		/* round 4: */
            //		s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >> 8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[16];
            //		s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >> 8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[17];
            //		s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >> 8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[18];
            //		s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >> 8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[19];
            //		/* round 5: */
            //		t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[20];
            //		t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[21];
            //		t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[22];
            //		t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[23];
            //		/* round 6: */
            //		s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >> 8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[24];
            //		s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >> 8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[25];
            //		s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >> 8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[26];
            //		s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >> 8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[27];
            //		/* round 7: */
            //		t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[28];
            //		t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[29];
            //		t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[30];
            //		t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[31];
            //		/* round 8: */
            //		s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >> 8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[32];
            //		s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >> 8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[33];
            //		s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >> 8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[34];
            //		s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >> 8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[35];
            //		/* round 9: */
            //		t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[36];
            //		t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[37];
            //		t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[38];
            //		t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[39];
            //		if (Nr > 10)
            //		{
            //		/* round 10: */
            //		s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >> 8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[40];
            //		s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >> 8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[41];
            //		s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >> 8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[42];
            //		s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >> 8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[43];
            //		/* round 11: */
            //		t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[44];
            //		t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[45];
            //		t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[46];
            //		t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[47];
            //		if (Nr > 12)
            //		{
            //			/* round 12: */
            //			s0 = Td0[t0 >> 24] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >> 8) & 0xff] ^ Td3[t1 & 0xff] ^ rk[48];
            //			s1 = Td0[t1 >> 24] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >> 8) & 0xff] ^ Td3[t2 & 0xff] ^ rk[49];
            //			s2 = Td0[t2 >> 24] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >> 8) & 0xff] ^ Td3[t3 & 0xff] ^ rk[50];
            //			s3 = Td0[t3 >> 24] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >> 8) & 0xff] ^ Td3[t0 & 0xff] ^ rk[51];
            //			/* round 13: */
            //			t0 = Td0[s0 >> 24] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[s1 & 0xff] ^ rk[52];
            //			t1 = Td0[s1 >> 24] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[s2 & 0xff] ^ rk[53];
            //			t2 = Td0[s2 >> 24] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[s3 & 0xff] ^ rk[54];
            //			t3 = Td0[s3 >> 24] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[s0 & 0xff] ^ rk[55];
            //		}
            //		}
            //		rk += Nr << 2;
            //#else
            //                        /*
            //                         * Nr - 1 full rounds:
            //                         */
            //                        r = Nr >> 1;
            //                        for (;;)
            //                        {
            //                            t0 = Td0[(s0 >> 24)] ^ Td1[(s3 >> 16) & 0xff] ^ Td2[(s2 >> 8) & 0xff] ^ Td3[(s1) & 0xff] ^ rk[4];
            //                            t1 = Td0[(s1 >> 24)] ^ Td1[(s0 >> 16) & 0xff] ^ Td2[(s3 >> 8) & 0xff] ^ Td3[(s2) & 0xff] ^ rk[5];
            //                            t2 = Td0[(s2 >> 24)] ^ Td1[(s1 >> 16) & 0xff] ^ Td2[(s0 >> 8) & 0xff] ^ Td3[(s3) & 0xff] ^ rk[6];
            //                            t3 = Td0[(s3 >> 24)] ^ Td1[(s2 >> 16) & 0xff] ^ Td2[(s1 >> 8) & 0xff] ^ Td3[(s0) & 0xff] ^ rk[7];

            //                            rk += 8;
            //                            if (--r == 0)
            //                            {
            //                                break;
            //                            }

            //                            s0 = Td0[(t0 >> 24)] ^ Td1[(t3 >> 16) & 0xff] ^ Td2[(t2 >> 8) & 0xff] ^ Td3[(t1) & 0xff] ^ rk[0];
            //                            s1 = Td0[(t1 >> 24)] ^ Td1[(t0 >> 16) & 0xff] ^ Td2[(t3 >> 8) & 0xff] ^ Td3[(t2) & 0xff] ^ rk[1];
            //                            s2 = Td0[(t2 >> 24)] ^ Td1[(t1 >> 16) & 0xff] ^ Td2[(t0 >> 8) & 0xff] ^ Td3[(t3) & 0xff] ^ rk[2];
            //                            s3 = Td0[(t3 >> 24)] ^ Td1[(t2 >> 16) & 0xff] ^ Td2[(t1 >> 8) & 0xff] ^ Td3[(t0) & 0xff] ^ rk[3];
            //                        }
            //#endif
            //                        /*
            //                         * apply last round and
            //                         * map cipher state to byte array block:
            //                         */
            //                        s0 = (Td4[(t0 >> 24)] & 0xff000000) ^ (Td4[(t3 >> 16) & 0xff] & 0x00ff0000) ^ (Td4[(t2 >> 8) & 0xff] & 0x0000ff00) ^ (Td4[(t1) & 0xff] & 0x000000ff) ^ rk[0];
            //                        {
            //                            (pt)[0] = (byte)((s0) >> 24);
            //                            (pt)[1] = (byte)((s0) >> 16);
            //                            (pt)[2] = (byte)((s0) >> 8);
            //                            (pt)[3] = (byte)(s0);
            //                        };
            //                        s1 = (Td4[(t1 >> 24)] & 0xff000000) ^ (Td4[(t0 >> 16) & 0xff] & 0x00ff0000) ^ (Td4[(t3 >> 8) & 0xff] & 0x0000ff00) ^ (Td4[(t2) & 0xff] & 0x000000ff) ^ rk[1];
            //                        {
            //                            (pt + 4)[0] = (byte)((s1) >> 24);
            //                            (pt + 4)[1] = (byte)((s1) >> 16);
            //                            (pt + 4)[2] = (byte)((s1) >> 8);
            //                            (pt + 4)[3] = (byte)(s1);
            //                        };
            //                        s2 = (Td4[(t2 >> 24)] & 0xff000000) ^ (Td4[(t1 >> 16) & 0xff] & 0x00ff0000) ^ (Td4[(t0 >> 8) & 0xff] & 0x0000ff00) ^ (Td4[(t3) & 0xff] & 0x000000ff) ^ rk[2];
            //                        {
            //                            (pt + 8)[0] = (byte)((s2) >> 24);
            //                            (pt + 8)[1] = (byte)((s2) >> 16);
            //                            (pt + 8)[2] = (byte)((s2) >> 8);
            //                            (pt + 8)[3] = (byte)(s2);
            //                        };
            //                        s3 = (Td4[(t3 >> 24)] & 0xff000000) ^ (Td4[(t2 >> 16) & 0xff] & 0x00ff0000) ^ (Td4[(t1 >> 8) & 0xff] & 0x0000ff00) ^ (Td4[(t0) & 0xff] & 0x000000ff) ^ rk[3];
            //                        {
            //                            (pt + 12)[0] = (byte)((s3) >> 24);
            //                            (pt + 12)[1] = (byte)((s3) >> 16);
            //                            (pt + 12)[2] = (byte)((s3) >> 8);
            //                            (pt + 12)[3] = (byte)(s3);
            //                        };
            //                    }

            //                    public static void xor_128(byte[] a, byte[] b, byte[] @out)
            //                    {
            //                        int i;
            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            @out[i] = a[i] ^ b[i];
            //                        }
            //                    }

            //                    /* AES-CMAC Generation Function */
            //                    public static void leftshift_onebit(byte[] input, byte[] output)
            //                    {
            //                        int i;
            //                        byte overflow = 0;

            //                        for (i = 15; i >= 0; i--)
            //                        {
            //                            output[i] = input[i] << 1;
            //                            output[i] |= overflow;
            //                            overflow = (input[i] & 0x80) ? 1 : 0;
            //                        }
            //                    }

            //                    public static void generate_subkey(AesCtx ctx, byte[] K1, ref byte K2)
            //                    {
            //                        byte[] L = new byte[16];
            //                        byte[] Z = new byte[16];
            //                        byte[] tmp = new byte[16];
            //                        int i;

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            Z[i] = 0;
            //                        }


            //                        AES_encrypt(ctx, Z, ref L);

            //                        if ((L[0] & 0x80) == 0) // If MSB(L) = 0, then K1 = L << 1
            //                        {
            //                            leftshift_onebit(L, K1);
            //                        }
            //                        else
            //                        { // Else K1 = ( L << 1 ) (+) Rb
            //                            leftshift_onebit(L, tmp);
            //                            xor_128(tmp, const_Rb, K1);
            //                        }

            //                        if ((K1[0] & 0x80) == 0)
            //                        {
            //                            leftshift_onebit(K1, K2);
            //                        }
            //                        else
            //                        {
            //                            leftshift_onebit(K1, tmp);
            //                            xor_128(tmp, const_Rb, K2);
            //                        }
            //                    }

            //                    public static void padding(byte[] lastb, byte[] pad, int length)
            //                    {
            //                        int j;

            //                        /* original last block */
            //                        for (j = 0; j < 16; j++)
            //                        {
            //                            if (j < length)
            //                            {
            //                                pad[j] = lastb[j];
            //                            }
            //                            else if (j == length)
            //                            {
            //                                pad[j] = 0x80;
            //                            }
            //                            else
            //                            {
            //                                pad[j] = 0x00;
            //                            }
            //                        }
            //                    }


            //                    /*
            //                        BBMac functions.
            //                    */

            //                    public static int sceDrmBBMacInit(MAC_KEY mkey, int type)
            //                    {
            //                        mkey.type = type;
            //                        mkey.pad_size = 0;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(mkey.key, 0, 16);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(mkey.pad, 0, 16);

            //                        return 0;
            //                    }
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'buf', so pointers on this parameter are left unchanged:
            //                    public static int sceDrmBBMacUpdate(MAC_KEY mkey, byte buf, int size)
            //                    {
            //                        int retv = 0;
            //                        int ksize;
            //                        int p;
            //                        int type;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *kbuf;
            //                        byte kbuf;

            //                        if (mkey.pad_size > 16)
            //                        {
            //                            retv = 0x80510302;
            //                            goto _exit;
            //                        }

            //                        if (mkey.pad_size + size <= 16)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(mkey.pad + mkey.pad_size, buf, size);
            //                            mkey.pad_size += size;
            //                            retv = 0;
            //                        }
            //                        else
            //                        {
            //                            kbuf = kirk_buf + 0x14;
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf, mkey.pad, mkey.pad_size);

            //                            p = mkey.pad_size;

            //                            mkey.pad_size += size;
            //                            mkey.pad_size &= 0x0f;
            //                            if (mkey.pad_size == 0)
            //                            {
            //                                mkey.pad_size = 16;
            //                            }

            //                            size -= mkey.pad_size;
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(mkey.pad, buf + size, mkey.pad_size);

            //                            type = (mkey.type == 2) ? 0x3A : 0x38;

            //                            while (size != 0)
            //                            {
            //                                ksize = (size + p >= 0x0800) ? 0x0800 : size + p;
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(kbuf + p, buf, ksize - p);
            //                                retv = encrypt_buf(kirk_buf, ksize, mkey.key, type);

            //                                if (retv != 0)
            //                                {
            //                                    goto _exit;
            //                                }

            //                                size -= (ksize - p);
            //                                buf += ksize - p;
            //                                p = 0;
            //                            }
            //                        }

            //                        _exit:
            //                        return retv;

            //                    }
            //                    public static int sceDrmBBMacFinal(MAC_KEY mkey, ref byte buf, byte[] vkey)
            //                    {
            //                        int i;
            //                        int retv;
            //                        int code;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *kbuf, tmp[16], tmp1[16];
            //                        byte kbuf;
            //                        byte[] tmp = new byte[16];
            //                        byte[] tmp1 = new byte[16];
            //                        uint t0;
            //                        uint v0;
            //                        uint v1;

            //                        if (mkey.pad_size > 16)
            //                        {
            //                            return 0x80510302;
            //                        }

            //                        code = (mkey.type == 2) ? 0x3A : 0x38;
            //                        kbuf = kirk_buf + 0x14;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(kbuf, 0, 16);
            //                        retv = kirk4(ref kirk_buf, 16, code);

            //                        if (retv != 0)
            //                        {
            //                            goto _exit;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp, kbuf, 16);

            //                        t0 = ((tmp[0] & 0x80) != 0) ? 0x87 : 0;
            //                        for (i = 0; i < 15; i++)
            //                        {
            //                            v1 = tmp[i + 0];
            //                            v0 = tmp[i + 1];
            //                            v1 <<= 1;
            //                            v0 >>= 7;
            //                            v0 |= v1;
            //                            tmp[i + 0] = v0;
            //                        }
            //                        v0 = tmp[15];
            //                        v0 <<= 1;
            //                        v0 ^= t0;
            //                        tmp[15] = v0;

            //                        if (mkey.pad_size < 16)
            //                        {
            //                            t0 = ((tmp[0] & 0x80) != 0) ? 0x87 : 0;
            //                            for (i = 0; i < 15; i++)
            //                            {
            //                                v1 = tmp[i + 0];
            //                                v0 = tmp[i + 1];
            //                                v1 <<= 1;
            //                                v0 >>= 7;
            //                                v0 |= v1;
            //                                tmp[i + 0] = v0;
            //                            }
            //                            v0 = tmp[15];
            //                            v0 <<= 1;
            //                            v0 ^= t0;
            //                            tmp[15] = v0;

            //                            mkey.pad[mkey.pad_size] = 0x80;
            //                            if (mkey.pad_size + 1 < 16)
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                                memset(mkey.pad + mkey.pad_size + 1, 0, 16 - mkey.pad_size - 1);
            //                            }
            //                        }

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            mkey.pad[i] ^= tmp[i];
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf, mkey.pad, 16);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp1, mkey.key, 16);

            //                        retv = encrypt_buf(kirk_buf, 0x10, tmp1, code);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            tmp1[i] ^= amctrl_key1[i];
            //                        }

            //                        if (mkey.type == 2)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf, tmp1, 16);

            //                            retv = kirk5(ref kirk_buf, 0x10);

            //                            if (retv != 0)
            //                            {
            //                                goto _exit;
            //                            }

            //                            retv = kirk4(ref kirk_buf, 0x10, code);

            //                            if (retv != 0)
            //                            {
            //                                goto _exit;
            //                            }

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(tmp1, kbuf, 16);
            //                        }

            //                        if (vkey != 0)
            //                        {
            //                            for (i = 0; i < 0x10; i++)
            //                            {
            //                                tmp1[i] ^= vkey[i];
            //                            }
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf, tmp1, 16);

            //                            retv = kirk4(ref kirk_buf, 0x10, code);

            //                            if (retv != 0)
            //                            {
            //                                goto _exit;
            //                            }

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(tmp1, kbuf, 16);
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(buf, tmp1, 16);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(mkey.key, 0, 16);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(mkey.pad, 0, 16);

            //                        mkey.pad_size = 0;
            //                        mkey.type = 0;
            //                        retv = 0;

            //                        _exit:
            //                        return retv;
            //                    }
            //                    public static int sceDrmBBMacFinal2(MAC_KEY mkey, ref byte @out, ref byte vkey)
            //                    {
            //                        int i;
            //                        int retv;
            //                        int type;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *kbuf, tmp[16];
            //                        byte kbuf;
            //                        byte[] tmp = new byte[16];

            //                        type = mkey.type;
            //                        retv = sceDrmBBMacFinal(mkey, ref tmp, vkey);
            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        kbuf = kirk_buf + 0x14;

            //                        if (type == 3)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf, @out, 0x10);
            //                            kirk7(ref kirk_buf, 0x10, 0x63);
            //                        }
            //                        else
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kirk_buf, @out, 0x10);
            //                        }

            //                        retv = 0;
            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            if (kirk_buf[i] != tmp[i])
            //                            {
            //                                retv = 0x80510300;
            //                                break;
            //                            }
            //                        }

            //                        return retv;
            //                    }

            //                    /*
            //                        Extra functions.
            //                    */

            //                    public static int bbmac_build_final2(int type, ref byte mac)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *kbuf = kirk_buf + 0x14;
            //                        byte kbuf = kirk_buf + 0x14;

            //                        if (type == 3)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf, mac, 16);
            //                            kirk4(ref kirk_buf, 0x10, 0x63);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(mac, kbuf, 16);
            //                        }

            //                        return 0;
            //                    }
            //                    public static int bbmac_getkey(MAC_KEY mkey, ref byte bbmac, byte[] vkey)
            //                    {
            //                        int i;
            //                        int retv;
            //                        int type;
            //                        int code;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *kbuf, tmp[16], tmp1[16];
            //                        byte kbuf;
            //                        byte[] tmp = new byte[16];
            //                        byte[] tmp1 = new byte[16];

            //                        type = mkey.type;
            //                        retv = sceDrmBBMacFinal(mkey, ref tmp, null);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        kbuf = kirk_buf + 0x14;

            //                        if (type == 3)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf, bbmac, 0x10);
            //                            kirk7(ref kirk_buf, 0x10, 0x63);
            //                        }
            //                        else
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kirk_buf, bbmac, 0x10);
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp1, kirk_buf, 16);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf, tmp1, 16);

            //                        code = (type == 2) ? 0x3A : 0x38;
            //                        kirk7(ref kirk_buf, 0x10, code);

            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            vkey[i] = tmp[i] ^ kirk_buf[i];
            //                        }

            //                        return 0;
            //                    }
            //                    public static int bbmac_forge(MAC_KEY mkey, ref byte bbmac, byte[] vkey, byte[] buf)
            //                    {
            //                        int i;
            //                        int retv;
            //                        int type;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *kbuf, tmp[16], tmp1[16];
            //                        byte kbuf;
            //                        byte[] tmp = new byte[16];
            //                        byte[] tmp1 = new byte[16];
            //                        uint t0;
            //                        uint v0;
            //                        uint v1;

            //                        if (mkey.pad_size > 16)
            //                        {
            //                            return 0x80510302;
            //                        }

            //                        type = (mkey.type == 2) ? 0x3A : 0x38;
            //                        kbuf = kirk_buf + 0x14;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(kbuf, 0, 16);
            //                        retv = kirk4(ref kirk_buf, 16, type);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp, kbuf, 16);

            //                        t0 = ((tmp[0] & 0x80) != 0) ? 0x87 : 0;
            //                        for (i = 0; i < 15; i++)
            //                        {
            //                            v1 = tmp[i + 0];
            //                            v0 = tmp[i + 1];
            //                            v1 <<= 1;
            //                            v0 >>= 7;
            //                            v0 |= v1;
            //                            tmp[i + 0] = v0;
            //                        }
            //                        v0 = tmp[15];
            //                        v0 <<= 1;
            //                        v0 ^= t0;
            //                        tmp[15] = v0;

            //                        if (mkey.pad_size < 16)
            //                        {
            //                            t0 = ((tmp[0] & 0x80) != 0) ? 0x87 : 0;
            //                            for (i = 0; i < 15; i++)
            //                            {
            //                                v1 = tmp[i + 0];
            //                                v0 = tmp[i + 1];
            //                                v1 <<= 1;
            //                                v0 >>= 7;
            //                                v0 |= v1;
            //                                tmp[i + 0] = v0;
            //                            }
            //                            v0 = tmp[15];
            //                            v0 <<= 1;
            //                            v0 ^= t0;
            //                            tmp[15] = t0;

            //                            mkey.pad[mkey.pad_size] = 0x80;
            //                            if (mkey.pad_size + 1 < 16)
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                                memset(mkey.pad + mkey.pad_size + 1, 0, 16 - mkey.pad_size - 1);
            //                            }
            //                        }

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            mkey.pad[i] ^= tmp[i];
            //                        }
            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            mkey.pad[i] ^= mkey.key[i];
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf, bbmac, 0x10);
            //                        kirk7(ref kirk_buf, 0x10, 0x63);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf, kirk_buf, 0x10);
            //                        kirk7(ref kirk_buf, 0x10, type);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp1, kirk_buf, 0x10);
            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            tmp1[i] ^= vkey[i];
            //                        }
            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            tmp1[i] ^= amctrl_key1[i];
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf, tmp1, 0x10);
            //                        kirk7(ref kirk_buf, 0x10, type);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp1, kirk_buf, 0x10);
            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            mkey.pad[i] ^= tmp1[i];
            //                        }

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            buf[i] ^= mkey.pad[i];
            //                        }

            //                        return 0;
            //                    }

            //                    /*
            //                        BBCipher functions.
            //                    */

            //                    public static int sceDrmBBCipherInit(CIPHER_KEY ckey, int type, int mode, byte[] header_key, byte[] version_key, uint seed)
            //                    {
            //                        int i;
            //                        int retv;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *kbuf;
            //                        byte kbuf;

            //                        kbuf = kirk_buf + 0x14;
            //                        ckey.type = type;
            //                        if (mode == 2)
            //                        {
            //                            ckey.seed = seed + 1;
            //                            for (i = 0; i < 16; i++)
            //                            {
            //                                ckey.key[i] = header_key[i];
            //                            }
            //                            if (version_key != 0)
            //                            {
            //                                for (i = 0; i < 16; i++)
            //                                {
            //                                    ckey.key[i] ^= version_key[i];
            //                                }
            //                            }
            //                            retv = 0;
            //                        }
            //                        else if (mode == 1)
            //                        {
            //                            ckey.seed = 1;
            //                            retv = kirk14(ref kirk_buf);

            //                            if (retv != 0)
            //                            {
            //                                return retv;
            //                            }

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf, kirk_buf, 0x10);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(kbuf + 0x0c, 0, 4);

            //                            if (ckey.type == 2)
            //                            {
            //                                for (i = 0; i < 16; i++)
            //                                {
            //                                    kbuf[i] ^= amctrl_key2[i];
            //                                }
            //                                retv = kirk5(ref kirk_buf, 0x10);
            //                                for (i = 0; i < 16; i++)
            //                                {
            //                                    kbuf[i] ^= amctrl_key3[i];
            //                                }
            //                            }
            //                            else
            //                            {
            //                                for (i = 0; i < 16; i++)
            //                                {
            //                                    kbuf[i] ^= amctrl_key2[i];
            //                                }
            //                                retv = kirk4(ref kirk_buf, 0x10, 0x39);
            //                                for (i = 0; i < 16; i++)
            //                                {
            //                                    kbuf[i] ^= amctrl_key3[i];
            //                                }
            //                            }

            //                            if (retv != 0)
            //                            {
            //                                return retv;
            //                            }

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(ckey.key, kbuf, 0x10);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(header_key, kbuf, 0x10);

            //                            if (version_key != 0)
            //                            {
            //                                for (i = 0; i < 16; i++)
            //                                {
            //                                    ckey.key[i] ^= version_key[i];
            //                                }
            //                            }
            //                        }
            //                        else
            //                        {
            //                            retv = 0;
            //                        }

            //                        return retv;
            //                    }
            //                    public static int sceDrmBBCipherUpdate(CIPHER_KEY ckey, ref byte data, int size)
            //                    {
            //                        int p;
            //                        int retv;
            //                        int dsize;

            //                        retv = 0;
            //                        p = 0;

            //                        while (size > 0)
            //                        {
            //                            dsize = (size >= 0x0800) ? 0x0800 : size;
            //                            retv = cipher_buf(kirk_buf, data + p, dsize, ckey);

            //                            if (retv != 0)
            //                            {
            //                                break;
            //                            }

            //                            size -= dsize;
            //                            p += dsize;
            //                        }

            //                        return retv;
            //                    }
            //                    public static int sceDrmBBCipherFinal(CIPHER_KEY ckey)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(ckey.key, 0, 16);
            //                        ckey.type = 0;
            //                        ckey.seed = 0;

            //                        return 0;
            //                    }

            //                    /*
            //                        sceNpDrm functions.
            //                    */

            //                    public static int sceNpDrmGetFixedKey(ref byte[] key, ref string npstr, int type)
            //                    {
            //                        AesCtx akey = new AesCtx();
            //                        MAC_KEY mkey = new MAC_KEY();
            //                        string strbuf = new string(new char[0x30]);
            //                        int retv;

            //                        if ((type & 0x01000000) == 0)
            //                        {
            //                            return 0x80550901;
            //                        }

            //                        type &= 0x000000ff;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(strbuf, 0, 0x30);
            //                        strbuf = npstr.Substring(0, 0x30);

            //                        retv = sceDrmBBMacInit(mkey, 1);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        retv = sceDrmBBMacUpdate(mkey, (byte)strbuf, 0x30);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        retv = sceDrmBBMacFinal(mkey, ref key, npdrm_fixed_key);

            //                        if (retv != 0)
            //                        {
            //                            return 0x80550902;
            //                        }

            //                        if (type == 0)
            //                        {
            //                            return 0;
            //                        }
            //                        if (type > 3)
            //                        {
            //                            return 0x80550901;
            //                        }

            //                        type = (type - 1) * 16;

            //                        AES_set_key(akey, npdrm_enc_keys[type], 128);
            //                        AES_encrypt(akey, key, ref key);

            //                        return 0;
            //                    }


            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define AES_MAXKEYBYTES (AES_MAXKEYBITS/8)
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define pwuAESContextBuffer RijndaelCtx

            //                    // KIRK buffer.
            //                    internal static byte[] kirk_buf = new byte[0x0814];

            //                    // AMCTRL keys.
            //                    internal static byte[] amctrl_key1 = { 0xE3, 0x50, 0xED, 0x1D, 0x91, 0x0A, 0x1F, 0xD0, 0x29, 0xBB, 0x1C, 0x3E, 0xF3, 0x40, 0x77, 0xFB };
            //                    internal static byte[] amctrl_key2 = { 0x13, 0x5F, 0xA4, 0x7C, 0xAB, 0x39, 0x5B, 0xA4, 0x76, 0xB8, 0xCC, 0xA9, 0x8F, 0x3A, 0x04, 0x45 };
            //                    internal static byte[] amctrl_key3 = { 0x67, 0x8D, 0x7F, 0xA3, 0x2A, 0x9C, 0xA0, 0xD1, 0x50, 0x8A, 0xD8, 0x38, 0x5E, 0x4B, 0x01, 0x7E };

            //                    // sceNpDrmGetFixedKey keys.
            //                    internal static byte[] npdrm_enc_keys = { 0x07, 0x3D, 0x9E, 0x9D, 0xA8, 0xFD, 0x3B, 0x2F, 0x63, 0x18, 0x93, 0x2E, 0xF8, 0x57, 0xA6, 0x64, 0x37, 0x49, 0xB7, 0x01, 0xCA, 0xE2, 0xE0, 0xC5, 0x44, 0x2E, 0x06, 0xB6, 0x1E, 0xFF, 0x84, 0xF2, 0x9D, 0x31, 0xB8, 0x5A, 0xC8, 0xFA, 0x16, 0x80, 0x73, 0x60, 0x18, 0x82, 0x18, 0x77, 0x91, 0x9D };
            //                    internal static byte[] npdrm_fixed_key = { 0x38, 0x20, 0xD0, 0x11, 0x07, 0xA3, 0xFF, 0x3E, 0x0A, 0x4C, 0x20, 0x85, 0x39, 0x10, 0xB5, 0x54 };

            //                    /*
            //                        KIRK wrapper functions.
            //                    */
            //                    internal static int kirk4(ref byte buf, int size, int type)
            //                    {
            //                        int retv;
            //                        uint[] header = (uint)buf;

            //                        header[0] = 4;
            //                        header[1] = 0;
            //                        header[2] = 0;
            //                        header[3] = type;
            //                        header[4] = size;

            //                        retv = sceUtilsBufferCopyWithRange(ref buf, size + 0x14, ref buf, size, 4);

            //                        if (retv != 0)
            //                        {
            //                            return 0x80510311;
            //                        }

            //                        return 0;
            //                    }

            //                    internal static int kirk7(ref byte buf, int size, int type)
            //                    {
            //                        int retv;
            //                        uint[] header = (uint)buf;

            //                        header[0] = 5;
            //                        header[1] = 0;
            //                        header[2] = 0;
            //                        header[3] = type;
            //                        header[4] = size;

            //                        retv = sceUtilsBufferCopyWithRange(ref buf, size + 0x14, ref buf, size, 7);

            //                        if (retv != 0)
            //                        {
            //                            return 0x80510311;
            //                        }

            //                        return 0;
            //                    }

            //                    internal static int kirk5(ref byte buf, int size)
            //                    {
            //                        int retv;
            //                        uint[] header = (uint)buf;

            //                        header[0] = 4;
            //                        header[1] = 0;
            //                        header[2] = 0;
            //                        header[3] = 0x0100;
            //                        header[4] = size;

            //                        retv = sceUtilsBufferCopyWithRange(ref buf, size + 0x14, ref buf, size, 5);

            //                        if (retv != 0)
            //                        {
            //                            return 0x80510312;
            //                        }

            //                        return 0;
            //                    }

            //                    internal static int kirk8(ref byte buf, int size)
            //                    {
            //                        int retv;
            //                        uint[] header = (uint)buf;

            //                        header[0] = 5;
            //                        header[1] = 0;
            //                        header[2] = 0;
            //                        header[3] = 0x0100;
            //                        header[4] = size;

            //                        retv = sceUtilsBufferCopyWithRange(ref buf, size + 0x14, ref buf, size, 8);

            //                        if (retv != 0)
            //                        {
            //                            return 0x80510312;
            //                        }

            //                        return 0;
            //                    }

            //                    internal static int kirk14(ref byte buf)
            //                    {
            //                        int retv;

            //                        retv = sceUtilsBufferCopyWithRange(ref buf, 0x14, 0, 0, 14);

            //                        if (retv != 0)
            //                        {
            //                            return 0x80510315;
            //                        }

            //                        return 0;
            //                    }

            //                    /*
            //                        Internal functions.
            //                    */
            //                    internal static int encrypt_buf(byte[] buf, int size, byte[] key, int key_type)
            //                    {
            //                        int i;
            //                        int retv;

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            buf[0x14 + i] ^= key[i];
            //                        }

            //                        retv = kirk4(ref buf, size, key_type);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(key, buf + size + 4, 16);

            //                        return 0;
            //                    }

            //                    internal static int decrypt_buf(byte[] buf, int size, byte[] key, int key_type)
            //                    {
            //                        int i;
            //                        int retv;
            //                        byte[] tmp = new byte[16];

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp, buf + size + 0x14 - 16, 16);

            //                        retv = kirk7(ref buf, size, key_type);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            buf[i] ^= key[i];
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(key, tmp, 16);

            //                        return 0;
            //                    }

            //                    internal static int cipher_buf(byte[] kbuf, byte[] dbuf, int size, CIPHER_KEY ckey)
            //                    {
            //                        int i;
            //                        int retv;
            //                        byte[] tmp1 = new byte[16];
            //                        byte[] tmp2 = new byte[16];

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf + 0x14, ckey.key, 16);

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            kbuf[0x14 + i] ^= amctrl_key3[i];
            //                        }

            //                        if (ckey.type == 2)
            //                        {
            //                            retv = kirk8(ref kbuf, 16);
            //                        }
            //                        else
            //                        {
            //                            retv = kirk7(ref kbuf, 16, 0x39);
            //                        }

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        for (i = 0; i < 16; i++)
            //                        {
            //                            kbuf[i] ^= amctrl_key2[i];
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp2, kbuf, 0x10);

            //                        if (ckey.seed == 1)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(tmp1, 0, 0x10);
            //                        }
            //                        else
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(tmp1, tmp2, 0x10);
            //                            (uint)(tmp1 + 0x0c) = ckey.seed - 1;
            //                        }

            //                        for (i = 0; i < size; i += 16)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kbuf + 0x14 + i, tmp2, 12);
            //                            (uint)(kbuf + 0x14 + i + 12) = ckey.seed;
            //                            ckey.seed += 1;
            //                        }

            //                        retv = decrypt_buf(kbuf, size, tmp1, 0x63);

            //                        if (retv != 0)
            //                        {
            //                            return retv;
            //                        }

            //                        for (i = 0; i < size; i++)
            //                        {
            //                            dbuf[i] ^= kbuf[i];
            //                        }

            //                        return 0;
            //                    }
            //                    // Copyright 2007,2008,2010  Segher Boessenkool  <segher@kernel.crashing.org>
            //                    // Licensed under the terms of the GNU GPL, version 2
            //                    // http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt


            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define round_up(x,n) (-(-(x) & -(n)))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define array_size(x) (sizeof(x) / sizeof(*(x)))

            //                    public static void bn_print(ref string name, byte[] a, uint n)
            //                    {
            //                        uint i;

            //                        Console.Write("{0} = ", name);

            //                        for (i = 0; i < n; i++)
            //                        {
            //                            Console.Write("{0:x2}", a[i]);
            //                        }

            //                        Console.Write("\n");
            //                    }

            //                    internal static void bn_zero(ref byte d, uint n)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(d, 0, n);
            //                    }

            //                    public static void bn_copy(ref byte d, ref byte a, uint n)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(d, a, n);
            //                    }

            //                    public static int bn_compare(byte[] a, byte[] b, uint n)
            //                    {
            //                        uint i;

            //                        for (i = 0; i < n; i++)
            //                        {
            //                            if (a[i] < b[i])
            //                            {
            //                                return -1;
            //                            }
            //                            if (a[i] > b[i])
            //                            {
            //                                return 1;
            //                            }
            //                        }

            //                        return 0;
            //                    }

            //                    internal static byte bn_add_1(byte[] d, byte[] a, byte[] b, uint n)
            //                    {
            //                        uint i;
            //                        uint dig;
            //                        byte c;

            //                        c = 0;
            //                        for (i = n - 1; i < n; i--)
            //                        {
            //                            dig = a[i] + b[i] + c;
            //                            c = dig >> 8;
            //                            d[i] = dig;
            //                        }

            //                        return c;
            //                    }

            //                    internal static byte bn_sub_1(byte[] d, byte[] a, byte[] b, uint n)
            //                    {
            //                        uint i;
            //                        uint dig;
            //                        byte c;

            //                        c = 1;
            //                        for (i = n - 1; i < n; i--)
            //                        {
            //                            dig = a[i] + 255 - b[i] + c;
            //                            c = dig >> 8;
            //                            d[i] = dig;
            //                        }

            //                        return 1 - c;
            //                    }

            //                    public static void bn_reduce(ref byte d, ref byte N, uint n)
            //                    {
            //                        if (bn_compare(d, N, n) >= 0)
            //                        {
            //                            bn_sub_1(d, d, N, n);
            //                        }
            //                    }

            //                    public static void bn_add(ref byte d, ref byte a, ref byte b, ref byte N, uint n)
            //                    {
            //                        if (bn_add_1(d, a, b, n) != 0)
            //                        {
            //                            bn_sub_1(d, d, N, n);
            //                        }

            //                        bn_reduce(ref d, ref N, n);
            //                    }

            //                    public static void bn_sub(ref byte d, ref byte a, ref byte b, ref byte N, uint n)
            //                    {
            //                        if (bn_sub_1(d, a, b, n) != 0)
            //                        {
            //                            bn_add_1(d, d, N, n);
            //                        }
            //                    }

            //                    internal static byte[] inv256 = { 0x01, 0xab, 0xcd, 0xb7, 0x39, 0xa3, 0xc5, 0xef, 0xf1, 0x1b, 0x3d, 0xa7, 0x29, 0x13, 0x35, 0xdf, 0xe1, 0x8b, 0xad, 0x97, 0x19, 0x83, 0xa5, 0xcf, 0xd1, 0xfb, 0x1d, 0x87, 0x09, 0xf3, 0x15, 0xbf, 0xc1, 0x6b, 0x8d, 0x77, 0xf9, 0x63, 0x85, 0xaf, 0xb1, 0xdb, 0xfd, 0x67, 0xe9, 0xd3, 0xf5, 0x9f, 0xa1, 0x4b, 0x6d, 0x57, 0xd9, 0x43, 0x65, 0x8f, 0x91, 0xbb, 0xdd, 0x47, 0xc9, 0xb3, 0xd5, 0x7f, 0x81, 0x2b, 0x4d, 0x37, 0xb9, 0x23, 0x45, 0x6f, 0x71, 0x9b, 0xbd, 0x27, 0xa9, 0x93, 0xb5, 0x5f, 0x61, 0x0b, 0x2d, 0x17, 0x99, 0x03, 0x25, 0x4f, 0x51, 0x7b, 0x9d, 0x07, 0x89, 0x73, 0x95, 0x3f, 0x41, 0xeb, 0x0d, 0xf7, 0x79, 0xe3, 0x05, 0x2f, 0x31, 0x5b, 0x7d, 0xe7, 0x69, 0x53, 0x75, 0x1f, 0x21, 0xcb, 0xed, 0xd7, 0x59, 0xc3, 0xe5, 0x0f, 0x11, 0x3b, 0x5d, 0xc7, 0x49, 0x33, 0x55, 0xff };

            //                    internal static void bn_mon_muladd_dig(byte[] d, byte[] a, byte b, byte[] N, uint n)
            //                    {
            //                        uint dig;
            //                        uint i;

            //                        byte z = -(d[n - 1] + a[n - 1] * b) * inv256[N[n - 1] / 2];

            //                        dig = d[n - 1] + a[n - 1] * b + N[n - 1] * z;
            //                        dig >>= 8;

            //                        for (i = n - 2; i < n; i--)
            //                        {
            //                            dig += d[i] + a[i] * b + N[i] * z;
            //                            d[i + 1] = dig;
            //                            dig >>= 8;
            //                        }

            //                        d[0] = dig;
            //                        dig >>= 8;

            //                        if (dig != 0)
            //                        {
            //                            bn_sub_1(d, d, N, n);
            //                        }

            //                        bn_reduce(ref d, ref N, n);
            //                    }

            //                    public static void bn_mon_mul(ref byte d, ref byte a, byte[] b, ref byte N, uint n)
            //                    {
            //                        byte[] t = new byte[512];
            //                        uint i;

            //                        bn_zero(ref t, n);

            //                        for (i = n - 1; i < n; i--)
            //                        {
            //                            bn_mon_muladd_dig(t, a, b[i], N, n);
            //                        }

            //                        bn_copy(ref d, ref t, n);
            //                    }

            //                    public static void bn_to_mon(ref byte d, ref byte N, uint n)
            //                    {
            //                        uint i;

            //                        for (i = 0; i < 8 * n; i++)
            //                        {
            //                            bn_add(ref d, ref d, ref d, ref N, n);
            //                        }
            //                    }

            //                    public static void bn_from_mon(ref byte d, ref byte N, uint n)
            //                    {
            //                        byte[] t = new byte[512];

            //                        bn_zero(ref t, n);
            //                        t[n - 1] = 1;
            //                        bn_mon_mul(ref d, ref d, t, ref N, n);
            //                    }

            //                    internal static void bn_mon_exp(byte[] d, ref byte a, ref byte N, uint n, byte[] e, uint en)
            //                    {
            //                        byte[] t = new byte[512];
            //                        uint i;
            //                        byte mask;

            //                        bn_zero(ref d, n);
            //                        d[n - 1] = 1;
            //                        bn_to_mon(ref d, ref N, n);

            //                        for (i = 0; i < en; i++)
            //                        {
            //                            for (mask = 0x80; mask != 0; mask >>= 1)
            //                            {
            //                                bn_mon_mul(ref t, ref d, d, ref N, n);
            //                                if ((e[i] & mask) != 0)
            //                                {
            //                                    bn_mon_mul(ref d, ref t, a, ref N, n);
            //                                }
            //                                else
            //                                {
            //                                    bn_copy(ref d, ref t, n);
            //                                }
            //                            }
            //                        }
            //                    }

            //                    public static void bn_mon_inv(ref byte d, ref byte a, ref byte N, uint n)
            //                    {
            //                        byte[] t = new byte[512];
            //                        byte[] s = new byte[512];

            //                        bn_zero(ref s, n);
            //                        s[n - 1] = 2;
            //                        bn_sub_1(t, N, s, n);
            //                        bn_mon_exp(d, ref a, ref N, n, t, n);
            //                    }


            //                    public static byte[] ec_p = new byte[20];
            //                    public static byte[] ec_a = new byte[20];
            //                    public static byte[] ec_b = new byte[20];
            //                    public static byte[] ec_N = new byte[21];
            //                    public static point ec_G = new point(); // mon
            //                    public static point ec_Q = new point(); // mon
            //                    public static byte[] ec_k = new byte[21];

            //                    public static void hex_dump(ref string str, byte[] buf, int size)
            //                    {
            //                        int i;

            //                        if (str != null)
            //                        {
            //                            Console.Write("{0}:", str);
            //                        }

            //                        for (i = 0; i < size; i++)
            //                        {
            //                            if ((i % 32) == 0)
            //                            {
            //                                Console.Write("\n{0,4:X}:", i);
            //                            }
            //                            Console.Write(" {0:X2}", buf[i]);
            //                        }
            //                        Console.Write("\n\n");
            //                    }

            //                    internal static void elt_copy(ref byte d, ref byte a)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(d, a, 20);
            //                    }

            //                    internal static void elt_zero(ref byte d)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(d, 0, 20);
            //                    }

            //                    internal static int elt_is_zero(byte[] d)
            //                    {
            //                        uint i;

            //                        for (i = 0; i < 20; i++)
            //                        {
            //                            if (d[i] != 0)
            //                            {
            //                                return 0;
            //                            }
            //                        }

            //                        return 1;
            //                    }

            //                    internal static void elt_add(ref byte d, ref byte a, ref byte b)
            //                    {
            //                        bn_add(ref d, ref a, ref b, ref ec_p, 20);
            //                    }

            //                    internal static void elt_sub(ref byte d, ref byte a, ref byte b)
            //                    {
            //                        bn_sub(ref d, ref a, ref b, ref ec_p, 20);
            //                    }

            //                    internal static void elt_mul(ref byte d, ref byte a, ref byte b)
            //                    {
            //                        bn_mon_mul(ref d, ref a, ref b, ref ec_p, 20);
            //                    }

            //                    internal static void elt_square(ref byte d, ref byte a)
            //                    {
            //                        elt_mul(ref d, ref a, ref a);
            //                    }

            //                    internal static void elt_inv(ref byte d, ref byte a)
            //                    {
            //                        byte[] s = new byte[20];
            //                        elt_copy(ref s, ref a);
            //                        bn_mon_inv(ref d, ref s, ref ec_p, 20);
            //                    }

            //                    internal static void point_to_mon(point p)
            //                    {
            //                        bn_to_mon(ref p.x, ref ec_p, 20);
            //                        bn_to_mon(ref p.y, ref ec_p, 20);
            //                    }

            //                    internal static void point_from_mon(point p)
            //                    {
            //                        bn_from_mon(ref p.x, ref ec_p, 20);
            //                        bn_from_mon(ref p.y, ref ec_p, 20);
            //                    }

            //#if false
            //	//static int point_is_on_curve(u8 *p)
            //	//{
            //	//	u8 s[20], t[20];
            //	//	u8 *x, *y;
            //	//
            //	//	x = p;
            //	//	y = p + 20;
            //	//
            //	//	elt_square(t, x);
            //	//	elt_mul(s, t, x);
            //	//
            //	//	elt_mul(t, x, ec_a);
            //	//	elt_add(s, s, t);
            //	//
            //	//	elt_add(s, s, ec_b);
            //	//
            //	//	elt_square(t, y);
            //	//	elt_sub(s, s, t);
            //	//
            //	//	return elt_is_zero(s);
            //	//}
            //#endif

            //                    internal static void point_zero(point p)
            //                    {
            //                        elt_zero(ref p.x);
            //                        elt_zero(ref p.y);
            //                    }

            //                    internal static int point_is_zero(point p)
            //                    {
            //                        return elt_is_zero(p.x) && elt_is_zero(p.y) != 0;
            //                    }

            //                    internal static void point_double(point r, point p)
            //                    {
            //                        byte[] s = new byte[20];
            //                        byte[] t = new byte[20];
            //                        point pp = new point();
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *px, *py, *rx, *ry;
            //                        byte px;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *py;
            //                        byte py;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *rx;
            //                        byte rx;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *ry;
            //                        byte ry;

            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            //                        //ORIGINAL LINE: pp = *p;
            //                        pp.CopyFrom(p);

            //                        px = pp.x;
            //                        py = pp.y;
            //                        rx = r.x;
            //                        ry = r.y;

            //                        if (elt_is_zero(py) != 0)
            //                        {
            //                            point_zero(r);
            //                            return;
            //                        }

            //                        elt_square(ref t, ref px); // t = px*px
            //                        elt_add(ref s, ref t, ref t); // s = 2*px*px
            //                        elt_add(ref s, ref s, ref t); // s = 3*px*px
            //                        elt_add(ref s, ref s, ref ec_a); // s = 3*px*px + a
            //                        elt_add(ref t, ref py, ref py); // t = 2*py
            //                        elt_inv(ref t, ref t); // t = 1/(2*py)
            //                        elt_mul(ref s, ref s, ref t); // s = (3*px*px+a)/(2*py)

            //                        elt_square(ref rx, ref s); // rx = s*s
            //                        elt_add(ref t, ref px, ref px); // t = 2*px
            //                        elt_sub(ref rx, ref rx, ref t); // rx = s*s - 2*px

            //                        elt_sub(ref t, ref px, ref rx); // t = -(rx-px)
            //                        elt_mul(ref ry, ref s, ref t); // ry = -s*(rx-px)
            //                        elt_sub(ref ry, ref ry, ref py); // ry = -s*(rx-px) - py
            //                    }

            //                    internal static void point_add(point r, point p, point q)
            //                    {
            //                        byte[] s = new byte[20];
            //                        byte[] t = new byte[20];
            //                        byte[] u = new byte[20];
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *px, *py, *qx, *qy, *rx, *ry;
            //                        byte px;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *py;
            //                        byte py;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *qx;
            //                        byte qx;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *qy;
            //                        byte qy;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *rx;
            //                        byte rx;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *ry;
            //                        byte ry;
            //                        point pp = new point();
            //                        point qq = new point();

            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            //                        //ORIGINAL LINE: pp = *p;
            //                        pp.CopyFrom(p);
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
            //                        //ORIGINAL LINE: qq = *q;
            //                        qq.CopyFrom(q);

            //                        px = pp.x;
            //                        py = pp.y;
            //                        qx = qq.x;
            //                        qy = qq.y;
            //                        rx = r.x;
            //                        ry = r.y;

            //                        if (point_is_zero(pp) != 0)
            //                        {
            //                            elt_copy(ref rx, ref qx);
            //                            elt_copy(ref ry, ref qy);
            //                            return;
            //                        }

            //                        if (point_is_zero(qq) != 0)
            //                        {
            //                            elt_copy(ref rx, ref px);
            //                            elt_copy(ref ry, ref py);
            //                            return;
            //                        }

            //                        elt_sub(ref u, ref qx, ref px);

            //                        if (elt_is_zero(u) != 0)
            //                        {
            //                            elt_sub(ref u, ref qy, ref py);
            //                            if (elt_is_zero(u) != 0)
            //                            {
            //                                point_double(r, pp);
            //                            }
            //                            else
            //                            {
            //                                point_zero(r);
            //                            }

            //                            return;
            //                        }

            //                        elt_inv(ref t, ref u); // t = 1/(qx-px)
            //                        elt_sub(ref u, ref qy, ref py); // u = qy-py
            //                        elt_mul(ref s, ref t, ref u); // s = (qy-py)/(qx-px)

            //                        elt_square(ref rx, ref s); // rx = s*s
            //                        elt_add(ref t, ref px, ref qx); // t = px+qx
            //                        elt_sub(ref rx, ref rx, ref t); // rx = s*s - (px+qx)

            //                        elt_sub(ref t, ref px, ref rx); // t = -(rx-px)
            //                        elt_mul(ref ry, ref s, ref t); // ry = -s*(rx-px)
            //                        elt_sub(ref ry, ref ry, ref py); // ry = -s*(rx-px) - py
            //                    }

            //                    internal static void point_mul(point d, byte[] a, point b)
            //                    {
            //                        uint i;
            //                        byte mask;

            //                        point_zero(d);

            //                        for (i = 0; i < 21; i++)
            //                        {
            //                            for (mask = 0x80; mask != 0; mask >>= 1)
            //                            {
            //                                point_double(d, d);
            //                                if ((a[i] & mask) != 0)
            //                                {
            //                                    point_add(d, d, b);
            //                                }
            //                            }
            //                        }
            //                    }

            //                    internal static void generate_ecdsa(ref byte outR, ref byte outS, ref byte k, ref byte hash)
            //                    {
            //                        byte[] e = new byte[21];
            //                        byte[] kk = new byte[21];
            //                        byte[] m = new byte[21];
            //                        byte[] R = new byte[21];
            //                        byte[] S = new byte[21];
            //                        byte[] minv = new byte[21];
            //                        point mG = new point();

            //                        e[0] = 0;
            //                        R[0] = 0;
            //                        S[0] = 0;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(e + 1, hash, 20);
            //                        bn_reduce(ref e, ref ec_N, 21);

            //                        kirk_CMD14(ref m + 1, 20);
            //                        m[0] = 0;

            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: point_mul(&mG, m, &ec_G);
            //                        point_mul(mG, m, new point(ec_G));
            //                        point_from_mon(mG);
            //                        R[0] = 0;
            //                        elt_copy(R + 1, ref mG.x);

            //                        bn_copy(ref kk, ref k, 21);
            //                        bn_reduce(ref kk, ref ec_N, 21);
            //                        bn_to_mon(ref m, ref ec_N, 21);
            //                        bn_to_mon(ref e, ref ec_N, 21);
            //                        bn_to_mon(ref R, ref ec_N, 21);
            //                        bn_to_mon(ref kk, ref ec_N, 21);

            //                        bn_mon_mul(ref S, ref R, ref kk, ref ec_N, 21);
            //                        bn_add(ref kk, ref S, ref e, ref ec_N, 21);
            //                        bn_mon_inv(ref minv, ref m, ref ec_N, 21);
            //                        bn_mon_mul(ref S, ref minv, ref kk, ref ec_N, 21);

            //                        bn_from_mon(ref R, ref ec_N, 21);
            //                        bn_from_mon(ref S, ref ec_N, 21);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(outR, R + 1, 0x20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(outS, S + 1, 0x20);
            //                    }

            //                    internal static int check_ecdsa(point Q, ref byte inR, ref byte inS, ref byte hash)
            //                    {
            //                        byte[] Sinv = new byte[21];
            //                        byte[] e = new byte[21];
            //                        byte[] R = new byte[21];
            //                        byte[] S = new byte[21];
            //                        byte[] w1 = new byte[21];
            //                        byte[] w2 = new byte[21];
            //                        point r1 = new point();
            //                        point r2 = new point();
            //                        byte[] rr = new byte[21];

            //                        e[0] = 0;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(e + 1, hash, 20);
            //                        bn_reduce(ref e, ref ec_N, 21);
            //                        R[0] = 0;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(R + 1, inR, 20);
            //                        bn_reduce(ref R, ref ec_N, 21);
            //                        S[0] = 0;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(S + 1, inS, 20);
            //                        bn_reduce(ref S, ref ec_N, 21);

            //                        bn_to_mon(ref R, ref ec_N, 21);
            //                        bn_to_mon(ref S, ref ec_N, 21);
            //                        bn_to_mon(ref e, ref ec_N, 21);
            //                        // make Sinv = 1/S
            //                        bn_mon_inv(ref Sinv, ref S, ref ec_N, 21);
            //                        // w1 = m * Sinv
            //                        bn_mon_mul(ref w1, ref e, ref Sinv, ref ec_N, 21);
            //                        // w2 = r * Sinv
            //                        bn_mon_mul(ref w2, ref R, ref Sinv, ref ec_N, 21);

            //                        // mod N both
            //                        bn_from_mon(ref w1, ref ec_N, 21);
            //                        bn_from_mon(ref w2, ref ec_N, 21);

            //                        // r1 = m/s * G
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: point_mul(&r1, w1, &ec_G);
            //                        point_mul(r1, w1, new point(ec_G));
            //                        // r2 = r/s * P
            //                        point_mul(r2, w2, Q);

            //                        //r1 = r1 + r2
            //                        point_add(r1, r1, r2);

            //                        point_from_mon(r1);

            //                        rr[0] = 0;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(rr + 1, r1.x, 20);
            //                        bn_reduce(ref rr, ref ec_N, 21);

            //                        bn_from_mon(ref R, ref ec_N, 21);
            //                        bn_from_mon(ref S, ref ec_N, 21);

            //                        return (bn_compare(ref rr, ref R, 21) == 0);
            //                    }

            //                    public static void ec_priv_to_pub(ref byte k, ref byte Q)
            //                    {
            //                        point ec_temp = new point();
            //                        bn_to_mon(ref k, ref ec_N, 21);
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: point_mul(&ec_temp, k, &ec_G);
            //                        point_mul(ec_temp, k, new point(ec_G));
            //                        point_from_mon(ec_temp);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(Q, ec_temp.x, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(Q + 20, ec_temp.y, 20);
            //                    }

            //                    public static void ec_pub_mult(ref byte k, ref byte Q)
            //                    {
            //                        point ec_temp = new point();
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: point_mul(&ec_temp, k, &ec_Q);
            //                        point_mul(ec_temp, k, new point(ec_Q));
            //                        point_from_mon(ec_temp);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(Q, ec_temp.x, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(Q + 20, ec_temp.y, 20);
            //                    }

            //                    public static int ecdsa_set_curve(ref byte p, ref byte a, ref byte b, ref byte N, ref byte Gx, ref byte Gy)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_p, p, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_a, a, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_b, b, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_N, N, 21);

            //                        bn_to_mon(ref ec_a, ref ec_p, 20);
            //                        bn_to_mon(ref ec_b, ref ec_p, 20);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_G.x, Gx, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_G.y, Gy, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: point_to_mon(&ec_G);
            //                        point_to_mon(new point(ec_G));

            //                        return 0;
            //                    }

            //                    public static void ecdsa_set_pub(ref byte Q)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_Q.x, Q, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_Q.y, Q + 20, 20);
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: point_to_mon(&ec_Q);
            //                        point_to_mon(new point(ec_Q));
            //                    }

            //                    public static void ecdsa_set_priv(ref byte ink)
            //                    {
            //                        byte[] k = new byte[21];
            //                        k[0] = 0;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(k + 1, ink, 20);
            //                        bn_reduce(ref k, ref ec_N, 21);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ec_k, k, sizeof(byte));
            //                    }

            //                    public static int ecdsa_verify(ref byte hash, ref byte R, ref byte S)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: return check_ecdsa(&ec_Q, R, S, hash);
            //                        return check_ecdsa(new point(ec_Q), ref R, ref S, ref hash);
            //                    }

            //                    public static void ecdsa_sign(ref byte hash, ref byte R, ref byte S)
            //                    {
            //                        generate_ecdsa(ref R, ref S, ref ec_k, ref hash);
            //                    }

            //                    public static int point_is_on_curve(ref byte p)
            //                    {
            //                        byte[] s = new byte[20];
            //                        byte[] t = new byte[20];
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *x, *y;
            //                        byte x;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *y;
            //                        byte y;

            //                        x = p;
            //                        y = p + 20;

            //                        elt_square(ref t, ref x);
            //                        elt_mul(ref s, ref t, ref x); // s = x^3

            //                        elt_mul(ref t, ref x, ref ec_a);
            //                        elt_add(ref s, ref s, ref t); //s = x^3 + a *x

            //                        elt_add(ref s, ref s, ref ec_b); //s = x^3 + a *x + b

            //                        elt_square(ref t, ref y); //t = y^2
            //                        elt_sub(ref s, ref s, ref t); // is s - t = 0?
            //                        hex_dump("S", s, 20);
            //                        hex_dump("T", t, 20);
            //                        return elt_is_zero(s);
            //                    }

            //                    public static void dump_ecc()
            //                    {
            //                        hex_dump("P", ec_p, 20);
            //                        hex_dump("a", ec_a, 20);
            //                        hex_dump("b", ec_b, 20);
            //                        hex_dump("N", ec_N, 21);
            //                        hex_dump("Gx", ec_G.x, 20);
            //                        hex_dump("Gy", ec_G.y, 20);
            //                    }


            //                    // KIRK commands

            //                    // KIRK commands
            //                    /*
            //                        // Private Sig + Cipher
            //                        0x01: Super-Duper decryption (no inverse)
            //                        0x02: Encrypt Operation (inverse of 0x03)
            //                        0x03: Decrypt Operation (inverse of 0x02)

            //                        // Cipher
            //                        0x04: Encrypt Operation (inverse of 0x07) (IV=0)
            //                        0x05: Encrypt Operation (inverse of 0x08) (IV=FuseID)
            //                        0x06: Encrypt Operation (inverse of 0x09) (IV=UserDefined)
            //                        0x07: Decrypt Operation (inverse of 0x04)
            //                        0x08: Decrypt Operation (inverse of 0x05)
            //                        0x09: Decrypt Operation (inverse of 0x06)

            //                        // Sig Gens
            //                        0x0A: Private Signature Check (checks for private SCE sig)
            //                        0x0B: SHA1 Hash
            //                        0x0C: Mul1
            //                        0x0D: Mul2
            //                        0x0E: Random Number Gen
            //                        0x0F: (absolutely no idea � could be KIRK initialization)
            //                        0x10: Signature Gen

            //                        // Sig Checks
            //                        0x11: Signature Check (checks for generated sigs)
            //                        0x12: Certificate Check (idstorage signatures)
            //                    */

            //                    public static int kirk_init()
            //                    {
            //                        return kirk_init2(ref (byte)"Lazy Dev should have initialized!", 33, 0xBABEF00D, 0xDEADBEEF);
            //                    }
            //                    public static int kirk_init2(ref byte rnd_seed, uint seed_size, uint fuseid_90, uint fuseid_94)
            //                    {
            //                        byte[] temp = new byte[0x104];

            //                        KIRK_SHA1_HEADER header = (KIRK_SHA1_HEADER)temp;

            //                        // Another randomly selected data for a "key" to add to each randomization
            //                        byte[] key = { 0x07, 0xAB, 0xEF, 0xF8, 0x96, 0x8C, 0xF3, 0xD6, 0x14, 0xE0, 0xEB, 0xB2, 0x9D, 0x8B, 0x4E, 0x74 };
            //                        uint curtime;

            //                        //Set PRNG_DATA initially, otherwise use what ever uninitialized data is in the buffer
            //                        if (seed_size > 0)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte * seedbuf;
            //                            byte seedbuf;
            //                            KIRK_SHA1_HEADER seedheader;
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                            seedbuf = (byte)malloc(seed_size + 4);
            //                            seedheader = (KIRK_SHA1_HEADER)seedbuf;
            //                            seedheader.data_size = seed_size;
            //                            kirk_CMD11(ref PRNG_DATA, ref seedbuf, seed_size + 4);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                            free(seedbuf);
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(temp + 4, PRNG_DATA, 0x14);

            //                        // This uses the standard C time function for portability.
            //                        curtime = (uint)time(0);
            //                        temp[0x18] = curtime & 0xFF;
            //                        temp[0x19] = (curtime >> 8) & 0xFF;
            //                        temp[0x1A] = (curtime >> 16) & 0xFF;
            //                        temp[0x1B] = (curtime >> 24) & 0xFF;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(temp[0x1C], key, 0x10);

            //                        // This leaves the remainder of the 0x100 bytes in temp to whatever remains on the stack
            //                        // in an uninitialized state. This should add unpredicableness to the results as well
            //                        header.data_size = 0x100;
            //                        kirk_CMD11(ref PRNG_DATA, ref temp, 0x104);

            //                        //Set Fuse ID
            //                        g_fuse90 = fuseid_90;
            //                        g_fuse94 = fuseid_94;

            //                        // Set KIRK1 main key
            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: AES_set_key(&aes_kirk1, kirk1_key, 128);
            //                        AES_set_key(new AesCtx(aes_kirk1), kirk1_key, 128);

            //                        is_kirk_initialized = 1;
            //                        return 0;
            //                    }
            //                    public static int kirk_CMD0(ref byte outbuff, ref byte inbuff, int size, int generate_trash)
            //                    {
            //                        KIRK_CMD1_HEADER header = (KIRK_CMD1_HEADER)outbuff;
            //                        header_keys keys = (header_keys)outbuff; //0-15 AES key, 16-31 CMAC key
            //                        int chk_size;
            //                        AesCtx k1 = new AesCtx();
            //                        AesCtx cmac_key = new AesCtx();
            //                        byte[] cmac_header_hash = new byte[16];
            //                        byte[] cmac_data_hash = new byte[16];

            //                        if (is_kirk_initialized == 0)
            //                        {
            //                            return DefineConstants.KIRK_NOT_INITIALIZED;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(outbuff, inbuff, size);

            //                        if (header.mode != DefineConstants.KIRK_MODE_CMD1)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_MODE;
            //                        }

            //                        // FILL PREDATA WITH RANDOM DATA
            //                        if (generate_trash != 0)
            //                        {
            //                            kirk_CMD14(outbuff + sizeof(KIRK_CMD1_HEADER), header.data_offset);
            //                        }

            //                        // Make sure data is 16 aligned
            //                        chk_size = header.data_size;
            //                        if (chk_size % 16 != 0)
            //                        {
            //                            chk_size += 16 - (chk_size % 16);
            //                        }

            //                        // ENCRYPT DATA
            //                        AES_set_key(k1, keys.AES, 128);
            //                        AES_cbc_encrypt(k1, ref inbuff + sizeof(KIRK_CMD1_HEADER) + header.data_offset, ref (byte)outbuff + sizeof(KIRK_CMD1_HEADER) + header.data_offset, chk_size);

            //                        // CMAC HASHES
            //                        AES_set_key(cmac_key, keys.CMAC, 128);
            //                        AES_CMAC(cmac_key, ref outbuff + 0x60, 0x30, ref cmac_header_hash);
            //                        AES_CMAC(cmac_key, ref outbuff + 0x60, 0x30 + chk_size + header.data_offset, ref cmac_data_hash);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(header.CMAC_header_hash, cmac_header_hash, 16);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(header.CMAC_data_hash, cmac_data_hash, 16);

            //                        // ENCRYPT KEYS
            //                        AES_cbc_encrypt(aes_kirk1, ref inbuff, ref outbuff, 16 * 2);
            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD1(ref byte outbuff, ref byte inbuff, int size)
            //                    {
            //                        KIRK_CMD1_HEADER header = (KIRK_CMD1_HEADER)inbuff;
            //                        header_keys keys = new header_keys(); //0-15 AES key, 16-31 CMAC key
            //                        AesCtx k1 = new AesCtx();

            //                        if (size < 0x90)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }
            //                        if (is_kirk_initialized == 0)
            //                        {
            //                            return DefineConstants.KIRK_NOT_INITIALIZED;
            //                        }
            //                        if (header.mode != DefineConstants.KIRK_MODE_CMD1)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_MODE;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
            //                        //ORIGINAL LINE: AES_cbc_decrypt(&aes_kirk1, inbuff, (byte*)&keys, 16 *2);
            //                        AES_cbc_decrypt(new AesCtx(aes_kirk1), ref inbuff, ref (byte)keys, 16 * 2); //decrypt AES & CMAC key to temp buffer

            //                        if (header.ecdsa_hash == 1)
            //                        {
            //                            SHA_CTX sha = new SHA_CTX();
            //                            KIRK_CMD1_ECDSA_HEADER eheader = (KIRK_CMD1_ECDSA_HEADER)inbuff;
            //                            byte[] kirk1_pub = new byte[40];
            //                            byte[] header_hash = new byte[20];
            //                            byte[] data_hash = new byte[20];
            //                            ecdsa_set_curve(ref ec_p, ref ec_a, ref ec_b1, ref ec_N1, ref Gx1, ref Gy1);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kirk1_pub, Px1, 20);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(kirk1_pub + 20, Py1, 20);
            //                            ecdsa_set_pub(ref kirk1_pub);

            //                            //Hash the Header
            //                            SHAInit(sha);
            //                            SHAUpdate(sha, ref (byte)eheader + 0x60, 0x30);
            //                            SHAFinal(ref header_hash, sha);

            //                            if (ecdsa_verify(ref header_hash, ref eheader.header_sig_r, ref eheader.header_sig_s) == 0)
            //                            {
            //                                return DefineConstants.KIRK_HEADER_HASH_INVALID;
            //                            }

            //                            SHAInit(sha);
            //                            SHAUpdate(sha, ref (byte)eheader + 0x60, size - 0x60);
            //                            SHAFinal(ref data_hash, sha);

            //                            if (ecdsa_verify(ref data_hash, ref eheader.data_sig_r, ref eheader.data_sig_s) == 0)
            //                            {
            //                                return DefineConstants.KIRK_DATA_HASH_INVALID;
            //                            }
            //                        }
            //                        else
            //                        {
            //                            int ret = kirk_CMD10(ref inbuff, size);
            //                            if (ret != DefineConstants.KIRK_OPERATION_SUCCESS)
            //                            {
            //                                return ret;
            //                            }
            //                        }

            //                        AES_set_key(k1, keys.AES, 128);
            //                        AES_cbc_decrypt(k1, ref inbuff + sizeof(KIRK_CMD1_HEADER) + header.data_offset, ref outbuff, header.data_size);

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD1_ex(ref byte outbuff, ref byte inbuff, int size, KIRK_CMD1_HEADER header)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte* buffer = (byte*)malloc(size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte buffer = (byte)malloc(size);
            //                        int ret;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(buffer, header, sizeof(KIRK_CMD1_HEADER));
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(buffer + sizeof(KIRK_CMD1_HEADER), inbuff, header.data_size);

            //                        ret = kirk_CMD1(ref outbuff, ref buffer, size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        free(buffer);
            //                        return ret;
            //                    }
            //                    public static int kirk_CMD4(ref byte outbuff, ref byte inbuff, int size)
            //                    {
            //                        KIRK_AES128CBC_HEADER header = (KIRK_AES128CBC_HEADER)inbuff;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte* key;
            //                        byte key;
            //                        AesCtx aesKey = new AesCtx();

            //                        if (is_kirk_initialized == 0)
            //                        {
            //                            return DefineConstants.KIRK_NOT_INITIALIZED;
            //                        }
            //                        if (header.mode != DefineConstants.KIRK_MODE_ENCRYPT_CBC)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_MODE;
            //                        }
            //                        if (header.data_size == 0)
            //                        {
            //                            return DefineConstants.KIRK_DATA_SIZE_ZERO;
            //                        }

            //                        key = kirk_4_7_get_key(header.keyseed);
            //                        if (key == (byte)DefineConstants.KIRK_INVALID_SIZE)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }

            //                        // Set the key
            //                        AES_set_key(aesKey, key, 128);
            //                        AES_cbc_encrypt(aesKey, ref inbuff + sizeof(KIRK_AES128CBC_HEADER), ref outbuff + sizeof(KIRK_AES128CBC_HEADER), size);

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD7(ref byte outbuff, ref byte inbuff, int size)
            //                    {
            //                        KIRK_AES128CBC_HEADER header = (KIRK_AES128CBC_HEADER)inbuff;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte* key;
            //                        byte key;
            //                        AesCtx aesKey = new AesCtx();

            //                        if (is_kirk_initialized == 0)
            //                        {
            //                            return DefineConstants.KIRK_NOT_INITIALIZED;
            //                        }
            //                        if (header.mode != DefineConstants.KIRK_MODE_DECRYPT_CBC)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_MODE;
            //                        }
            //                        if (header.data_size == 0)
            //                        {
            //                            return DefineConstants.KIRK_DATA_SIZE_ZERO;
            //                        }

            //                        key = kirk_4_7_get_key(header.keyseed);
            //                        if (key == (byte)DefineConstants.KIRK_INVALID_SIZE)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }

            //                        // Set the key
            //                        AES_set_key(aesKey, key, 128);
            //                        AES_cbc_decrypt(aesKey, ref inbuff + sizeof(KIRK_AES128CBC_HEADER), ref outbuff, size);

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD10(ref byte inbuff, int insize)
            //                    {
            //                        KIRK_CMD1_HEADER header = (KIRK_CMD1_HEADER)inbuff;
            //                        header_keys keys = new header_keys(); //0-15 AES key, 16-31 CMAC key
            //                        byte[] cmac_header_hash = new byte[16];
            //                        byte[] cmac_data_hash = new byte[16];
            //                        AesCtx cmac_key = new AesCtx();
            //                        int chk_size;

            //                        if (is_kirk_initialized == 0)
            //                        {
            //                            return DefineConstants.KIRK_NOT_INITIALIZED;
            //                        }
            //                        if (!(header.mode == DefineConstants.KIRK_MODE_CMD1 || header.mode == DefineConstants.KIRK_MODE_CMD2 || header.mode == DefineConstants.KIRK_MODE_CMD3))
            //                        {
            //                            return DefineConstants.KIRK_INVALID_MODE;
            //                        }
            //                        if (header.data_size == 0)
            //                        {
            //                            return DefineConstants.KIRK_DATA_SIZE_ZERO;
            //                        }

            //                        if (header.mode == DefineConstants.KIRK_MODE_CMD1)
            //                        {
            //                            AES_cbc_decrypt(aes_kirk1, ref inbuff, ref (byte)keys, 32); //decrypt AES & CMAC key to temp buffer
            //                            AES_set_key(cmac_key, keys.CMAC, 128);
            //                            AES_CMAC(cmac_key, ref inbuff + 0x60, 0x30, ref cmac_header_hash);

            //                            // Make sure data is 16 aligned
            //                            chk_size = header.data_size;
            //                            if (chk_size % 16 != 0)
            //                            {
            //                                chk_size += 16 - (chk_size % 16);
            //                            }
            //                            AES_CMAC(cmac_key, ref inbuff + 0x60, 0x30 + chk_size + header.data_offset, ref cmac_data_hash);

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
            //                            if (memcmp(cmac_header_hash, header.CMAC_header_hash, 16) != 0)
            //                            {
            //                                return DefineConstants.KIRK_HEADER_HASH_INVALID;
            //                            }
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
            //                            if (memcmp(cmac_data_hash, header.CMAC_data_hash, 16) != 0)
            //                            {
            //                                return DefineConstants.KIRK_DATA_HASH_INVALID;
            //                            }

            //                            return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                        }

            //                        return DefineConstants.KIRK_SIG_CHECK_INVALID; //Checks for cmd 2 & 3 not included right now
            //                    }
            //                    public static int kirk_CMD11(ref byte outbuff, ref byte inbuff, int size)
            //                    {
            //                        KIRK_SHA1_HEADER header = (KIRK_SHA1_HEADER)inbuff;
            //                        SHA_CTX sha = new SHA_CTX();
            //                        if (is_kirk_initialized == 0)
            //                        {
            //                            return DefineConstants.KIRK_NOT_INITIALIZED;
            //                        }
            //                        if (header.data_size == 0 || size == 0)
            //                        {
            //                            return DefineConstants.KIRK_DATA_SIZE_ZERO;
            //                        }

            //                        SHAInit(sha);
            //                        SHAUpdate(sha, ref inbuff + sizeof(KIRK_SHA1_HEADER), header.data_size);
            //                        SHAFinal(ref outbuff, sha);

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD12(ref byte outbuff, int outsize)
            //                    {
            //                        byte[] k = new byte[0x15];
            //                        KIRK_CMD12_BUFFER keypair = (KIRK_CMD12_BUFFER)outbuff;

            //                        if (outsize != 0x3C)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }
            //                        ecdsa_set_curve(ref ec_p, ref ec_a, ref ec_b2, ref ec_N2, ref Gx2, ref Gy2);
            //                        k[0] = 0;

            //                        kirk_CMD14(k + 1, 0x14);
            //                        ec_priv_to_pub(ref k, ref (byte)keypair.public_key.x);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(keypair.private_key, k + 1, 0x14);

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD13(ref byte outbuff, int outsize, ref byte inbuff, int insize)
            //                    {
            //                        byte[] k = new byte[0x15];
            //                        KIRK_CMD13_BUFFER pointmult = (KIRK_CMD13_BUFFER)inbuff;
            //                        k[0] = 0;

            //                        if (outsize != 0x28)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }
            //                        if (insize != 0x3C)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }

            //                        ecdsa_set_curve(ref ec_p, ref ec_a, ref ec_b2, ref ec_N2, ref Gx2, ref Gy2);
            //                        ecdsa_set_pub(ref (byte)pointmult.public_key.x);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(k + 1, pointmult.multiplier, 0x14);
            //                        ec_pub_mult(ref k, ref outbuff);

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'outbuff', so pointers on this parameter are left unchanged:
            //                    public static int kirk_CMD14(byte outbuff, int outsize)
            //                    {
            //                        byte[] temp = new byte[0x104];
            //                        KIRK_SHA1_HEADER header = (KIRK_SHA1_HEADER)temp;

            //                        // Some randomly selected data for a "key" to add to each randomization
            //                        byte[] key = { 0xA7, 0x2E, 0x4C, 0xB6, 0xC3, 0x34, 0xDF, 0x85, 0x70, 0x01, 0x49, 0xFC, 0xC0, 0x87, 0xC4, 0x77 };
            //                        uint curtime;

            //                        if (outsize <= 0)
            //                        {
            //                            return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(temp + 4, PRNG_DATA, 0x14);

            //                        // This uses the standard C time function for portability.
            //                        curtime = (uint)time(0);
            //                        temp[0x18] = curtime & 0xFF;
            //                        temp[0x19] = (curtime >> 8) & 0xFF;
            //                        temp[0x1A] = (curtime >> 16) & 0xFF;
            //                        temp[0x1B] = (curtime >> 24) & 0xFF;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(temp[0x1C], key, 0x10);

            //                        // This leaves the remainder of the 0x100 bytes in temp to whatever remains on the stack
            //                        // in an uninitialized state. This should add unpredicableness to the results as well
            //                        header.data_size = 0x100;
            //                        kirk_CMD11(ref PRNG_DATA, ref temp, 0x104);

            //                        while (outsize != 0)
            //                        {
            //                            int blockrem = outsize % 0x14;
            //                            int block = outsize / 0x14;

            //                            if (block != 0)
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(outbuff, PRNG_DATA, 0x14);
            //                                outbuff += 0x14;
            //                                outsize -= 0x14;
            //                                kirk_CMD14(outbuff, outsize);
            //                            }
            //                            else
            //                            {
            //                                if (blockrem != 0)
            //                                {
            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                    memcpy(outbuff, PRNG_DATA, blockrem);
            //                                    outsize -= blockrem;
            //                                }
            //                            }
            //                        }

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD16(ref byte outbuff, int outsize, ref byte inbuff, int insize)
            //                    {
            //                        byte[] dec_private = new byte[0x20];
            //                        KIRK_CMD16_BUFFER signbuf = (KIRK_CMD16_BUFFER)inbuff;
            //                        ECDSA_SIG sig = (ECDSA_SIG)outbuff;

            //                        if (insize != 0x34)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }
            //                        if (outsize != 0x28)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }

            //                        decrypt_kirk16_private(ref dec_private, ref signbuf.enc_private);

            //                        // Clear out the padding for safety
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(dec_private[0x14], 0, 0xC);

            //                        ecdsa_set_curve(ref ec_p, ref ec_a, ref ec_b2, ref ec_N2, ref Gx2, ref Gy2);
            //                        ecdsa_set_priv(ref dec_private);
            //                        ecdsa_sign(ref signbuf.message_hash, ref sig.r, ref sig.s);

            //                        return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                    }
            //                    public static int kirk_CMD17(ref byte inbuff, int insize)
            //                    {
            //                        KIRK_CMD17_BUFFER sig = (KIRK_CMD17_BUFFER)inbuff;

            //                        if (insize != 0x64)
            //                        {
            //                            return DefineConstants.KIRK_INVALID_SIZE;
            //                        }

            //                        ecdsa_set_curve(ref ec_p, ref ec_a, ref ec_b2, ref ec_N2, ref Gx2, ref Gy2);
            //                        ecdsa_set_pub(ref sig.public_key.x);

            //                        if (ecdsa_verify(ref sig.message_hash, ref sig.signature.r, ref sig.signature.s) != 0)
            //                        {
            //                            return DefineConstants.KIRK_OPERATION_SUCCESS;
            //                        }
            //                        else
            //                        {
            //                            return DefineConstants.KIRK_SIG_CHECK_INVALID;
            //                        }
            //                    }

            //                    // Internal functions

            //                    // Internal functions
            //                    //C++ TO C# CONVERTER WARNING: C# has no equivalent to methods returning pointers to value types:
            //                    //ORIGINAL LINE: byte* kirk_4_7_get_key(int key_type)
            //                    public static byte kirk_4_7_get_key(int key_type)
            //                    {
            //                        switch (key_type)
            //                        {
            //                            case (0x02):
            //                                return kirk7_key02;
            //                                break;
            //                            case (0x03):
            //                                return kirk7_key03;
            //                                break;
            //                            case (0x04):
            //                                return kirk7_key04;
            //                                break;
            //                            case (0x05):
            //                                return kirk7_key05;
            //                                break;
            //                            case (0x07):
            //                                return kirk7_key07;
            //                                break;
            //                            case (0x0C):
            //                                return kirk7_key0C;
            //                                break;
            //                            case (0x0D):
            //                                return kirk7_key0D;
            //                                break;
            //                            case (0x0E):
            //                                return kirk7_key0E;
            //                                break;
            //                            case (0x0F):
            //                                return kirk7_key0F;
            //                                break;
            //                            case (0x10):
            //                                return kirk7_key10;
            //                                break;
            //                            case (0x11):
            //                                return kirk7_key11;
            //                                break;
            //                            case (0x12):
            //                                return kirk7_key12;
            //                                break;
            //                            case (0x38):
            //                                return kirk7_key38;
            //                                break;
            //                            case (0x39):
            //                                return kirk7_key39;
            //                                break;
            //                            case (0x3A):
            //                                return kirk7_key3A;
            //                                break;
            //                            case (0x44):
            //                                return kirk7_key44;
            //                                break;
            //                            case (0x4B):
            //                                return kirk7_key4B;
            //                                break;
            //                            case (0x53):
            //                                return kirk7_key53;
            //                                break;
            //                            case (0x57):
            //                                return kirk7_key57;
            //                                break;
            //                            case (0x5D):
            //                                return kirk7_key5D;
            //                                break;
            //                            case (0x63):
            //                                return kirk7_key63;
            //                                break;
            //                            case (0x64):
            //                                return kirk7_key64;
            //                                break;
            //                            default:
            //                                return (byte)DefineConstants.KIRK_INVALID_SIZE;
            //                                break;
            //                        }
            //                    }
            //                    public static void decrypt_kirk16_private(ref byte dA_out, ref byte dA_enc)
            //                    {
            //                        int i;
            //                        int k;
            //                        kirk16_data keydata = new kirk16_data();
            //                        byte[] subkey_1 = new byte[0x10];
            //                        byte[] subkey_2 = new byte[0x10];
            //                        RijndaelCtx AesCtx = new RijndaelCtx();

            //                        keydata.fuseid[7] = g_fuse90 & 0xFF;
            //                        keydata.fuseid[6] = (g_fuse90 >> 8) & 0xFF;
            //                        keydata.fuseid[5] = (g_fuse90 >> 16) & 0xFF;
            //                        keydata.fuseid[4] = (g_fuse90 >> 24) & 0xFF;
            //                        keydata.fuseid[3] = g_fuse94 & 0xFF;
            //                        keydata.fuseid[2] = (g_fuse94 >> 8) & 0xFF;
            //                        keydata.fuseid[1] = (g_fuse94 >> 16) & 0xFF;
            //                        keydata.fuseid[0] = (g_fuse94 >> 24) & 0xFF;

            //                        /* set encryption key */
            //                        rijndael_set_key(AesCtx, kirk16_key, 128);

            //                        /* set the subkeys */
            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            /* set to the fuseid */
            //                            subkey_2[i] = subkey_1[i] = keydata.fuseid[i % 8];
            //                        }

            //                        /* do aes crypto */
            //                        for (i = 0; i < 3; i++)
            //                        {
            //                            /* encrypt + decrypt */
            //                            rijndael_encrypt(AesCtx, subkey_1, ref subkey_1);
            //                            rijndael_decrypt(AesCtx, subkey_2, ref subkey_2);
            //                        }

            //                        /* set new key */
            //                        rijndael_set_key(AesCtx, subkey_1, 128);

            //                        /* now lets make the key mesh */
            //                        for (i = 0; i < 3; i++)
            //                        {
            //                            /* do encryption in group of 3 */
            //                            for (k = 0; k < 3; k++)
            //                            {
            //                                /* crypto */
            //                                rijndael_encrypt(AesCtx, subkey_2, ref subkey_2);
            //                            }

            //                            /* copy to out block */
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(keydata.mesh[i * 0x10], subkey_2, 0x10);
            //                        }

            //                        /* set the key to the mesh */
            //                        rijndael_set_key(AesCtx, keydata.mesh[0x20], 128);

            //                        /* do the encryption routines for the aes key */
            //                        for (i = 0; i < 2; i++)
            //                        {
            //                            /* encrypt the data */
            //                            rijndael_encrypt(AesCtx, keydata.mesh[0x10], ref keydata.mesh[0x10]);
            //                        }

            //                        /* set the key to that mesh shit */
            //                        rijndael_set_key(AesCtx, keydata.mesh[0x10], 128);

            //                        /* cbc decrypt the dA */
            //                        AES_cbc_decrypt((AesCtx)AesCtx, ref dA_enc, ref dA_out, 0x20);
            //                    }
            //                    public static void encrypt_kirk16_private(ref byte dA_out, ref byte dA_dec)
            //                    {
            //                        int i;
            //                        int k;
            //                        kirk16_data keydata = new kirk16_data();
            //                        byte[] subkey_1 = new byte[0x10];
            //                        byte[] subkey_2 = new byte[0x10];
            //                        RijndaelCtx AesCtx = new RijndaelCtx();

            //                        keydata.fuseid[7] = g_fuse90 & 0xFF;
            //                        keydata.fuseid[6] = (g_fuse90 >> 8) & 0xFF;
            //                        keydata.fuseid[5] = (g_fuse90 >> 16) & 0xFF;
            //                        keydata.fuseid[4] = (g_fuse90 >> 24) & 0xFF;
            //                        keydata.fuseid[3] = g_fuse94 & 0xFF;
            //                        keydata.fuseid[2] = (g_fuse94 >> 8) & 0xFF;
            //                        keydata.fuseid[1] = (g_fuse94 >> 16) & 0xFF;
            //                        keydata.fuseid[0] = (g_fuse94 >> 24) & 0xFF;
            //                        /* set encryption key */
            //                        rijndael_set_key(AesCtx, kirk16_key, 128);

            //                        /* set the subkeys */
            //                        for (i = 0; i < 0x10; i++)
            //                        {
            //                            /* set to the fuseid */
            //                            subkey_2[i] = subkey_1[i] = keydata.fuseid[i % 8];
            //                        }

            //                        /* do aes crypto */
            //                        for (i = 0; i < 3; i++)
            //                        {
            //                            /* encrypt + decrypt */
            //                            rijndael_encrypt(AesCtx, subkey_1, ref subkey_1);
            //                            rijndael_decrypt(AesCtx, subkey_2, ref subkey_2);
            //                        }

            //                        /* set new key */
            //                        rijndael_set_key(AesCtx, subkey_1, 128);

            //                        /* now lets make the key mesh */
            //                        for (i = 0; i < 3; i++)
            //                        {
            //                            /* do encryption in group of 3 */
            //                            for (k = 0; k < 3; k++)
            //                            {
            //                                /* crypto */
            //                                rijndael_encrypt(AesCtx, subkey_2, ref subkey_2);
            //                            }

            //                            /* copy to out block */
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(keydata.mesh[i * 0x10], subkey_2, 0x10);
            //                        }

            //                        /* set the key to the mesh */
            //                        rijndael_set_key(AesCtx, keydata.mesh[0x20], 128);

            //                        /* do the encryption routines for the aes key */
            //                        for (i = 0; i < 2; i++)
            //                        {
            //                            /* encrypt the data */
            //                            rijndael_encrypt(AesCtx, keydata.mesh[0x10], ref keydata.mesh[0x10]);
            //                        }

            //                        /* set the key to that mesh shit */
            //                        rijndael_set_key(AesCtx, keydata.mesh[0x10], 128);

            //                        /* cbc encrypt the dA */
            //                        AES_cbc_encrypt((AesCtx)AesCtx, ref dA_dec, ref dA_out, 0x20);
            //                    }

            //                    // SCE functions
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //int sceUtilsSetFuseID(ref byte fuse);

            //                    // SCE functions
            //                    public static int sceUtilsBufferCopyWithRange(ref byte[] outbuff, int outsize, ref byte[] inbuff, int insize, int cmd)
            //                    {
            //                        switch (cmd)
            //                        {
            //                            case DefineConstants.KIRK_CMD_DECRYPT_PRIVATE:
            //                                return kirk_CMD1(ref outbuff, ref inbuff, insize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_ENCRYPT_IV_0:
            //                                return kirk_CMD4(ref outbuff, ref inbuff, insize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_DECRYPT_IV_0:
            //                                return kirk_CMD7(ref outbuff, ref inbuff, insize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_PRIV_SIGN_CHECK:
            //                                return kirk_CMD10(ref inbuff, insize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_SHA1_HASH:
            //                                return kirk_CMD11(ref outbuff, ref inbuff, insize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_ECDSA_GEN_KEYS:
            //                                return kirk_CMD12(ref outbuff, outsize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_ECDSA_MULTIPLY_POINT:
            //                                return kirk_CMD13(ref outbuff, outsize, ref inbuff, insize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_PRNG:
            //                                return kirk_CMD14(outbuff, outsize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_ECDSA_SIGN:
            //                                return kirk_CMD16(ref outbuff, outsize, ref inbuff, insize);
            //                                break;
            //                            case DefineConstants.KIRK_CMD_ECDSA_VERIFY:
            //                                return kirk_CMD17(ref inbuff, insize);
            //                                break;
            //                        }
            //                        return -1;
            //                    }

            //                    // Prototypes for the Elliptic Curve and Big Number functions
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //int ecdsa_get_params(uint type, ref byte p, ref byte a, ref byte b, ref byte N, ref byte Gx, ref byte Gy);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //int ecdsa_set_curve(ref byte p, ref byte a, ref byte b, ref byte N, ref byte Gx, ref byte Gy);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void ecdsa_set_pub(ref byte Q);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void ecdsa_set_priv(ref byte k);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //int ecdsa_verify(ref byte hash, ref byte R, ref byte S);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void ecdsa_sign(ref byte hash, ref byte R, ref byte S);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void ec_priv_to_pub(ref byte k, ref byte Q);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void ec_pub_mult(ref byte k, ref byte Q);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_copy(ref byte d, ref byte a, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //int bn_compare(ref byte a, ref byte b, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_reduce(ref byte d, ref byte N, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_add(ref byte d, ref byte a, ref byte b, ref byte N, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_sub(ref byte d, ref byte a, ref byte b, ref byte N, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_to_mon(ref byte d, ref byte N, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_from_mon(ref byte d, ref byte N, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_mon_mul(ref byte d, ref byte a, ref byte b, ref byte N, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void bn_mon_inv(ref byte d, ref byte a, ref byte N, uint n);
            //                    //C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
            //                    //void hex_dump(ref string str, ref byte buf, int size);

            //                    public static uint g_fuse90;
            //                    public static uint g_fuse94;

            //                    public static AesCtx aes_kirk1 = new AesCtx();
            //                    public static byte[] PRNG_DATA = new byte[0x14];

            //                    public static char is_kirk_initialized;

            //                    /* Initialize the SHS values */


            //                    /* Message digest functions */

            //                    public static void SHAInit(SHA_CTX shsInfo)
            //                    {
            //                        endianTest(ref shsInfo.Endianness);
            //                        /* Set the h-vars to their initial values */
            //                        shsInfo.digest[0] = DefineConstants.h0init;
            //                        shsInfo.digest[1] = DefineConstants.h1init;
            //                        shsInfo.digest[2] = DefineConstants.h2init;
            //                        shsInfo.digest[3] = DefineConstants.h3init;
            //                        shsInfo.digest[4] = DefineConstants.h4init;

            //                        /* Initialise bit count */
            //                        shsInfo.countLo = shsInfo.countHi = 0;
            //                    }

            //                    /* Update SHS for a block of data */

            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'buffer', so pointers on this parameter are left unchanged:
            //                    public static void SHAUpdate(SHA_CTX shsInfo, byte buffer, int count)
            //                    {
            //                        uint tmp;
            //                        int dataCount;

            //                        /* Update bitcount */
            //                        tmp = shsInfo.countLo;
            //                        if ((shsInfo.countLo = tmp + ((uint)count << 3)) < tmp)
            //                        {
            //                            shsInfo.countHi++; // Carry from low to high
            //                        }
            //                        shsInfo.countHi += count >> 29;

            //                        /* Get count of bytes already in data */
            //                        dataCount = (int)(tmp >> 3) & 0x3F;

            //                        /* Handle any leading odd-sized chunks */
            //                        if (dataCount != 0)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte *p = (byte *) shsInfo->data + dataCount;
            //                            byte p = (byte)shsInfo.data + dataCount;

            //                            dataCount = DefineConstants.SHS_DATASIZE - dataCount;
            //                            if (count < dataCount)
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(p, buffer, count);
            //                                return;
            //                            }
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(p, buffer, dataCount);
            //                            longReverse(shsInfo.data, DefineConstants.SHS_DATASIZE, shsInfo.Endianness);
            //                            SHSTransform(shsInfo.digest, ref shsInfo.data);
            //                            buffer += dataCount;
            //                            count -= dataCount;
            //                        }

            //                        /* Process data in SHS_DATASIZE chunks */
            //                        while (count >= DefineConstants.SHS_DATASIZE)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy((byte)shsInfo.data, (byte)buffer, DefineConstants.SHS_DATASIZE);
            //                            longReverse(shsInfo.data, DefineConstants.SHS_DATASIZE, shsInfo.Endianness);
            //                            SHSTransform(shsInfo.digest, ref shsInfo.data);
            //                            buffer += DefineConstants.SHS_DATASIZE;
            //                            count -= DefineConstants.SHS_DATASIZE;
            //                        }

            //                        /* Handle any remaining bytes of data. */
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy((byte)shsInfo.data, (byte)buffer, count);
            //                    }

            //                    /* Final wrapup - pad to SHS_DATASIZE-byte boundary with the bit pattern
            //                       1 0* (64-bit count of bits processed, MSB-first) */

            //                    public static void SHAFinal(ref byte output, SHA_CTX shsInfo)
            //                    {
            //                        int count;
            //                        //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
            //                        byte* dataPtr;

            //                        /* Compute number of bytes mod 64 */
            //                        count = (int)shsInfo.countLo;
            //                        count = (count >> 3) & 0x3F;

            //                        /* Set the first char of padding to 0x80.  This is safe since there is
            //                           always at least one byte free */
            //                        dataPtr = (byte)shsInfo.data + count;
            //                        *dataPtr++ = 0x80;

            //                        /* Bytes of padding needed to make 64 bytes */
            //                        count = DefineConstants.SHS_DATASIZE - 1 - count;

            //                        /* Pad out to 56 mod 64 */
            //                        if (count < 8)
            //                        {
            //                            /* Two lots of padding:  Pad the first block to 64 bytes */
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(dataPtr, 0, count);
            //                            longReverse(shsInfo.data, DefineConstants.SHS_DATASIZE, shsInfo.Endianness);
            //                            SHSTransform(shsInfo.digest, ref shsInfo.data);

            //                            /* Now fill the next block with 56 bytes */
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset((byte)shsInfo.data, 0, DefineConstants.SHS_DATASIZE - 8);
            //                        }
            //                        else
            //                        {
            //                            /* Pad block to 56 bytes */
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(dataPtr, 0, count - 8);
            //                        }

            //                        /* Append length in bits and transform */
            //                        shsInfo.data[14] = shsInfo.countHi;
            //                        shsInfo.data[15] = shsInfo.countLo;

            //                        longReverse(shsInfo.data, DefineConstants.SHS_DATASIZE - 8, shsInfo.Endianness);
            //                        SHSTransform(shsInfo.digest, ref shsInfo.data);

            //                        /* Output to an array of bytes */
            //                        SHAtoByte(output, shsInfo.digest, DefineConstants.SHS_DIGESTSIZE);

            //                        /* Zeroise sensitive stuff */
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset((byte)shsInfo, 0, sizeof(SHA_CTX));
            //                    }

            //                    /* endian.c */



            //                    /* endian.h */


            //                    public static void endianTest(ref int endian_ness)
            //                    {
            //                        if (((ushort)("#S") >> 8) == '#')
            //                        {
            //                            /* printf("Big endian = no change\n"); */
            //                            endian_ness = !(0);
            //                        }
            //                        else
            //                        {
            //                            /* printf("Little endian = swap\n"); */
            //                            endian_ness = null;
            //                        }
            //                    }




            //                    internal static void SHAtoByte(byte[] output, uint[] input, uint len)
            //                    { // Output SHA digest in byte array
            //                        uint i;
            //                        uint j;

            //                        for (i = 0, j = 0; j < len; i++, j += 4)
            //                        {
            //                            output[j + 3] = (byte)(input[i] & 0xff);
            //                            output[j + 2] = (byte)((input[i] >> 8) & 0xff);
            //                            output[j + 1] = (byte)((input[i] >> 16) & 0xff);
            //                            output[j] = (byte)((input[i] >> 24) & 0xff);
            //                        }
            //                    }

            //                    /* The SHS block size and message digest sizes, in bytes */



            //                    /* The SHS f()-functions.  The f1 and f3 functions can be optimized to
            //                       save one boolean operation each - thanks to Rich Schroeppel,
            //                       rcs@cs.arizona.edu for discovering this */

            //                    /*#define f1(x,y,z) ( ( x & y ) | ( ~x & z ) )          // Rounds  0-19 */
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define f1(x,y,z) ( z ^ ( x & ( y ^ z ) ) )
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define f2(x,y,z) ( x ^ y ^ z )
            //                    /*#define f3(x,y,z) ( ( x & y ) | ( x & z ) | ( y & z ) )   // Rounds 40-59 */
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define f3(x,y,z) ( ( x & y ) | ( z & ( x | y ) ) )
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define f4(x,y,z) ( x ^ y ^ z )

            //                    /* The SHS Mysterious Constants */


            //                    /* SHS initial values */


            //                    /* Note that it may be necessary to add parentheses to these macros if they
            //                       are to be called with expressions as arguments */
            //                    /* 32-bit rotate left - kludged with shifts */

            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define ROTL(n,X) ( ( ( X ) << n ) | ( ( X ) >> ( 32 - n ) ) )

            //                    /* The initial expanding function.  The hash function is defined over an
            //                       80-UINT2 expanded input array W, where the first 16 are copies of the input
            //                       data, and the remaining 64 are defined by

            //                            W[ i ] = W[ i - 16 ] ^ W[ i - 14 ] ^ W[ i - 8 ] ^ W[ i - 3 ]

            //                       This implementation generates these values on the fly in a circular
            //                       buffer - thanks to Colin Plumb, colin@nyx10.cs.du.edu for this
            //                       optimization.

            //                       The updated SHS changes the expanding function by adding a rotate of 1
            //                       bit.  Thanks to Jim Gillogly, jim@rand.org, and an anonymous contributor
            //                       for this information */

            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define expand(W,i) ( W[ i & 15 ] = ROTL( 1, ( W[ i & 15 ] ^ W[ (i - 14) & 15 ] ^ W[ (i - 8) & 15 ] ^ W[ (i - 3) & 15 ] ) ) )


            //                    /* The prototype SHS sub-round.  The fundamental sub-round is:

            //                            a' = e + ROTL( 5, a ) + f( b, c, d ) + k + data;
            //                            b' = a;
            //                            c' = ROTL( 30, b );
            //                            d' = c;
            //                            e' = d;

            //                       but this is implemented by unrolling the loop 5 times and renaming the
            //                       variables ( e, a, b, c, d ) = ( a', b', c', d', e' ) each iteration.
            //                       This code is then replicated 20 times for each of the 4 functions, using
            //                       the next 20 values from the W[] array each time */

            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define subRound(a, b, c, d, e, f, k, data) ( e += ROTL( 5, a ) + f( b, c, d ) + k + data, b = ROTL( 30, b ) )


            //                    /* Perform the SHS transformation.  Note that this code, like MD5, seems to
            //                       break some optimizing compilers due to the complexity of the expressions
            //                       and the size of the basic block.  It may be necessary to split it into
            //                       sections, e.g. based on the four subrounds

            //                       Note that this corrupts the shsInfo->data area */

            //                    internal static void SHSTransform(uint[] digest, ref uint data)
            //                    {
            //                        uint A; // Local vars
            //                        uint B;
            //                        uint C;
            //                        uint D;
            //                        uint E;
            //                        uint[] eData = new uint[16]; // Expanded data

            //                        /* Set up first buffer and local data buffer */
            //                        A = digest[0];
            //                        B = digest[1];
            //                        C = digest[2];
            //                        D = digest[3];
            //                        E = digest[4];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy((byte)eData, (byte)data, DefineConstants.SHS_DATASIZE);

            //                        /* Heavy mangling, in 4 sub-rounds of 20 interations each. */
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (D ^ (B & (C ^ D))) + DefineConstants.K1 + eData[0], B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (C ^ (A & (B ^ C))) + DefineConstants.K1 + eData[1], A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (B ^ (E & (A ^ B))) + DefineConstants.K1 + eData[2], E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (A ^ (D & (E ^ A))) + DefineConstants.K1 + eData[3], D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (E ^ (C & (D ^ E))) + DefineConstants.K1 + eData[4], C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (D ^ (B & (C ^ D))) + DefineConstants.K1 + eData[5], B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (C ^ (A & (B ^ C))) + DefineConstants.K1 + eData[6], A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (B ^ (E & (A ^ B))) + DefineConstants.K1 + eData[7], E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (A ^ (D & (E ^ A))) + DefineConstants.K1 + eData[8], D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (E ^ (C & (D ^ E))) + DefineConstants.K1 + eData[9], C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (D ^ (B & (C ^ D))) + DefineConstants.K1 + eData[10], B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (C ^ (A & (B ^ C))) + DefineConstants.K1 + eData[11], A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (B ^ (E & (A ^ B))) + DefineConstants.K1 + eData[12], E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (A ^ (D & (E ^ A))) + DefineConstants.K1 + eData[13], D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (E ^ (C & (D ^ E))) + DefineConstants.K1 + eData[14], C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (D ^ (B & (C ^ D))) + DefineConstants.K1 + eData[15], B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (C ^ (A & (B ^ C))) + DefineConstants.K1 + (eData[16 & 15] = ((((eData[16 & 15] ^ eData[(16 - 14) & 15] ^ eData[(16 - 8) & 15] ^ eData[(16 - 3) & 15])) << 1) | (((eData[16 & 15] ^ eData[(16 - 14) & 15] ^ eData[(16 - 8) & 15] ^ eData[(16 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (B ^ (E & (A ^ B))) + DefineConstants.K1 + (eData[17 & 15] = ((((eData[17 & 15] ^ eData[(17 - 14) & 15] ^ eData[(17 - 8) & 15] ^ eData[(17 - 3) & 15])) << 1) | (((eData[17 & 15] ^ eData[(17 - 14) & 15] ^ eData[(17 - 8) & 15] ^ eData[(17 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (A ^ (D & (E ^ A))) + DefineConstants.K1 + (eData[18 & 15] = ((((eData[18 & 15] ^ eData[(18 - 14) & 15] ^ eData[(18 - 8) & 15] ^ eData[(18 - 3) & 15])) << 1) | (((eData[18 & 15] ^ eData[(18 - 14) & 15] ^ eData[(18 - 8) & 15] ^ eData[(18 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (E ^ (C & (D ^ E))) + DefineConstants.K1 + (eData[19 & 15] = ((((eData[19 & 15] ^ eData[(19 - 14) & 15] ^ eData[(19 - 8) & 15] ^ eData[(19 - 3) & 15])) << 1) | (((eData[19 & 15] ^ eData[(19 - 14) & 15] ^ eData[(19 - 8) & 15] ^ eData[(19 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));

            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K2 + (eData[20 & 15] = ((((eData[20 & 15] ^ eData[(20 - 14) & 15] ^ eData[(20 - 8) & 15] ^ eData[(20 - 3) & 15])) << 1) | (((eData[20 & 15] ^ eData[(20 - 14) & 15] ^ eData[(20 - 8) & 15] ^ eData[(20 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K2 + (eData[21 & 15] = ((((eData[21 & 15] ^ eData[(21 - 14) & 15] ^ eData[(21 - 8) & 15] ^ eData[(21 - 3) & 15])) << 1) | (((eData[21 & 15] ^ eData[(21 - 14) & 15] ^ eData[(21 - 8) & 15] ^ eData[(21 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K2 + (eData[22 & 15] = ((((eData[22 & 15] ^ eData[(22 - 14) & 15] ^ eData[(22 - 8) & 15] ^ eData[(22 - 3) & 15])) << 1) | (((eData[22 & 15] ^ eData[(22 - 14) & 15] ^ eData[(22 - 8) & 15] ^ eData[(22 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K2 + (eData[23 & 15] = ((((eData[23 & 15] ^ eData[(23 - 14) & 15] ^ eData[(23 - 8) & 15] ^ eData[(23 - 3) & 15])) << 1) | (((eData[23 & 15] ^ eData[(23 - 14) & 15] ^ eData[(23 - 8) & 15] ^ eData[(23 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K2 + (eData[24 & 15] = ((((eData[24 & 15] ^ eData[(24 - 14) & 15] ^ eData[(24 - 8) & 15] ^ eData[(24 - 3) & 15])) << 1) | (((eData[24 & 15] ^ eData[(24 - 14) & 15] ^ eData[(24 - 8) & 15] ^ eData[(24 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K2 + (eData[25 & 15] = ((((eData[25 & 15] ^ eData[(25 - 14) & 15] ^ eData[(25 - 8) & 15] ^ eData[(25 - 3) & 15])) << 1) | (((eData[25 & 15] ^ eData[(25 - 14) & 15] ^ eData[(25 - 8) & 15] ^ eData[(25 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K2 + (eData[26 & 15] = ((((eData[26 & 15] ^ eData[(26 - 14) & 15] ^ eData[(26 - 8) & 15] ^ eData[(26 - 3) & 15])) << 1) | (((eData[26 & 15] ^ eData[(26 - 14) & 15] ^ eData[(26 - 8) & 15] ^ eData[(26 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K2 + (eData[27 & 15] = ((((eData[27 & 15] ^ eData[(27 - 14) & 15] ^ eData[(27 - 8) & 15] ^ eData[(27 - 3) & 15])) << 1) | (((eData[27 & 15] ^ eData[(27 - 14) & 15] ^ eData[(27 - 8) & 15] ^ eData[(27 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K2 + (eData[28 & 15] = ((((eData[28 & 15] ^ eData[(28 - 14) & 15] ^ eData[(28 - 8) & 15] ^ eData[(28 - 3) & 15])) << 1) | (((eData[28 & 15] ^ eData[(28 - 14) & 15] ^ eData[(28 - 8) & 15] ^ eData[(28 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K2 + (eData[29 & 15] = ((((eData[29 & 15] ^ eData[(29 - 14) & 15] ^ eData[(29 - 8) & 15] ^ eData[(29 - 3) & 15])) << 1) | (((eData[29 & 15] ^ eData[(29 - 14) & 15] ^ eData[(29 - 8) & 15] ^ eData[(29 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K2 + (eData[30 & 15] = ((((eData[30 & 15] ^ eData[(30 - 14) & 15] ^ eData[(30 - 8) & 15] ^ eData[(30 - 3) & 15])) << 1) | (((eData[30 & 15] ^ eData[(30 - 14) & 15] ^ eData[(30 - 8) & 15] ^ eData[(30 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K2 + (eData[31 & 15] = ((((eData[31 & 15] ^ eData[(31 - 14) & 15] ^ eData[(31 - 8) & 15] ^ eData[(31 - 3) & 15])) << 1) | (((eData[31 & 15] ^ eData[(31 - 14) & 15] ^ eData[(31 - 8) & 15] ^ eData[(31 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K2 + (eData[32 & 15] = ((((eData[32 & 15] ^ eData[(32 - 14) & 15] ^ eData[(32 - 8) & 15] ^ eData[(32 - 3) & 15])) << 1) | (((eData[32 & 15] ^ eData[(32 - 14) & 15] ^ eData[(32 - 8) & 15] ^ eData[(32 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K2 + (eData[33 & 15] = ((((eData[33 & 15] ^ eData[(33 - 14) & 15] ^ eData[(33 - 8) & 15] ^ eData[(33 - 3) & 15])) << 1) | (((eData[33 & 15] ^ eData[(33 - 14) & 15] ^ eData[(33 - 8) & 15] ^ eData[(33 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K2 + (eData[34 & 15] = ((((eData[34 & 15] ^ eData[(34 - 14) & 15] ^ eData[(34 - 8) & 15] ^ eData[(34 - 3) & 15])) << 1) | (((eData[34 & 15] ^ eData[(34 - 14) & 15] ^ eData[(34 - 8) & 15] ^ eData[(34 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K2 + (eData[35 & 15] = ((((eData[35 & 15] ^ eData[(35 - 14) & 15] ^ eData[(35 - 8) & 15] ^ eData[(35 - 3) & 15])) << 1) | (((eData[35 & 15] ^ eData[(35 - 14) & 15] ^ eData[(35 - 8) & 15] ^ eData[(35 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K2 + (eData[36 & 15] = ((((eData[36 & 15] ^ eData[(36 - 14) & 15] ^ eData[(36 - 8) & 15] ^ eData[(36 - 3) & 15])) << 1) | (((eData[36 & 15] ^ eData[(36 - 14) & 15] ^ eData[(36 - 8) & 15] ^ eData[(36 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K2 + (eData[37 & 15] = ((((eData[37 & 15] ^ eData[(37 - 14) & 15] ^ eData[(37 - 8) & 15] ^ eData[(37 - 3) & 15])) << 1) | (((eData[37 & 15] ^ eData[(37 - 14) & 15] ^ eData[(37 - 8) & 15] ^ eData[(37 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K2 + (eData[38 & 15] = ((((eData[38 & 15] ^ eData[(38 - 14) & 15] ^ eData[(38 - 8) & 15] ^ eData[(38 - 3) & 15])) << 1) | (((eData[38 & 15] ^ eData[(38 - 14) & 15] ^ eData[(38 - 8) & 15] ^ eData[(38 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K2 + (eData[39 & 15] = ((((eData[39 & 15] ^ eData[(39 - 14) & 15] ^ eData[(39 - 8) & 15] ^ eData[(39 - 3) & 15])) << 1) | (((eData[39 & 15] ^ eData[(39 - 14) & 15] ^ eData[(39 - 8) & 15] ^ eData[(39 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));

            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + ((B & C) | (D & (B | C))) + DefineConstants.K3 + (eData[40 & 15] = ((((eData[40 & 15] ^ eData[(40 - 14) & 15] ^ eData[(40 - 8) & 15] ^ eData[(40 - 3) & 15])) << 1) | (((eData[40 & 15] ^ eData[(40 - 14) & 15] ^ eData[(40 - 8) & 15] ^ eData[(40 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + ((A & B) | (C & (A | B))) + DefineConstants.K3 + (eData[41 & 15] = ((((eData[41 & 15] ^ eData[(41 - 14) & 15] ^ eData[(41 - 8) & 15] ^ eData[(41 - 3) & 15])) << 1) | (((eData[41 & 15] ^ eData[(41 - 14) & 15] ^ eData[(41 - 8) & 15] ^ eData[(41 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + ((E & A) | (B & (E | A))) + DefineConstants.K3 + (eData[42 & 15] = ((((eData[42 & 15] ^ eData[(42 - 14) & 15] ^ eData[(42 - 8) & 15] ^ eData[(42 - 3) & 15])) << 1) | (((eData[42 & 15] ^ eData[(42 - 14) & 15] ^ eData[(42 - 8) & 15] ^ eData[(42 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + ((D & E) | (A & (D | E))) + DefineConstants.K3 + (eData[43 & 15] = ((((eData[43 & 15] ^ eData[(43 - 14) & 15] ^ eData[(43 - 8) & 15] ^ eData[(43 - 3) & 15])) << 1) | (((eData[43 & 15] ^ eData[(43 - 14) & 15] ^ eData[(43 - 8) & 15] ^ eData[(43 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + ((C & D) | (E & (C | D))) + DefineConstants.K3 + (eData[44 & 15] = ((((eData[44 & 15] ^ eData[(44 - 14) & 15] ^ eData[(44 - 8) & 15] ^ eData[(44 - 3) & 15])) << 1) | (((eData[44 & 15] ^ eData[(44 - 14) & 15] ^ eData[(44 - 8) & 15] ^ eData[(44 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + ((B & C) | (D & (B | C))) + DefineConstants.K3 + (eData[45 & 15] = ((((eData[45 & 15] ^ eData[(45 - 14) & 15] ^ eData[(45 - 8) & 15] ^ eData[(45 - 3) & 15])) << 1) | (((eData[45 & 15] ^ eData[(45 - 14) & 15] ^ eData[(45 - 8) & 15] ^ eData[(45 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + ((A & B) | (C & (A | B))) + DefineConstants.K3 + (eData[46 & 15] = ((((eData[46 & 15] ^ eData[(46 - 14) & 15] ^ eData[(46 - 8) & 15] ^ eData[(46 - 3) & 15])) << 1) | (((eData[46 & 15] ^ eData[(46 - 14) & 15] ^ eData[(46 - 8) & 15] ^ eData[(46 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + ((E & A) | (B & (E | A))) + DefineConstants.K3 + (eData[47 & 15] = ((((eData[47 & 15] ^ eData[(47 - 14) & 15] ^ eData[(47 - 8) & 15] ^ eData[(47 - 3) & 15])) << 1) | (((eData[47 & 15] ^ eData[(47 - 14) & 15] ^ eData[(47 - 8) & 15] ^ eData[(47 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + ((D & E) | (A & (D | E))) + DefineConstants.K3 + (eData[48 & 15] = ((((eData[48 & 15] ^ eData[(48 - 14) & 15] ^ eData[(48 - 8) & 15] ^ eData[(48 - 3) & 15])) << 1) | (((eData[48 & 15] ^ eData[(48 - 14) & 15] ^ eData[(48 - 8) & 15] ^ eData[(48 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + ((C & D) | (E & (C | D))) + DefineConstants.K3 + (eData[49 & 15] = ((((eData[49 & 15] ^ eData[(49 - 14) & 15] ^ eData[(49 - 8) & 15] ^ eData[(49 - 3) & 15])) << 1) | (((eData[49 & 15] ^ eData[(49 - 14) & 15] ^ eData[(49 - 8) & 15] ^ eData[(49 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + ((B & C) | (D & (B | C))) + DefineConstants.K3 + (eData[50 & 15] = ((((eData[50 & 15] ^ eData[(50 - 14) & 15] ^ eData[(50 - 8) & 15] ^ eData[(50 - 3) & 15])) << 1) | (((eData[50 & 15] ^ eData[(50 - 14) & 15] ^ eData[(50 - 8) & 15] ^ eData[(50 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + ((A & B) | (C & (A | B))) + DefineConstants.K3 + (eData[51 & 15] = ((((eData[51 & 15] ^ eData[(51 - 14) & 15] ^ eData[(51 - 8) & 15] ^ eData[(51 - 3) & 15])) << 1) | (((eData[51 & 15] ^ eData[(51 - 14) & 15] ^ eData[(51 - 8) & 15] ^ eData[(51 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + ((E & A) | (B & (E | A))) + DefineConstants.K3 + (eData[52 & 15] = ((((eData[52 & 15] ^ eData[(52 - 14) & 15] ^ eData[(52 - 8) & 15] ^ eData[(52 - 3) & 15])) << 1) | (((eData[52 & 15] ^ eData[(52 - 14) & 15] ^ eData[(52 - 8) & 15] ^ eData[(52 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + ((D & E) | (A & (D | E))) + DefineConstants.K3 + (eData[53 & 15] = ((((eData[53 & 15] ^ eData[(53 - 14) & 15] ^ eData[(53 - 8) & 15] ^ eData[(53 - 3) & 15])) << 1) | (((eData[53 & 15] ^ eData[(53 - 14) & 15] ^ eData[(53 - 8) & 15] ^ eData[(53 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + ((C & D) | (E & (C | D))) + DefineConstants.K3 + (eData[54 & 15] = ((((eData[54 & 15] ^ eData[(54 - 14) & 15] ^ eData[(54 - 8) & 15] ^ eData[(54 - 3) & 15])) << 1) | (((eData[54 & 15] ^ eData[(54 - 14) & 15] ^ eData[(54 - 8) & 15] ^ eData[(54 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + ((B & C) | (D & (B | C))) + DefineConstants.K3 + (eData[55 & 15] = ((((eData[55 & 15] ^ eData[(55 - 14) & 15] ^ eData[(55 - 8) & 15] ^ eData[(55 - 3) & 15])) << 1) | (((eData[55 & 15] ^ eData[(55 - 14) & 15] ^ eData[(55 - 8) & 15] ^ eData[(55 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + ((A & B) | (C & (A | B))) + DefineConstants.K3 + (eData[56 & 15] = ((((eData[56 & 15] ^ eData[(56 - 14) & 15] ^ eData[(56 - 8) & 15] ^ eData[(56 - 3) & 15])) << 1) | (((eData[56 & 15] ^ eData[(56 - 14) & 15] ^ eData[(56 - 8) & 15] ^ eData[(56 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + ((E & A) | (B & (E | A))) + DefineConstants.K3 + (eData[57 & 15] = ((((eData[57 & 15] ^ eData[(57 - 14) & 15] ^ eData[(57 - 8) & 15] ^ eData[(57 - 3) & 15])) << 1) | (((eData[57 & 15] ^ eData[(57 - 14) & 15] ^ eData[(57 - 8) & 15] ^ eData[(57 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + ((D & E) | (A & (D | E))) + DefineConstants.K3 + (eData[58 & 15] = ((((eData[58 & 15] ^ eData[(58 - 14) & 15] ^ eData[(58 - 8) & 15] ^ eData[(58 - 3) & 15])) << 1) | (((eData[58 & 15] ^ eData[(58 - 14) & 15] ^ eData[(58 - 8) & 15] ^ eData[(58 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + ((C & D) | (E & (C | D))) + DefineConstants.K3 + (eData[59 & 15] = ((((eData[59 & 15] ^ eData[(59 - 14) & 15] ^ eData[(59 - 8) & 15] ^ eData[(59 - 3) & 15])) << 1) | (((eData[59 & 15] ^ eData[(59 - 14) & 15] ^ eData[(59 - 8) & 15] ^ eData[(59 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));

            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K4 + (eData[60 & 15] = ((((eData[60 & 15] ^ eData[(60 - 14) & 15] ^ eData[(60 - 8) & 15] ^ eData[(60 - 3) & 15])) << 1) | (((eData[60 & 15] ^ eData[(60 - 14) & 15] ^ eData[(60 - 8) & 15] ^ eData[(60 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K4 + (eData[61 & 15] = ((((eData[61 & 15] ^ eData[(61 - 14) & 15] ^ eData[(61 - 8) & 15] ^ eData[(61 - 3) & 15])) << 1) | (((eData[61 & 15] ^ eData[(61 - 14) & 15] ^ eData[(61 - 8) & 15] ^ eData[(61 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K4 + (eData[62 & 15] = ((((eData[62 & 15] ^ eData[(62 - 14) & 15] ^ eData[(62 - 8) & 15] ^ eData[(62 - 3) & 15])) << 1) | (((eData[62 & 15] ^ eData[(62 - 14) & 15] ^ eData[(62 - 8) & 15] ^ eData[(62 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K4 + (eData[63 & 15] = ((((eData[63 & 15] ^ eData[(63 - 14) & 15] ^ eData[(63 - 8) & 15] ^ eData[(63 - 3) & 15])) << 1) | (((eData[63 & 15] ^ eData[(63 - 14) & 15] ^ eData[(63 - 8) & 15] ^ eData[(63 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K4 + (eData[64 & 15] = ((((eData[64 & 15] ^ eData[(64 - 14) & 15] ^ eData[(64 - 8) & 15] ^ eData[(64 - 3) & 15])) << 1) | (((eData[64 & 15] ^ eData[(64 - 14) & 15] ^ eData[(64 - 8) & 15] ^ eData[(64 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K4 + (eData[65 & 15] = ((((eData[65 & 15] ^ eData[(65 - 14) & 15] ^ eData[(65 - 8) & 15] ^ eData[(65 - 3) & 15])) << 1) | (((eData[65 & 15] ^ eData[(65 - 14) & 15] ^ eData[(65 - 8) & 15] ^ eData[(65 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K4 + (eData[66 & 15] = ((((eData[66 & 15] ^ eData[(66 - 14) & 15] ^ eData[(66 - 8) & 15] ^ eData[(66 - 3) & 15])) << 1) | (((eData[66 & 15] ^ eData[(66 - 14) & 15] ^ eData[(66 - 8) & 15] ^ eData[(66 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K4 + (eData[67 & 15] = ((((eData[67 & 15] ^ eData[(67 - 14) & 15] ^ eData[(67 - 8) & 15] ^ eData[(67 - 3) & 15])) << 1) | (((eData[67 & 15] ^ eData[(67 - 14) & 15] ^ eData[(67 - 8) & 15] ^ eData[(67 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K4 + (eData[68 & 15] = ((((eData[68 & 15] ^ eData[(68 - 14) & 15] ^ eData[(68 - 8) & 15] ^ eData[(68 - 3) & 15])) << 1) | (((eData[68 & 15] ^ eData[(68 - 14) & 15] ^ eData[(68 - 8) & 15] ^ eData[(68 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K4 + (eData[69 & 15] = ((((eData[69 & 15] ^ eData[(69 - 14) & 15] ^ eData[(69 - 8) & 15] ^ eData[(69 - 3) & 15])) << 1) | (((eData[69 & 15] ^ eData[(69 - 14) & 15] ^ eData[(69 - 8) & 15] ^ eData[(69 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K4 + (eData[70 & 15] = ((((eData[70 & 15] ^ eData[(70 - 14) & 15] ^ eData[(70 - 8) & 15] ^ eData[(70 - 3) & 15])) << 1) | (((eData[70 & 15] ^ eData[(70 - 14) & 15] ^ eData[(70 - 8) & 15] ^ eData[(70 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K4 + (eData[71 & 15] = ((((eData[71 & 15] ^ eData[(71 - 14) & 15] ^ eData[(71 - 8) & 15] ^ eData[(71 - 3) & 15])) << 1) | (((eData[71 & 15] ^ eData[(71 - 14) & 15] ^ eData[(71 - 8) & 15] ^ eData[(71 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K4 + (eData[72 & 15] = ((((eData[72 & 15] ^ eData[(72 - 14) & 15] ^ eData[(72 - 8) & 15] ^ eData[(72 - 3) & 15])) << 1) | (((eData[72 & 15] ^ eData[(72 - 14) & 15] ^ eData[(72 - 8) & 15] ^ eData[(72 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K4 + (eData[73 & 15] = ((((eData[73 & 15] ^ eData[(73 - 14) & 15] ^ eData[(73 - 8) & 15] ^ eData[(73 - 3) & 15])) << 1) | (((eData[73 & 15] ^ eData[(73 - 14) & 15] ^ eData[(73 - 8) & 15] ^ eData[(73 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K4 + (eData[74 & 15] = ((((eData[74 & 15] ^ eData[(74 - 14) & 15] ^ eData[(74 - 8) & 15] ^ eData[(74 - 3) & 15])) << 1) | (((eData[74 & 15] ^ eData[(74 - 14) & 15] ^ eData[(74 - 8) & 15] ^ eData[(74 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));
            //                        (E += (((A) << 5) | ((A) >> (32 - 5))) + (B ^ C ^ D) + DefineConstants.K4 + (eData[75 & 15] = ((((eData[75 & 15] ^ eData[(75 - 14) & 15] ^ eData[(75 - 8) & 15] ^ eData[(75 - 3) & 15])) << 1) | (((eData[75 & 15] ^ eData[(75 - 14) & 15] ^ eData[(75 - 8) & 15] ^ eData[(75 - 3) & 15])) >> (32 - 1)))), B = (((B) << 30) | ((B) >> (32 - 30))));
            //                        (D += (((E) << 5) | ((E) >> (32 - 5))) + (A ^ B ^ C) + DefineConstants.K4 + (eData[76 & 15] = ((((eData[76 & 15] ^ eData[(76 - 14) & 15] ^ eData[(76 - 8) & 15] ^ eData[(76 - 3) & 15])) << 1) | (((eData[76 & 15] ^ eData[(76 - 14) & 15] ^ eData[(76 - 8) & 15] ^ eData[(76 - 3) & 15])) >> (32 - 1)))), A = (((A) << 30) | ((A) >> (32 - 30))));
            //                        (C += (((D) << 5) | ((D) >> (32 - 5))) + (E ^ A ^ B) + DefineConstants.K4 + (eData[77 & 15] = ((((eData[77 & 15] ^ eData[(77 - 14) & 15] ^ eData[(77 - 8) & 15] ^ eData[(77 - 3) & 15])) << 1) | (((eData[77 & 15] ^ eData[(77 - 14) & 15] ^ eData[(77 - 8) & 15] ^ eData[(77 - 3) & 15])) >> (32 - 1)))), E = (((E) << 30) | ((E) >> (32 - 30))));
            //                        (B += (((C) << 5) | ((C) >> (32 - 5))) + (D ^ E ^ A) + DefineConstants.K4 + (eData[78 & 15] = ((((eData[78 & 15] ^ eData[(78 - 14) & 15] ^ eData[(78 - 8) & 15] ^ eData[(78 - 3) & 15])) << 1) | (((eData[78 & 15] ^ eData[(78 - 14) & 15] ^ eData[(78 - 8) & 15] ^ eData[(78 - 3) & 15])) >> (32 - 1)))), D = (((D) << 30) | ((D) >> (32 - 30))));
            //                        (A += (((B) << 5) | ((B) >> (32 - 5))) + (C ^ D ^ E) + DefineConstants.K4 + (eData[79 & 15] = ((((eData[79 & 15] ^ eData[(79 - 14) & 15] ^ eData[(79 - 8) & 15] ^ eData[(79 - 3) & 15])) << 1) | (((eData[79 & 15] ^ eData[(79 - 14) & 15] ^ eData[(79 - 8) & 15] ^ eData[(79 - 3) & 15])) >> (32 - 1)))), C = (((C) << 30) | ((C) >> (32 - 30))));

            //                        /* Build message digest */
            //                        digest[0] += A;
            //                        digest[1] += B;
            //                        digest[2] += C;
            //                        digest[3] += D;
            //                        digest[4] += E;
            //                    }

            //                    /* When run on a little-endian CPU we need to perform byte reversal on an
            //                       array of long words. */

            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'buffer', so pointers on this parameter are left unchanged:
            //                    internal static void longReverse(uint buffer, int byteCount, int Endianness)
            //                    {
            //                        uint value;

            //                        if (Endianness == (!DefineConstants.FALSE))
            //                        {
            //                            return;
            //                        }
            //                        byteCount /= sizeof(uint);
            //                        while (byteCount-- != 0)
            //                        {
            //                            value = buffer;
            //                            value = ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
            //                            *buffer++ = (value << 16) | (value >> 16);
            //                        }
            //                    }

            //                    // Copyright (C) 2013       tpu
            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt

            //                    // Copyright (C) 2013       tpu
            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt


            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define round_up(x,n) (-(-(x) & -(n)))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define array_size(x) (sizeof(x) / sizeof(*(x)))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define PF_RW (PF_R|PF_W)

            //                    internal static byte[] test_k140 = { 0x35, 0xfe, 0x4c, 0x96, 0x00, 0xb2, 0xf6, 0x7e, 0xf5, 0x83, 0xa6, 0x79, 0x1f, 0xa0, 0xe8, 0x86 };
            //                    internal static byte[] test_kirk1 = { 0xca, 0x03, 0x84, 0xb1, 0xd9, 0x63, 0x47, 0x92, 0xce, 0xc7, 0x01, 0x23, 0x43, 0x72, 0x68, 0xac, 0x77, 0xea, 0xec, 0xba, 0x6d, 0xaa, 0x97, 0xdf, 0xfe, 0x91, 0xb9, 0x39, 0x70, 0x99, 0x8b, 0x3a };

            //                    internal static TAG_KEY[] key_list =
            //                    {
            //        new TAG_KEY(0xd91609f0, new byte[] {0xD0,0x36,0x12,0x75,0x80,0x56,0x20,0x43,0xC4,0x30,0x94,0x3E,0x1C,0x75,0xD1,0xBF}, 0x5d, 2),

            //        new TAG_KEY(0xd9160af0, new byte[] { 0x10,0xA9,0xAC,0x16,0xAE,0x19,0xC0,0x7E,0x3B,0x60,0x77,0x86,0x01,0x6F,0xF2,0x63}, 0x5d, 2),
            //        new TAG_KEY(0xd9160bf0,  new byte[]{ 0x83,0x83,0xF1,0x37,0x53,0xD0,0xBE,0xFC,0x8D,0xA7,0x32,0x52,0x46,0x0A,0xC2,0xC2}, 0x5d, 2),
            //        new TAG_KEY(0xd91611f0,  new byte[]{ 0x61,0xB0,0xC0,0x58,0x71,0x57,0xD9,0xFA,0x74,0x67,0x0E,0x5C,0x7E,0x6E,0x95,0xB9}, 0x5d, 2),
            //        new TAG_KEY(0xd91612f0, new byte[] { 0x9e,0x20,0xe1,0xcd,0xd7,0x88,0xde,0xc0,0x31,0x9b,0x10,0xaf,0xc5,0xb8,0x73,0x23}, 0x5d, 2),
            //        new TAG_KEY(0xd91613f0,  new byte[]{ 0xEB,0xFF,0x40,0xD8,0xB4,0x1A,0xE1,0x66,0x91,0x3B,0x8F,0x64,0xB6,0xFC,0xB7,0x12}, 0x5d, 2),
            //        new TAG_KEY(0xd91614f0,  new byte[]{ 0xFD,0xF7,0xB7,0x3C,0x9F,0xD1,0x33,0x95,0x11,0xB8,0xB5,0xBB,0x54,0x23,0x73,0x85}, 0x5d, 2),
            //        new TAG_KEY(0xd91615f0, new byte[] { 0xC8,0x03,0xE3,0x44,0x50,0xF1,0xE7,0x2A,0x6A,0x0D,0xC3,0x61,0xB6,0x8E,0x5F,0x51}, 0x5d, 2),
            //        new TAG_KEY(0xd91624f0,  new byte[]{ 0x61,0xB7,0x26,0xAF,0x8B,0xF1,0x41,0x58,0x83,0x6A,0xC4,0x92,0x12,0xCB,0xB1,0xE9}, 0x5d, 2),
            //        new TAG_KEY(0xd91628f0,  new byte[]{ 0x49,0xA4,0xFC,0x66,0xDC,0xE7,0x62,0x21,0xDB,0x18,0xA7,0x50,0xD6,0xA8,0xC1,0xB6}, 0x5d, 2),
            //        new TAG_KEY(0xd91680f0,  new byte[]{ 0x2C,0x22,0x9B,0x12,0x36,0x74,0x11,0x67,0x49,0xD1,0xD1,0x88,0x92,0xF6,0xA1,0xD8}, 0x5d, 6),
            //        new TAG_KEY(0xd91681f0,  new byte[]{ 0x52,0xB6,0x36,0x6C,0x8C,0x46,0x7F,0x7A,0xCC,0x11,0x62,0x99,0xC1,0x99,0xBE,0x98}, 0x5d, 6)
            //                    };

            //                    /*
            //                        PSP EBOOT signing function.
            //                    */

            //                    public static int sign_eboot(ref byte[] eboot, int eboot_size, int tag, ref byte[] seboot)
            //                    {
            //                        PSP_Header2 psp_header = new PSP_Header2();

            //                        // Select tag.
            //                        tkey = key_list[tag];

            //                        // Allocate buffer for EBOOT data.
            //                        int esize = eboot_size;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *ebuf = (byte *) malloc(esize + 4096);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte[] ebuf = new byte[(esize + 4096) + 0x150];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        //memset(ebuf, 0, esize + 4096);
            //                         Array.Resize(ref (ebuf), (int)((esize + 4096) + 0x150));
            //                        // Read EBOOT data.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf, ref eboot, esize);

            //                        if ((uint)(ebuf.Length) != 0x464C457F)
            //                        {
            //                            Console.Write("ERROR: Invalid ELF file for EBOOT resigning!\n");
            //                            return -1;
            //                        }

            //                        Console.Write("Resigning EBOOT file with tag {0:X8}\n", tkey.tag);

            //                        // Build ~PSP header.
            //                        build_psp_header(psp_header, ref ebuf + 0x150, esize);

            //                        // Encrypt and sign data with KIRK1.
            //                        build_psp_kirk1(ebuf + 0x40, ref (byte)psp_header, esize);

            //                        // Generate PRX tag key.
            //                        build_tag_key(tkey);

            //                        // Hash the data.
            //                        build_psp_SHA1(ebuf, ref (byte)psp_header);

            //                        // Copy back the generated EBOOT.
            //                        esize = (esize + 15) & ~15;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(seboot, ebuf, esize + 0x150);

            //                        return (esize + 0x150);
            //                    }

            //                    public static void memcpy(byte[] val1, ref byte[] val2,int size)
            //                    {
            //                        Array.Copy(val1, val2, size);
            //                    }

            //                    public static TAG_KEY tkey;
            //                    public static byte[] tag_key = new byte[0x100];
            //                    public static string strtable;
            //                    public static int e_shnum;
            //                    public static Elf32_Shdr[] section;

            //                    /*
            //                        PSP header building functions.
            //                    */
            //                    public static Elf32_Shdr find_section(ref string name)
            //                    {
            //                        int i;

            //                        for (i = 0; i < e_shnum; i++)
            //                        {
            //                            if (string.Compare(name, strtable + section[i].sh_name) == 0)
            //                            {
            //                                return section[i];
            //                            }
            //                        }

            //                        return null;
            //                    }

            //                    public static void fix_reloc7(ref byte ebuf)
            //                    {
            //                        Elf32_Rel[] rel;
            //                        int i;
            //                        int j;
            //                        int count;

            //                        count = 0;
            //                        for (i = 0; i < e_shnum; i++)
            //                        {
            //                            if (section[i].sh_type == 0x700000A0)
            //                            {
            //                                rel = (Elf32_Rel)(ebuf + section[i].sh_offset);
            //                                for (j = 0; j < section[i].sh_size / sizeof(Elf32_Rel); j++)
            //                                {
            //                                    if ((rel[j].r_info & 0xFF) == 7)
            //                                    {
            //                                        rel[j].r_info = 0;
            //                                        count++;
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }

            //                    public static void build_psp_header(PSP_Header2 psph, ref byte[] ebuf, int esize)
            //                    {
            //                        Elf32_Ehdr elf;
            //                        Elf32_Shdr sh;
            //                        Elf32_Phdr ph;
            //                        SceModuleInfo modinfo;
            //                        int i;
            //                        int j;
            //                        int shtab_size;

            //                        elf = (Elf32_Ehdr)(ebuf);

            //                        section = (Elf32_Shdr)(ebuf + elf.e_shoff);
            //                        e_shnum = elf.e_shnum;

            //                        shtab_size = e_shnum * elf.e_shentsize;
            //                        if (elf.e_shoff + shtab_size > esize)
            //                        {
            //                            e_shnum = 0;
            //                        }
            //                        else
            //                        {
            //                            strtable = (string)(ebuf + section[elf.e_shstrndx].sh_offset);
            //                            fix_reloc7(ref ebuf);
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(psph, 0, sizeof(PSP_Header2));

            //                        psph.signature = 0x5053507E;
            //                        psph.mod_attribute = 0;
            //                        psph.comp_attribute = 0;
            //                        psph.module_ver_lo = 1;
            //                        psph.module_ver_hi = 1;
            //                        psph.mod_version = 1;
            //                        psph.devkit_version = 0x06020010;
            //                        psph.decrypt_mode = 9;
            //                        psph.overlap_size = 0;

            //                        psph.comp_size = esize;
            //                        psph._80 = 0x80;

            //                        psph.boot_entry = elf.e_entry;
            //                        psph.elf_size = esize;
            //                        psph.psp_size = ((esize + 15) & 0xfffffff0) + 0x150;

            //                        ph = (Elf32_Phdr)(ebuf + elf.e_phoff);
            //                        sh = find_section(".rodata.sceModuleInfo");

            //                        if (sh != null)
            //                        {
            //                            psph.modinfo_offset = sh.sh_offset;
            //                            modinfo = (SceModuleInfo)(ebuf + sh.sh_offset);
            //                        }
            //                        else
            //                        {
            //                            psph.modinfo_offset = ph[0].p_paddr;
            //                            modinfo = (SceModuleInfo)(ebuf + ph[0].p_paddr);
            //                        }

            //                        psph.modname = modinfo.modname;

            //                        j = 0;
            //                        for (i = 0; i < elf.e_phnum; i++)
            //                        {
            //                            if (ph[i].p_type == DefineConstants.PT_LOAD)
            //                            {
            //                                if (j > 3)
            //                                {
            //                                    Console.Write("ERROR: Too many EBOOT PH segments!\n");
            //                                    continue;
            //                                }
            //                                psph.seg_align[j] = ph[i].p_align;
            //                                psph.seg_address[j] = ph[i].p_vaddr;
            //                                psph.seg_size[j] = ph[i].p_memsz;
            //                                psph.bss_size = ph[i].p_memsz - ph[i].p_filesz;
            //                                j++;
            //                            }
            //                        }

            //                        psph.nsegments = j;
            //                    }

            //                    /*
            //                        PSP tag generating function.
            //                    */
            //                    public static void build_tag_key(TAG_KEY tk)
            //                    {
            //                        int i;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: uint *k7 = (uint*)tag_key;
            //                        uint k7 = (uint)tag_key;

            //                        for (i = 0; i < 9; i++)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(tag_key + 0x14 + (i * 16), tk.key, 0x10);
            //                            tag_key[0x14 + (i * 16)] = i;
            //                        }

            //                        k7[0] = DefineConstants.KIRK_MODE_DECRYPT_CBC;
            //                        k7[1] = 0;
            //                        k7[2] = 0;
            //                        k7[3] = tk.code;
            //                        k7[4] = 0x90;

            //                        kirk_CMD7(ref tag_key, ref tag_key, 0x90 + 0x14);
            //                    }

            //                    /*
            //                        PSP KIRK1 forging function.
            //                    */
            //                    public static void build_psp_kirk1(byte[] kbuf, ref byte pbuf, int esize)
            //                    {
            //                        KIRK_CMD1_HEADER k1 = (KIRK_CMD1_HEADER)kbuf;
            //                        int i;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf, test_kirk1, 32);

            //                        k1.mode = DefineConstants.KIRK_MODE_CMD1;
            //                        k1.data_size = esize;
            //                        k1.data_offset = 0x80;

            //                        if (tkey.type == 6)
            //                        {
            //                            k1.ecdsa_hash = 1;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(kbuf + 0x90, pbuf, 0x80);

            //                        if (esize % 16 != 0)
            //                        {
            //                            for (i = 0; i < (16 - (esize % 16)); i++)
            //                            {
            //                                kbuf[0x110 + esize + i] = 0xFF - i * 0x11;
            //                            }
            //                        }

            //                        kirk_CMD0(ref kbuf, ref kbuf, esize, 0);
            //                    }

            //                    /*
            //                        PSP SHA1 generating function.
            //                    */
            //                    public static void build_psp_SHA1(byte[] ebuf, ref byte pbuf)
            //                    {
            //                        byte[] tmp = new byte[0x150];
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: uint *k4 = (uint*)tmp;
            //                        uint k4 = (uint)tmp;
            //                        int i;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(tmp, 0, 0x150);

            //                        for (i = 0; i < 0x40; i++)
            //                        {
            //                            tmp[0x14 + i] = ebuf[0x40 + i] ^ tag_key[0x50 + i];
            //                        }
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp + 0xd0, pbuf, 0x80);

            //                        k4[0] = DefineConstants.KIRK_MODE_ENCRYPT_CBC;
            //                        k4[1] = 0;
            //                        k4[2] = 0;
            //                        k4[3] = tkey.code;
            //                        k4[4] = 0x40;
            //                        kirk_CMD4(ref tmp + 0x80 - 0x14, ref tmp, 0x40 + 0x14);

            //                        for (i = 0; i < 0x40; i++)
            //                        {
            //                            tmp[0x80 + i] ^= tag_key[0x10 + i];
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp + 0xd0, pbuf, 0x80);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp + 0xc0, pbuf + 0xb0, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp + 0x70, test_k140, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(tmp, 0, 0x70);

            //                        if (tkey.type == 6)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(tmp + 0x50, ebuf + 0x40 + 0x40, 0x20);
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp + 0x08, tag_key, 0x10);

            //                        k4[0] = 0x014c;
            //                        k4[1] = tkey.tag;

            //                        kirk_CMD11(ref tmp, ref tmp, 0x150);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp + 0x5c, test_k140, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(tmp + 0x6c, tmp, 0x14);

            //                        k4 = (uint)(tmp + 0x48);
            //                        k4[0] = DefineConstants.KIRK_MODE_ENCRYPT_CBC;
            //                        k4[1] = 0;
            //                        k4[2] = 0;
            //                        k4[3] = tkey.code;
            //                        k4[4] = 0x60;
            //                        kirk_CMD4(ref tmp + 0x48, ref tmp + 0x48, 0x60);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(tmp, 0, 0x5c);

            //                        if (tkey.type == 6)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(tmp + 0x3c, ebuf + 0x40 + 0x40, 0x20);
            //                        }

            //                        k4 = (uint)tmp;
            //                        k4[0] = tkey.tag;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf + 0x000, tmp + 0xd0, 0x80);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf + 0x080, tmp + 0x80, 0x30);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf + 0x0b0, tmp + 0xc0, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf + 0x0c0, tmp + 0xb0, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf + 0x0d0, tmp + 0x00, 0x5c);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf + 0x12c, tmp + 0x6c, 0x14);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(ebuf + 0x140, tmp + 0x5c, 0x10);
            //                    }

            //                    public static int isoOpen(string path)
            //                    {
            //                        int ret;

            //                        if (g_isofp != null)
            //                        {
            //                            isoClose();
            //                        }

            //                        g_filename = path;

            //                        if (reOpen() == null)
            //                        {
            //                            Console.Write("{0}: open failed {1}\n", __func__, g_filename);
            //                            ret = -2;
            //                            goto error;
            //                        }

            //                        fseek(g_isofp, 0, SEEK_SET);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(g_ciso_h, 0, sizeof(_CISOHeader));
            //                        ret = fread(g_ciso_h, sizeof(_CISOHeader), 1, g_isofp);
            //                        if (ret != 1)
            //                        {
            //                            ret = -9;
            //                            goto error;
            //                        }

            //                        if ((uint)g_ciso_h.magic == 0x4F534943 && g_ciso_h.block_size == DefineConstants.SECTOR_SIZE)
            //                        {
            //                            g_is_compressed = 1;
            //                        }
            //                        else
            //                        {
            //                            g_is_compressed = 0;
            //                        }

            //                        if (g_is_compressed != 0)
            //                        {
            //                            g_total_sectors = g_ciso_h.total_bytes / g_ciso_h.block_size;
            //                            g_CISO_cur_idx = -1;

            //                            if (g_ciso_dec_buf == null)
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                                g_ciso_dec_buf = malloc(DefineConstants.CISO_DEC_BUFFER_SIZE);

            //                                if (g_ciso_dec_buf == null)
            //                                {
            //                                    Console.Write("malloc -> 0x{0:x8}\n", (uint)g_ciso_dec_buf);
            //                                    ret = -6;
            //                                    goto error;
            //                                }
            //                            }

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(g_CISO_idx_cache, 0, sizeof(uint));
            //                            g_ciso_dec_buf_offset = -1;
            //                            g_CISO_cur_idx = -1;
            //                        }
            //                        else
            //                        {
            //                            g_total_sectors = isoGetSize();
            //                        }

            //                        ret = readSector(16, g_sector_buffer);

            //                        if (ret < 0)
            //                        {
            //                            ret = -7;
            //                            goto error;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
            //                        if (memcmp(g_sector_buffer[1], DefineConstants.ISO_STANDARD_ID, sizeof(DefineConstants.ISO_STANDARD_ID) - 1))
            //                        {
            //                            Console.Write("{0}: vol descriptor not found\n", __func__);
            //                            ret = -10;

            //                            goto error;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(g_root_record, g_sector_buffer[0x9C], sizeof(Iso9660DirectoryRecord));

            //                        return 0;

            //                        error:
            //                        if (g_isofp >= 0)
            //                        {
            //                            isoClose();
            //                        }

            //                        return ret;
            //                    }

            //                    public static void isoClose()
            //                    {
            //                        fclose(g_isofp);
            //                        g_isofp = null;
            //                        g_filename = null;

            //                        if (g_ciso_dec_buf != null)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                            free(g_ciso_dec_buf);
            //                            g_ciso_dec_buf = null;
            //                        }
            //                    }

            //                    public static int isoGetSize()
            //                    {
            //                        int ret;
            //                        int size;

            //                        ret = ftell(g_isofp);

            //                        fseek(g_isofp, 0, SEEK_END);
            //                        size = ftell(g_isofp);

            //                        fseek(g_isofp, ret, SEEK_SET);

            //                        return isoPos2LBA(size);
            //                    }

            //                    //get file information
            //                    public static int isoGetFileInfo(ref string path, ref uint filesize, ref uint lba)
            //                    {
            //                        int ret;
            //                        Iso9660DirectoryRecord rec = new Iso9660DirectoryRecord();

            //                        ret = findPath(path, rec);

            //                        if (ret < 0)
            //                        {
            //                            return ret;
            //                        }

            //                        lba = rec.lsbStart;

            //                        if (filesize != null)
            //                        {
            //                            filesize = rec.lsbDataLength;
            //                        }

            //                        return 0;
            //                    }

            //                    //read raw data from iso
            //                    public static int isoRead(object buffer, uint lba, int offset, uint size)
            //                    {
            //                        uint remaining;
            //                        uint pos;
            //                        uint copied;
            //                        uint re;
            //                        int ret;

            //                        remaining = size;
            //                        pos = isoLBA2Pos(lba, offset);
            //                        copied = 0;

            //                        while (remaining > 0)
            //                        {
            //                            ret = readSector(isoPos2LBA(pos), g_sector_buffer);

            //                            if (ret < 0)
            //                            {
            //                                break;
            //                            }

            //                            re = (((isoPos2RestSize(pos)) < (remaining)) ? (isoPos2RestSize(pos)) : (remaining));
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(buffer + copied, g_sector_buffer + isoPos2OffsetInSector(pos), re);
            //                            remaining -= re;
            //                            pos += re;
            //                            copied += re;
            //                        }

            //                        return copied;
            //                    }

            //                    internal static object g_ciso_dec_buf = null;
            //                    internal static uint[] g_CISO_idx_cache = new uint[DefineConstants.CISO_IDX_BUFFER_SIZE / 4];
            //                    internal static int g_ciso_dec_buf_offset = -1;
            //                    internal static _CISOHeader g_ciso_h = new _CISOHeader();
            //                    internal static int g_CISO_cur_idx = -1;

            //                    internal static string g_filename = null;
            //                    internal static string g_sector_buffer = new string(new char[DefineConstants.SECTOR_SIZE]);
            //                    internal static FileStream g_isofp = null;
            //                    internal static uint g_total_sectors = 0;
            //                    internal static uint g_is_compressed = 0;

            //                    internal static Iso9660DirectoryRecord g_root_record = new Iso9660DirectoryRecord();

            //                    internal static uint isoPos2LBA(uint pos)
            //                    {
            //                        return pos / DefineConstants.SECTOR_SIZE;
            //                    }

            //                    internal static uint isoLBA2Pos(uint lba, int offset)
            //                    {
            //                        return lba * DefineConstants.SECTOR_SIZE + offset;
            //                    }

            //                    internal static uint isoPos2OffsetInSector(uint pos)
            //                    {
            //                        return pos & (DefineConstants.SECTOR_SIZE - 1);
            //                    }

            //                    internal static uint isoPos2RestSize(uint pos)
            //                    {
            //                        return DefineConstants.SECTOR_SIZE - isoPos2OffsetInSector(pos);
            //                    }

            //                    internal static FileStream reOpen()
            //                    {
            //                        int retries = DefineConstants.MAX_RETRIES;
            //                        FILE fp = null;

            //                        if (g_isofp != null)
            //                        {
            //                            fclose(g_isofp);
            //                            g_isofp = null;
            //                        }

            //                        while (retries-- > 0)
            //                        {
            //                            fp = fopen(g_filename, "rb");
            //                            if (fp != null)
            //                            {
            //                                break;
            //                            }
            //                        }

            //                        if (fp >= 0)
            //                        {
            //                            g_isofp = fp;
            //                        }

            //                        return fp;
            //                    }

            //                    internal static int readRawData(object addr, uint size, int offset)
            //                    {
            //                        int ret;
            //                        int i;

            //                        for (i = 0; i < DefineConstants.MAX_RETRIES; ++i)
            //                        {
            //                            ret = fseek(g_isofp, offset, SEEK_SET);
            //                            if (ret >= 0)
            //                            {
            //                                break;
            //                            }
            //                            else
            //                            {
            //                                Console.Write("{0}: got error 0x{1:X8}, reOpening ISO: {2}\n", __func__, ret, g_filename);
            //                            }
            //                        }

            //                        for (i = 0; i < DefineConstants.MAX_RETRIES; ++i)
            //                        {
            //                            ret = fread(addr, size, 1, g_isofp);
            //                            if (ret >= 0)
            //                            {
            //                                break;
            //                            }
            //                            else
            //                            {
            //                                Console.Write("{0}: got error 0x{1:X8}, reOpening ISO: {2}\n", __func__, ret, g_filename);
            //                            }
            //                        }

            //                        return ret;
            //                    }

            //                    internal static int gzip_decompress(object dst, int dst_size, object src, int src_size)
            //                    {
            //                        z_stream strm = new z_stream();
            //                        int ret;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(strm, 0, sizeof(z_stream));

            //                        strm.next_in = src;
            //                        strm.avail_in = src_size;
            //                        strm.next_out = dst;
            //                        strm.avail_out = dst_size;

            //                        ret = inflateInit2(strm, -15);

            //                        if (ret != Z_OK)
            //                        {
            //                            return -1;
            //                        }

            //                        ret = inflate(strm, Z_FINISH);

            //                        if (ret != Z_STREAM_END)
            //                        {
            //                            return -2;
            //                        }

            //                        inflateEnd(strm);

            //                        return strm.total_out;
            //                    }

            //                    internal static int readSectorCompressed(int sector, object addr)
            //                    {
            //                        int ret;
            //                        int n_sector;
            //                        int offset;
            //                        int next_offset;
            //                        int size;

            //                        n_sector = sector - g_CISO_cur_idx;

            //                        // not within sector idx cache?
            //                        //C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
            //                        //ORIGINAL LINE: if (g_CISO_cur_idx == -1 || n_sector < 0 || n_sector >= (sizeof(g_CISO_idx_cache)/sizeof((g_CISO_idx_cache)[0])))
            //                        if (g_CISO_cur_idx == -1 || n_sector < 0 || n_sector >= (g_CISO_idx_cache.Length))
            //                        {
            //                            ret = readRawData(g_CISO_idx_cache, sizeof(uint), (sector << 2) + sizeof(_CISOHeader));

            //                            if (ret < 0)
            //                            {
            //                                return ret;
            //                            }

            //                            g_CISO_cur_idx = sector;
            //                            n_sector = 0;
            //                        }

            //                        offset = (g_CISO_idx_cache[n_sector] & 0x7FFFFFFF) << g_ciso_h.align;

            //                        // is uncompressed data?
            //                        if ((g_CISO_idx_cache[n_sector] & 0x80000000) != 0)
            //                        {
            //                            return readRawData(addr, DefineConstants.SECTOR_SIZE, offset);
            //                        }

            //                        sector++;
            //                        n_sector = sector - g_CISO_cur_idx;

            //                        //C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
            //                        //ORIGINAL LINE: if (g_CISO_cur_idx == -1 || n_sector < 0 || n_sector >= (sizeof(g_CISO_idx_cache)/sizeof((g_CISO_idx_cache)[0])))
            //                        if (g_CISO_cur_idx == -1 || n_sector < 0 || n_sector >= (g_CISO_idx_cache.Length))
            //                        {
            //                            ret = readRawData(g_CISO_idx_cache, sizeof(uint), (sector << 2) + sizeof(_CISOHeader));

            //                            if (ret < 0)
            //                            {
            //                                return ret;
            //                            }

            //                            g_CISO_cur_idx = sector;
            //                            n_sector = 0;
            //                        }

            //                        next_offset = (g_CISO_idx_cache[n_sector] & 0x7FFFFFFF) << g_ciso_h.align;
            //                        size = next_offset - offset;

            //                        if (size <= DefineConstants.SECTOR_SIZE)
            //                        {
            //                            size = DefineConstants.SECTOR_SIZE;
            //                        }

            //                        if (offset < g_ciso_dec_buf_offset || size + offset >= g_ciso_dec_buf_offset + DefineConstants.CISO_DEC_BUFFER_SIZE)
            //                        {
            //                            ret = readRawData(g_ciso_dec_buf, DefineConstants.CISO_DEC_BUFFER_SIZE, offset);

            //                            if (ret < 0)
            //                            {
            //                                g_ciso_dec_buf_offset = 0xFFF00000;

            //                                return ret;
            //                            }

            //                            g_ciso_dec_buf_offset = offset;
            //                        }

            //                        ret = gzip_decompress(addr, DefineConstants.SECTOR_SIZE, g_ciso_dec_buf + offset - g_ciso_dec_buf_offset, size);

            //                        return ret;
            //                    }

            //                    internal static int readSector(uint sector, object buf)
            //                    {
            //                        int ret;
            //                        uint pos;

            //                        if (g_is_compressed != 0)
            //                        {
            //                            ret = readSectorCompressed(sector, buf);
            //                        }
            //                        else
            //                        {
            //                            pos = isoLBA2Pos(sector, 0);
            //                            ret = readRawData(buf, DefineConstants.SECTOR_SIZE, pos);
            //                        }

            //                        return ret;
            //                    }

            //                    internal static void normalizeName(ref string filename)
            //                    {
            //                        string p;

            //                        p = StringFunctions.StrStr(filename, ";1");

            //                        if (p != null)
            //                        {
            //                            p = '\0';
            //                        }
            //                    }

            //                    internal static int findFile(string file, uint lba, uint dir_size, uint is_dir, Iso9660DirectoryRecord result_record)
            //                    {
            //                        uint pos;
            //                        int ret;
            //                        Iso9660DirectoryRecord rec;
            //                        string name = new string(new char[32]);
            //                        int re;

            //                        pos = isoLBA2Pos(lba, 0);
            //                        re = lba = 0;

            //                        while (re < dir_size)
            //                        {
            //                            if (isoPos2LBA(pos) != lba)
            //                            {
            //                                lba = isoPos2LBA(pos);
            //                                ret = readSector(lba, g_sector_buffer);

            //                                if (ret < 0)
            //                                {
            //                                    return ret;
            //                                }
            //                            }

            //                            rec = (Iso9660DirectoryRecord)g_sector_buffer[isoPos2OffsetInSector(pos)];

            //                            if (rec.len_dr == 0)
            //                            {
            //                                uint remaining;

            //                                remaining = isoPos2RestSize(pos);
            //                                pos += remaining;
            //                                re += remaining;
            //                                continue;
            //                            }

            //                            if (rec.len_dr < rec.len_fi + sizeof(Iso9660DirectoryRecord))
            //                            {
            //                                Console.Write("{0}: Corrupt directory record found in {1}, LBA {2:D}\n", __func__, g_filename, lba);

            //                                return -12;
            //                            }

            //                            if (rec.len_fi > 32)
            //                            {
            //                                return -11;
            //                            }

            //                            if (rec.len_fi == 1 && rec.fi == 0)
            //                            {
            //                                if (0 == string.Compare(file, "."))
            //                                {
            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                    memcpy(result_record, rec, sizeof(Iso9660DirectoryRecord));

            //                                    return 0;
            //                                }
            //                            }
            //                            else if (rec.len_fi == 1 && rec.fi == 1)
            //                            {
            //                                if (0 == string.Compare(file, ".."))
            //                                {
            //                                    // didn't support ..
            //                                    return -19;
            //                                }
            //                            }
            //                            else
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                                memset(name, 0, sizeof(char));
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(name, rec.fi, rec.len_fi);
            //                                normalizeName(ref name);

            //                                if (0 == string.Compare(name, file))
            //                                {
            //                                    if (is_dir != 0)
            //                                    {
            //                                        if (rec.fileFlags == 0 & DefineConstants.ISO9660_FILEFLAGS_DIR)
            //                                        {
            //                                            return -14;
            //                                        }
            //                                    }

            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                    memcpy(result_record, rec, sizeof(Iso9660DirectoryRecord));

            //                                    return 0;
            //                                }
            //                            }

            //                            pos += rec.len_dr;
            //                            re += rec.len_dr;
            //                        }

            //                        return -18;
            //                    }

            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'path', so pointers on this parameter are left unchanged:
            //                    internal static int findPath(char path, Iso9660DirectoryRecord result_record)
            //                    {
            //                        int level = 0;
            //                        int ret;
            //                        string cur_path;
            //                        string next;
            //                        uint lba;
            //                        uint dir_size;
            //                        string cur_dir = new string(new char[32]);

            //                        if (result_record == null)
            //                        {
            //                            return -17;
            //                        }

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(result_record, 0, sizeof(Iso9660DirectoryRecord));
            //                        lba = g_root_record.lsbStart;
            //                        dir_size = g_root_record.lsbDataLength;

            //                        cur_path = path;

            //                        while (cur_path == '/')
            //                        {
            //                            cur_path = cur_path.Substring(1);
            //                        }

            //                        next = StringFunctions.StrChr(cur_path, '/');

            //                        while (next != null)
            //                        {
            //                            if (next - cur_path >= sizeof(char))
            //                            {
            //                                return -15;
            //                            }

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(cur_dir, 0, sizeof(char));
            //                            cur_dir = cur_path.Substring(0, next - cur_path);
            //                            cur_dir = StringFunctions.ChangeCharacter(cur_dir, next - cur_path, '\0');

            //                            if (0 == string.Compare(cur_dir, "."))
            //                            {
            //                            }
            //                            else if (0 == string.Compare(cur_dir, ".."))
            //                            {
            //                                level--;
            //                            }
            //                            else
            //                            {
            //                                level++;
            //                            }

            //                            if (level > DefineConstants.MAX_DIR_LEVEL)
            //                            {
            //                                return -16;
            //                            }

            //                            ret = findFile(cur_dir, lba, dir_size, 1, result_record);

            //                            if (ret < 0)
            //                            {
            //                                return ret;
            //                            }

            //                            lba = result_record.lsbStart;
            //                            dir_size = result_record.lsbDataLength;

            //                            cur_path = next.Substring(1);

            //                            // skip unwant path separator
            //                            while (cur_path == '/')
            //                            {
            //                                cur_path = cur_path.Substring(1);
            //                            }

            //                            next = StringFunctions.StrChr(cur_path, '/');
            //                        }

            //                        ret = findFile(cur_path, lba, dir_size, 0, result_record);

            //                        return ret;
            //                    }
            //                    // Copyright (C) 2013       tpu
            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt

            //                    // Copyright (C) 2013       tpu
            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt


            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define round_up(x,n) (-(-(x) & -(n)))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define array_size(x) (sizeof(x) / sizeof(*(x)))

            //                    internal static byte[] dnas_key1A90 = { 0xED, 0xE2, 0x5D, 0x2D, 0xBB, 0xF8, 0x12, 0xE5, 0x3C, 0x5C, 0x59, 0x32, 0xFA, 0xE3, 0xE2, 0x43 };
            //                    internal static byte[] dnas_key1AA0 = { 0x27, 0x74, 0xFB, 0xEB, 0xA4, 0xA0, 0x01, 0xD7, 0x02, 0x56, 0x9E, 0x33, 0x8C, 0x19, 0x57, 0x83 };

            //                    /*
            //                        PGD encrypt function.
            //                    */

            //                    public static int encrypt_pgd(ref byte data, int data_size, int block_size, int key_index, int drm_type, int flag, ref byte key, ref byte pgd_data)
            //                    {
            //                        MAC_KEY mkey = new MAC_KEY();
            //                        CIPHER_KEY ckey = new CIPHER_KEY();

            //                        // Additional size variables.
            //                        int data_offset = 0x90;
            //                        int align_size = (data_size + 15) & ~15;
            //                        int table_offset = data_offset + align_size;
            //                        int block_nr = ((align_size + block_size - 1) & ~(block_size - 1)) / block_size;
            //                        int pgd_size = 0x90 + align_size + block_nr * 16;

            //                        // Build new PGD header.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte[] pgd = (byte)malloc(pgd_size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(pgd, 0, pgd_size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pgd + data_offset, data, data_size);

            //                        // Set magic PGD.
            //                        pgd[0] = 0x00;
            //                        pgd[1] = 0x50;
            //                        pgd[2] = 0x47;
            //                        pgd[3] = 0x44;

            //                        // Set key index and drm type.
            //                        (uint)(pgd + 4) = key_index;
            //                        (uint)(pgd + 8) = drm_type;

            //                        // Select the hashing, crypto and open modes.
            //                        int mac_type;
            //                        int cipher_type;
            //                        int open_flag = flag;
            //                        if (drm_type == 1)
            //                        {
            //                            mac_type = 1;
            //                            open_flag |= 4;
            //                            if (key_index > 1)
            //                            {
            //                                mac_type = 3;
            //                                open_flag |= 8;
            //                            }
            //                            cipher_type = 1;
            //                        }
            //                        else
            //                        {
            //                            mac_type = 2;
            //                            cipher_type = 2;
            //                        }

            //                        // Select the fixed DNAS key.
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte* fkey = null;
            //                        byte fkey = null;
            //                        if ((open_flag & 0x2) == 0x2)
            //                        {
            //                            fkey = dnas_key1A90;
            //                        }
            //                        if ((open_flag & 0x1) == 0x1)
            //                        {
            //                            fkey = dnas_key1AA0;
            //                        }

            //                        if (fkey == null)
            //                        {
            //                            Console.Write("PGD: Invalid PGD DNAS flag! {0:x8}\n", flag);
            //                            return -1;
            //                        }

            //                        // Set the decryption parameters in the decrypted header.
            //                        (uint)(pgd + 0x44) = data_size;
            //                        (uint)(pgd + 0x48) = block_size;
            //                        (uint)(pgd + 0x4C) = data_offset;

            //                        // Generate random header and data keys.
            //                        sceUtilsBufferCopyWithRange(ref pgd + 0x10, 0x30, 0, 0, DefineConstants.KIRK_CMD_PRNG);

            //                        // Encrypt the data.
            //                        sceDrmBBCipherInit(ckey, cipher_type, 2, ref pgd + 0x30, ref key, 0);
            //                        sceDrmBBCipherUpdate(ckey, ref pgd + data_offset, align_size);
            //                        sceDrmBBCipherFinal(ckey);

            //                        // Build data MAC hash.
            //                        int i;
            //                        for (i = 0; i < block_nr; i++)
            //                        {
            //                            int rsize = align_size - i * block_size;
            //                            if (rsize > block_size)
            //                            {
            //                                rsize = block_size;
            //                            }

            //                            sceDrmBBMacInit(mkey, mac_type);
            //                            sceDrmBBMacUpdate(mkey, ref pgd + data_offset + i * block_size, rsize);
            //                            sceDrmBBMacFinal(mkey, ref pgd + table_offset + i * 16, key);
            //                        }

            //                        // Build table MAC hash.
            //                        sceDrmBBMacInit(mkey, mac_type);
            //                        sceDrmBBMacUpdate(mkey, ref pgd + table_offset, block_nr * 16);
            //                        sceDrmBBMacFinal(mkey, ref pgd + 0x60, key);

            //                        // Encrypt the PGD header block (0x30 bytes).
            //                        sceDrmBBCipherInit(ckey, cipher_type, 2, ref pgd + 0x10, ref key, 0);
            //                        sceDrmBBCipherUpdate(ckey, ref pgd + 0x30, 0x30);
            //                        sceDrmBBCipherFinal(ckey);

            //                        // Build MAC hash at 0x70 (key hash).
            //                        sceDrmBBMacInit(mkey, mac_type);
            //                        sceDrmBBMacUpdate(mkey, ref pgd + 0x00, 0x70);
            //                        sceDrmBBMacFinal(mkey, ref pgd + 0x70, key);

            //                        // Build MAC hash at 0x80 (DNAS hash).
            //                        sceDrmBBMacInit(mkey, mac_type);
            //                        sceDrmBBMacUpdate(mkey, ref pgd + 0x00, 0x80);
            //                        sceDrmBBMacFinal(mkey, ref pgd + 0x80, fkey);

            //                        // Copy back the generated PGD file.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pgd_data, pgd, pgd_size);

            //                        return pgd_size;
            //                    }

            //                    /*
            //                        PGD decrypt function.
            //                    */
            //                    public static int decrypt_pgd(ref byte pgd_data, int pgd_size, int flag, ref byte key)
            //                    {
            //                        int result;
            //                        PGD_HEADER[] PGD = Arrays.InitializeWithDefaultInstances<PGD_HEADER>(sizeof(PGD_HEADER));
            //                        MAC_KEY mkey = new MAC_KEY();
            //                        CIPHER_KEY ckey = new CIPHER_KEY();
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte* fkey;
            //                        byte fkey;

            //                        // Read in the PGD header parameters.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(PGD, 0, sizeof(PGD_HEADER));

            //                        PGD.buf = pgd_data;
            //                        PGD.key_index = (uint)(pgd_data + 4);
            //                        PGD.drm_type = (uint)(pgd_data + 8);

            //                        // Set the hashing, crypto and open modes.
            //                        if (PGD.drm_type == 1)
            //                        {
            //                            PGD.mac_type = 1;
            //                            flag |= 4;

            //                            if (PGD.key_index > 1)
            //                            {
            //                                PGD.mac_type = 3;
            //                                flag |= 8;
            //                            }
            //                            PGD.cipher_type = 1;
            //                        }
            //                        else
            //                        {
            //                            PGD.mac_type = 2;
            //                            PGD.cipher_type = 2;
            //                        }
            //                        PGD.open_flag = flag;

            //                        // Get the fixed DNAS key.
            //                        fkey = null;
            //                        if ((flag & 0x2) == 0x2)
            //                        {
            //                            fkey = dnas_key1A90;
            //                        }
            //                        if ((flag & 0x1) == 0x1)
            //                        {
            //                            fkey = dnas_key1AA0;
            //                        }

            //                        if (fkey == null)
            //                        {
            //                            Console.Write("PGD: Invalid PGD DNAS flag! {0:x8}\n", flag);
            //                            return -1;
            //                        }

            //                        // Test MAC hash at 0x80 (DNAS hash).
            //                        sceDrmBBMacInit(mkey, PGD.mac_type);
            //                        sceDrmBBMacUpdate(mkey, ref pgd_data, 0x80);
            //                        result = sceDrmBBMacFinal2(mkey, ref pgd_data + 0x80, fkey);

            //                        if (result != 0)
            //                        {
            //                            Console.Write("PGD: Invalid PGD 0x80 MAC hash!\n");
            //                            return -1;
            //                        }

            //                        // Test MAC hash at 0x70 (key hash).
            //                        sceDrmBBMacInit(mkey, PGD.mac_type);
            //                        sceDrmBBMacUpdate(mkey, ref pgd_data, 0x70);

            //                        // If a key was provided, check it against MAC 0x70.
            //                        if (!isEmpty(ref key, 0x10))
            //                        {
            //                            result = sceDrmBBMacFinal2(mkey, ref pgd_data + 0x70, key);
            //                            if (result != 0)
            //                            {
            //                                Console.Write("PGD: Invalid PGD 0x70 MAC hash!\n");
            //                                return -1;
            //                            }
            //                            else
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(PGD.vkey, key, 16);
            //                            }
            //                        }
            //                        else
            //                        {
            //                            // Generate the key from MAC 0x70.
            //                            bbmac_getkey(mkey, ref pgd_data + 0x70, PGD.vkey);
            //                        }

            //                        // Decrypt the PGD header block (0x30 bytes).
            //                        sceDrmBBCipherInit(ckey, PGD.cipher_type, 2, ref pgd_data + 0x10, ref PGD.vkey, 0);
            //                        sceDrmBBCipherUpdate(ckey, ref pgd_data + 0x30, 0x30);
            //                        sceDrmBBCipherFinal(ckey);

            //                        // Get the decryption parameters from the decrypted header.
            //                        PGD.data_size = (uint)(pgd_data + 0x44);
            //                        PGD.block_size = (uint)(pgd_data + 0x48);
            //                        PGD.data_offset = (uint)(pgd_data + 0x4c);

            //                        // Additional size variables.
            //                        PGD.align_size = (PGD.data_size + 15) & ~15;
            //                        PGD.table_offset = PGD.data_offset + PGD.align_size;
            //                        PGD.block_nr = (PGD.align_size + PGD.block_size - 1) & ~(PGD.block_size - 1);
            //                        PGD.block_nr = PGD.block_nr / PGD.block_size;

            //                        if ((PGD.align_size + PGD.block_nr * 16) > pgd_size)
            //                        {
            //                            Console.Write("ERROR: Invalid PGD data size!\n");
            //                            return -1;
            //                        }

            //                        // Test MAC hash at 0x60 (table hash).
            //                        sceDrmBBMacInit(mkey, PGD.mac_type);
            //                        sceDrmBBMacUpdate(mkey, ref pgd_data + PGD.table_offset, PGD.block_nr * 16);
            //                        result = sceDrmBBMacFinal2(mkey, ref pgd_data + 0x60, PGD.vkey);

            //                        if (result != 0)
            //                        {
            //                            Console.Write("ERROR: Invalid PGD 0x60 MAC hash!\n");
            //                            return -1;
            //                        }

            //                        // Decrypt the data.
            //                        sceDrmBBCipherInit(ckey, PGD.cipher_type, 2, ref pgd_data + 0x30, ref PGD.vkey, 0);
            //                        sceDrmBBCipherUpdate(ckey, ref pgd_data + 0x90, PGD.align_size);
            //                        sceDrmBBCipherFinal(ckey);

            //                        return PGD.data_size;
            //                    }
            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt

            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt


            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define round_up(x,n) (-(-(x) & -(n)))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define array_size(x) (sizeof(x) / sizeof(*(x)))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define NELEMS(x) (sizeof(x)/sizeof((x)[0]))
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define MIN(a,b) ( ((a)<(b)) ? (a) : (b) )
            //                    //C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
            //                    //ORIGINAL LINE: #define PF_RW (PF_R|PF_W)


            //                    internal static byte[] npumdimg_private_key = { 0x14, 0xB0, 0x22, 0xE8, 0x92, 0xCF, 0x86, 0x14, 0xA4, 0x45, 0x57, 0xDB, 0x09, 0x5C, 0x92, 0x8D, 0xE9, 0xB8, 0x99, 0x70 };
            //                    internal static byte[] npumdimg_public_key = { 0x01, 0x21, 0xEA, 0x6E, 0xCD, 0xB2, 0x3A, 0x3E, 0x23, 0x75, 0x67, 0x1C, 0x53, 0x62, 0xE8, 0xE2, 0x8B, 0x1E, 0x78, 0x3B, 0x1A, 0x27, 0x32, 0x15, 0x8B, 0x8C, 0xED, 0x98, 0x46, 0x6C, 0x18, 0xA3, 0xAC, 0x3B, 0x11, 0x06, 0xAF, 0xB4, 0xEC, 0x3B };

            //                    public static byte[] ReadFully(Stream input)
            //                    {
            //                        byte[] buffer = new byte[16 * 1024];
            //                        using (MemoryStream ms = new MemoryStream())
            //                        {
            //                            int read;
            //                            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            //                            {
            //                                ms.Write(buffer, 0, read);
            //                            }
            //                            return ms.ToArray();
            //                        }
            //                    }


            //                    public static byte[] load_file_from_ISO(string iso, string name, int size)
            //                    {
            //                        using (FileStream isoStream = File.Open(iso, FileMode.Open))
            //                        {
            //                            CDReader cd = new CDReader(isoStream, true);
            //                            Stream fileStream = cd.OpenFile(name, FileMode.Open);
            //                            // Use fileStream...

            //                            return ReadFully(fileStream);
            //                        }
            //                    }

            //                    public static int sfo_get_key(ref byte sfo_buf, ref string name, object value)
            //                    {
            //                        int i;
            //                        int offset;
            //                        SFO_Header sfo = (SFO_Header)sfo_buf;
            //                        SFO_Entry[] sfo_keys = (SFO_Entry)(sfo_buf + 0x14);

            //                        if (sfo.magic != DefineConstants.PSF_MAGIC)
            //                        {
            //                            return -1;
            //                        }

            //                        for (i = 0; i < sfo.key_count; i++)
            //                        {
            //                            offset = sfo_keys[i].name_offset;
            //                            offset += sfo.key_offset;

            //                            if (string.Compare((string)sfo_buf + offset, name) == 0)
            //                            {
            //                                offset = sfo_keys[i].data_offset;
            //                                offset += sfo.val_offset;
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(value, sfo_buf + offset, sfo_keys[i].val_size);
            //                                return sfo_keys[i].val_size;
            //                            }
            //                        }

            //                        return -1;
            //                    }

            //                    public static int sfo_put_key(ref byte sfo_buf, ref string name, object value)
            //                    {
            //                        //int i;
            //                        //int offset;
            //                        //SFO_Header sfo = (SFO_Header)sfo_buf;
            //                        //SFO_Entry[] sfo_keys = (SFO_Entry)(sfo_buf + 0x14);

            //                        //if (sfo.magic != DefineConstants.PSF_MAGIC)
            //                        //{
            //                        //    return -1;
            //                        //}

            //                        //for (i = 0; i < sfo.key_count; i++)
            //                        //{
            //                        //    offset = sfo_keys[i].name_offset;
            //                        //    offset += sfo.key_offset;

            //                        //    if (string.Compare((string)sfo_buf + offset, name) == 0)
            //                        //    {
            //                        //        offset = sfo_keys[i].data_offset;
            //                        //        offset += sfo.val_offset;
            //                        //        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        //        memcpy(sfo_buf + offset, value, sfo_keys[i].val_size);
            //                        //        return 0;
            //                        //    }
            //                        //}

            //                        return -1;
            //                    }

            //                    public static void encrypt_table(ref byte table)
            //                    {
            //                        uint[] p = (uint)table;
            //                        uint k0;
            //                        uint k1;
            //                        uint k2;
            //                        uint k3;

            //                        k0 = p[0] ^ p[1];
            //                        k1 = p[1] ^ p[2];
            //                        k2 = p[0] ^ p[3];
            //                        k3 = p[2] ^ p[3];

            //                        p[4] ^= k3;
            //                        p[5] ^= k1;
            //                        p[6] ^= k2;
            //                        p[7] ^= k0;
            //                    }

            //                    public static NPUMDIMG_HEADER forge_npumdimg(int iso_size, int iso_blocks, int block_basis, ref string content_id, int np_flags, ref byte version_key, ref byte header_key, ref byte data_key)
            //                    {
            //                        // Build NPUMDIMG header.
            //                        NPUMDIMG_HEADER np_header = new NPUMDIMG_HEADER();
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(np_header, 0, sizeof(NPUMDIMG_HEADER));

            //                        // Set magic NPUMDIMG.
            //                        np_header.magic[0] = 0x4E;
            //                        np_header.magic[1] = 0x50;
            //                        np_header.magic[2] = 0x55;
            //                        np_header.magic[3] = 0x4D;
            //                        np_header.magic[4] = 0x44;
            //                        np_header.magic[5] = 0x49;
            //                        np_header.magic[6] = 0x4D;
            //                        np_header.magic[7] = 0x47;

            //                        // Set flags and block basis.
            //                        np_header.np_flags = np_flags;
            //                        np_header.block_basis = block_basis;

            //                        // Set content ID.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(np_header.content_id, content_id, content_id.Length);

            //                        // Set inner body parameters.
            //                        np_header.body.sector_size = 0x800;

            //                        if (iso_size > 0x40000000)
            //                        {
            //                            np_header.body.unk_2 = 0xE001;
            //                        }
            //                        else
            //                        {
            //                            np_header.body.unk_2 = 0xE000;
            //                        }

            //                        np_header.body.unk_4 = 0x0;
            //                        np_header.body.unk_8 = 0x1010;
            //                        np_header.body.unk_12 = 0x0;
            //                        np_header.body.unk_16 = 0x0;
            //                        np_header.body.lba_start = 0x0;
            //                        np_header.body.unk_24 = 0x0;

            //                        if (((iso_blocks * block_basis) - 1) > 0x6C0BF)
            //                        {
            //                            np_header.body.nsectors = 0x6C0BF;
            //                        }
            //                        else
            //                        {
            //                            np_header.body.nsectors = (iso_blocks * block_basis) - 1;
            //                        }

            //                        np_header.body.unk_32 = 0x0;
            //                        np_header.body.lba_end = (iso_blocks * block_basis) - 1;
            //                        np_header.body.unk_40 = 0x01003FFE;
            //                        np_header.body.block_entry_offset = 0x100;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(np_header.body.disc_id, content_id.Substring(7), 4);
            //                        np_header.body.disc_id = StringFunctions.ChangeCharacter(np_header.body.disc_id, 4, '-');
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(np_header.body.disc_id.Substring(5), content_id.Substring(11), 5);

            //                        np_header.body.header_start_offset = 0x0;
            //                        np_header.body.unk_68 = 0x0;
            //                        np_header.body.unk_72 = 0x0;
            //                        np_header.body.bbmac_param = 0x0;

            //                        np_header.body.unk_74 = 0x0;
            //                        np_header.body.unk_75 = 0x0;
            //                        np_header.body.unk_76 = 0x0;
            //                        np_header.body.unk_80 = 0x0;
            //                        np_header.body.unk_84 = 0x0;
            //                        np_header.body.unk_88 = 0x0;
            //                        np_header.body.unk_92 = 0x0;

            //                        // Set keys.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(np_header.header_key, 0, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(np_header.data_key, 0, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(np_header.header_hash, 0, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(np_header.padding, 0, 0x8);

            //                        // Copy header and data keys.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(np_header.header_key, header_key, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(np_header.data_key, data_key, 0x10);

            //                        // Generate random padding.
            //                        sceUtilsBufferCopyWithRange(ref np_header.padding, 0x8, 0, 0, DefineConstants.KIRK_CMD_PRNG);

            //                        // Prepare buffers to encrypt the NPUMDIMG body.
            //                        MAC_KEY mck = new MAC_KEY();
            //                        CIPHER_KEY bck = new CIPHER_KEY();

            //                        // Encrypt NPUMDIMG body.
            //                        sceDrmBBCipherInit(bck, 1, 2, ref np_header.header_key, ref version_key, 0);
            //                        sceDrmBBCipherUpdate(bck, ref (byte)(np_header) + 0x40, 0x60);
            //                        sceDrmBBCipherFinal(bck);

            //                        // Generate header hash.
            //                        sceDrmBBMacInit(mck, 3);
            //                        sceDrmBBMacUpdate(mck, ref (byte)np_header, 0xC0);
            //                        sceDrmBBMacFinal(mck, ref np_header.header_hash, ref version_key);
            //                        bbmac_build_final2(3, ref np_header.header_hash);

            //                        // Prepare the signature hash input buffer.
            //                        byte[] npumdimg_sha1_inbuf = new byte[0xD8 + 0x4];
            //                        byte[] npumdimg_sha1_outbuf = new byte[0x14];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(npumdimg_sha1_inbuf, 0, 0xD8 + 0x4);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(npumdimg_sha1_outbuf, 0, 0x14);

            //                        // Set SHA1 data size.
            //                        npumdimg_sha1_inbuf[0] = 0xD8;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(npumdimg_sha1_inbuf + 0x4, (byte)np_header, 0xD8);

            //                        // Hash the input buffer.
            //                        if (sceUtilsBufferCopyWithRange(ref npumdimg_sha1_outbuf, 0x14, ref npumdimg_sha1_inbuf, 0xD8 + 0x4, DefineConstants.KIRK_CMD_SHA1_HASH) != 0)
            //                        {
            //                            Console.Write("ERROR: Failed to generate SHA1 hash for NPUMDIMG header!\n");
            //                            return null;
            //                        }

            //                        // Prepare ECDSA signature buffer.
            //                        byte[] npumdimg_sign_buf_in = new byte[0x34];
            //                        byte[] npumdimg_sign_buf_out = new byte[0x28];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(npumdimg_sign_buf_in, 0, 0x34);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(npumdimg_sign_buf_out, 0, 0x28);

            //                        // Create ECDSA key pair.
            //                        byte[] npumdimg_keypair = new byte[0x3C];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(npumdimg_keypair, npumdimg_private_key, 0x14);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(npumdimg_keypair + 0x14, npumdimg_public_key, 0x28);

            //                        // Encrypt NPUMDIMG private key.
            //                        byte[] npumdimg_private_key_enc = new byte[0x20];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(npumdimg_private_key_enc, 0, 0x20);
            //                        encrypt_kirk16_private(ref npumdimg_private_key_enc, ref npumdimg_keypair);

            //                        // Generate ECDSA signature.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(npumdimg_sign_buf_in, npumdimg_private_key_enc, 0x20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(npumdimg_sign_buf_in + 0x20, npumdimg_sha1_outbuf, 0x14);
            //                        if (sceUtilsBufferCopyWithRange(ref npumdimg_sign_buf_out, 0x28, ref npumdimg_sign_buf_in, 0x34, DefineConstants.KIRK_CMD_ECDSA_SIGN) != 0)
            //                        {
            //                            Console.Write("ERROR: Failed to generate ECDSA signature for NPUMDIMG header!\n");
            //                            return null;
            //                        }

            //                        // Verify the generated ECDSA signature.
            //                        byte[] test_npumdimg_sign = new byte[0x64];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(test_npumdimg_sign, npumdimg_public_key, 0x28);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(test_npumdimg_sign + 0x28, npumdimg_sha1_outbuf, 0x14);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(test_npumdimg_sign + 0x3C, npumdimg_sign_buf_out, 0x28);
            //                        if (sceUtilsBufferCopyWithRange(0, 0, ref test_npumdimg_sign, 0x64, DefineConstants.KIRK_CMD_ECDSA_VERIFY) != 0)
            //                        {
            //                            Console.Write("ERROR: ECDSA signature for NPUMDIMG header is invalid!\n");
            //                            return null;
            //                        }
            //                        else
            //                        {
            //                            Console.Write("ECDSA signature for NPUMDIMG header is valid!\n");
            //                        }

            //                        // Store the signature.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(np_header.ecdsa_sig, npumdimg_sign_buf_out, 0x28);

            //                        return np_header;
            //                    }

            //                    public static int write_pbp(FileStream f, ref string iso_name, ref string content_id, int np_flags, ref byte startdat_buf, int startdat_size, ref byte pgd_buf, int pgd_size)
            //                    {
            //                        // Get all data files.
            //                        int param_sfo_size = 0;
            //                        int icon0_size = 0;
            //                        int icon1_size = 0;
            //                        int pic0_size = 0;
            //                        int pic1_size = 0;
            //                        int snd0_size = 0;



            //                        Param_SFO.PARAM_SFO param_sfo = new Param_SFO.PARAM_SFO(load_file_from_ISO(iso_name, @"PSP_GAME\PARAM.SFO", param_sfo_size));
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *icon0_buf = load_file_from_ISO(iso_name, "/PSP_GAME/ICON0.PNG", &icon0_size);
            //                        byte[] icon0_buf = load_file_from_ISO(iso_name, @"PSP_GAME\ICON0.PNG", icon0_size);
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *icon1_buf = load_file_from_ISO(iso_name, "/PSP_GAME/ICON1.PMF",&icon1_size);
            //                        byte[] icon1_buf = load_file_from_ISO(iso_name, @"PSP_GAME\ICON1.PMF", icon1_size);
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *pic0_buf = load_file_from_ISO(iso_name, "/PSP_GAME/PIC0.PNG", &pic0_size);
            //                        byte[] pic0_buf = load_file_from_ISO(iso_name, @"PSP_GAME\PIC0.PNG", pic0_size);
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *pic1_buf = load_file_from_ISO(iso_name, "/PSP_GAME/PIC1.PNG", &pic1_size);
            //                        byte[] pic1_buf = load_file_from_ISO(iso_name, @"PSP_GAME\PIC1.PNG", pic1_size);
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *snd0_buf = load_file_from_ISO(iso_name, "/PSP_GAME/SND0.AT3", &snd0_size);
            //                        byte[] snd0_buf = load_file_from_ISO(iso_name, @"PSP_GAME\SND0.AT3", snd0_size);

            //                        // Get system version from PARAM.SFO.
            //                        byte[] sys_ver = new byte[0x4];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:


            //                        for (int i = 0; i < param_sfo.Tables.Count; i++)
            //                        {
            //                            if (param_sfo.Tables[i].Name == "PSP_SYSTEM_VER")
            //                            {
            //                                //get the value 
            //                                sys_ver = param_sfo.Tables[i].ValueBuffer;
            //                            }
            //                        }

            //                        Console.Write("PSP_SYSTEM_VER: {0}\n\n", sys_ver);

            //                        // Change disc ID in PARAM.SFO.
            //                        byte[] disc_id = new byte[0x10];

            //                        for (int i = 0; i < param_sfo.Tables.Count; i++)
            //                        {
            //                            if (param_sfo.Tables[i].Name == "DISC_ID")
            //                            {
            //                                //get the value 
            //                                disc_id = param_sfo.Tables[i].ValueBuffer;
            //                            }
            //                        }
            //                        Console.Write("DISC_ID: {0}\n\n", disc_id);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        //memset(disc_id, 0, 0x10);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        // memcpy(disc_id, content_id.Substring(0x7), 0x9);
            //                        // sfo_put_key(ref param_sfo_buf, "DISC_ID", disc_id);

            //                        // Change category in PARAM.SFO.
            //                        //sfo_put_key(ref param_sfo_buf, "CATEGORY", "EG");
            //                        //param_sfo.Category = EG;
            //                        for (int i = 0; i < param_sfo.Tables.Count; i++)
            //                        {
            //                            if (param_sfo.Tables[i].Name == "CATEGORY")
            //                            {
            //                                var tempitem = param_sfo.Tables[i];
            //                                tempitem.Value = "EG";
            //                                param_sfo.Tables[i] = tempitem;
            //                            }
            //                        }

            //                        Console.Write("CATEGORY: {0}\n\n", param_sfo.Category);


            //                        // Build DATA.PSP (content ID + flags).
            //                        Console.Write("Building DATA.PSP...\n");
            //                        int data_psp_size = 0x594 + ((startdat_size) != 0 ? startdat_size + 0xC : 0) + pgd_size;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *data_psp_buf = (byte *) malloc(data_psp_size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte[] data_psp_buf = new byte[data_psp_size];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        //memset(data_psp_buf, 0, data_psp_size);

            //                        //write into buffer 


            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy (data_psp_buf + 0x560, content_id, content_id.Length);
            //                        (uint)(data_psp_buf + 0x590) = se32((uint)np_flags);

            //                        // DATA.PSP contains PARAM.SFO signature.
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *data_psp_param_buf = (byte *) malloc(param_sfo_size + 0x30);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte data_psp_param_buf = (byte)malloc(param_sfo_size + 0x30);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(data_psp_param_buf, 0, param_sfo_size + 0x30);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_param_buf, param_sfo_buf, param_sfo_size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_param_buf + param_sfo_size, data_psp_buf + 0x560, 0x30);

            //                        // Prepare the signature hash input buffer.
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *data_psp_sha1_inbuf = (byte *) malloc(param_sfo_size + 0x30 + 0x4);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte data_psp_sha1_inbuf = (byte)malloc(param_sfo_size + 0x30 + 0x4);
            //                        byte[] data_psp_sha1_outbuf = new byte[0x14];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(data_psp_sha1_inbuf, 0, param_sfo_size + 0x30 + 0x4);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(data_psp_sha1_outbuf, 0, 0x14);

            //                        // Set SHA1 data size.
            //                        (uint)data_psp_sha1_inbuf = param_sfo_size + 0x30;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_sha1_inbuf + 0x4, (byte)data_psp_param_buf, param_sfo_size + 0x30);

            //                        // Hash the input buffer.
            //                        if (sceUtilsBufferCopyWithRange(ref data_psp_sha1_outbuf, 0x14, ref data_psp_sha1_inbuf, param_sfo_size + 0x30 + 0x4, DefineConstants.KIRK_CMD_SHA1_HASH) != 0)
            //                        {
            //                            Console.Write("ERROR: Failed to generate SHA1 hash for DATA.PSP!\n");
            //                            return 0;
            //                        }

            //                        // Prepare ECDSA signature buffer.
            //                        byte[] data_psp_sign_buf_in = new byte[0x34];
            //                        byte[] data_psp_sign_buf_out = new byte[0x28];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(data_psp_sign_buf_in, 0, 0x34);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(data_psp_sign_buf_out, 0, 0x28);

            //                        // Create ECDSA key pair.
            //                        byte[] data_psp_keypair = new byte[0x3C];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_keypair, npumdimg_private_key, 0x14);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_keypair + 0x14, npumdimg_public_key, 0x28);

            //                        // Encrypt NPUMDIMG private key.
            //                        byte[] data_psp_private_key_enc = new byte[0x20];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(data_psp_private_key_enc, 0, 0x20);
            //                        encrypt_kirk16_private(ref data_psp_private_key_enc, ref data_psp_keypair);

            //                        // Generate ECDSA signature.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_sign_buf_in, data_psp_private_key_enc, 0x20);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_sign_buf_in + 0x20, data_psp_sha1_outbuf, 0x14);
            //                        if (sceUtilsBufferCopyWithRange(ref data_psp_sign_buf_out, 0x28, ref data_psp_sign_buf_in, 0x34, DefineConstants.KIRK_CMD_ECDSA_SIGN) != 0)
            //                        {
            //                            Console.Write("ERROR: Failed to generate ECDSA signature for DATA.PSP!\n");
            //                            return 0;
            //                        }

            //                        // Verify the generated ECDSA signature.
            //                        byte[] test_data_psp_sign = new byte[0x64];
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(test_data_psp_sign, npumdimg_public_key, 0x28);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(test_data_psp_sign + 0x28, data_psp_sha1_outbuf, 0x14);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(test_data_psp_sign + 0x3C, data_psp_sign_buf_out, 0x28);
            //                        if (sceUtilsBufferCopyWithRange(0, 0, ref test_data_psp_sign, 0x64, DefineConstants.KIRK_CMD_ECDSA_VERIFY) != 0)
            //                        {
            //                            Console.Write("ERROR: ECDSA signature for DATA.PSP is invalid!\n");
            //                            return 0;
            //                        }
            //                        else
            //                        {
            //                            Console.Write("ECDSA signature for DATA.PSP is valid!\n");
            //                        }

            //                        // Store the signature.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(data_psp_buf, data_psp_sign_buf_out, 0x28);

            //                        // Append STARTDAT file to DATA.PSP, if provided.
            //                        if (startdat_size != 0)
            //                        {
            //                            int startdat_offset = 0x594 + 0xC;
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(data_psp_buf + startdat_offset, startdat_buf, startdat_size);

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                            free(startdat_buf);
            //                        }

            //                        // Append encrypted OPNSSMP file to DATA.PSP, if provided.
            //                        if (pgd_size != 0)
            //                        {
            //                            int pgd_offset = (startdat_size) != 0 ? (0x594 + 0xC + startdat_size) : 0x594;
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(data_psp_buf + pgd_offset, pgd_buf, pgd_size);

            //                            // Store OPNSSMP offset and size.
            //                            (uint)(data_psp_buf + 0x28 + 0x8) = pgd_offset;
            //                            (uint)(data_psp_buf + 0x28 + 0x8 + 0x4) = pgd_size;

            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                            free(pgd_buf);
            //                        }

            //                        // Build empty DATA.PSAR.
            //                        Console.Write("Building DATA.PSAR...\n");
            //                        int data_psar_size = 0x100;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *data_psar_buf = (byte *) malloc(data_psar_size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte data_psar_buf = (byte)malloc(data_psar_size);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(data_psar_buf, 0, data_psar_size);

            //                        // Calculate header size.
            //                        int header_size = icon0_size + icon1_size + pic0_size + pic1_size + snd0_size + param_sfo_size + data_psp_size;

            //                        // Allocate PBP header.
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *pbp_header = malloc(header_size + 4096);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        byte pbp_header = malloc(header_size + 4096);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(pbp_header, 0, header_size + 4096);

            //                        // Write magic.
            //                        (uint)(pbp_header + 0) = 0x50425000;
            //                        (uint)(pbp_header + 4) = 0x00010001;

            //                        // Set header offset.
            //                        int header_offset = 0x28;

            //                        // Write PARAM.SFO
            //                        if (param_sfo_size != 0)
            //                        {
            //                            Console.Write("Writing PARAM.SFO...\n");
            //                        }
            //                        (uint)(pbp_header + 0x08) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, param_sfo_buf, param_sfo_size);
            //                        header_offset += param_sfo_size;

            //                        // Write ICON0.PNG
            //                        if (icon0_size != 0)
            //                        {
            //                            Console.Write("Writing ICON0.PNG...\n");
            //                        }
            //                        (uint)(pbp_header + 0x0C) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, icon0_buf, icon0_size);
            //                        header_offset += icon0_size;

            //                        // Write ICON1.PMF
            //                        if (icon1_size != 0)
            //                        {
            //                            Console.Write("Writing ICON1.PNG...\n");
            //                        }
            //                        (uint)(pbp_header + 0x10) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, icon1_buf, icon1_size);
            //                        header_offset += icon1_size;

            //                        // Write PIC0.PNG
            //                        if (pic0_size != 0)
            //                        {
            //                            Console.Write("Writing PIC0.PNG...\n");
            //                        }
            //                        (uint)(pbp_header + 0x14) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, pic0_buf, pic0_size);
            //                        header_offset += pic0_size;

            //                        // Write PIC1.PNG
            //                        if (pic1_size != 0)
            //                        {
            //                            Console.Write("Writing PIC1.PNG...\n");
            //                        }
            //                        (uint)(pbp_header + 0x18) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, pic1_buf, pic1_size);
            //                        header_offset += pic1_size;

            //                        // Write SND0.AT3
            //                        if (snd0_size != 0)
            //                        {
            //                            Console.Write("Writing SND0.AT3...\n");
            //                        }
            //                        (uint)(pbp_header + 0x1C) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, snd0_buf, snd0_size);
            //                        header_offset += snd0_size;

            //                        // Write DATA.PSP
            //                        Console.Write("Writing DATA.PSP...\n");
            //                        (uint)(pbp_header + 0x20) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, data_psp_buf, data_psp_size);
            //                        header_offset += data_psp_size;

            //                        // DATA.PSAR is 0x100 aligned.
            //                        header_offset = (header_offset + 15) & ~15;
            //                        while (header_offset % 0x100 != 0)
            //                        {
            //                            header_offset += 0x10;
            //                        }

            //                        // Write DATA.PSAR
            //                        Console.Write("Writing DATA.PSAR...\n\n");
            //                        (uint)(pbp_header + 0x24) = header_offset;
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                        memcpy(pbp_header + header_offset, data_psar_buf, data_psar_size);
            //                        header_offset += data_psar_size;

            //                        // Write PBP.
            //                        fwrite(pbp_header, header_offset, 1, f);

            //                        // Clean up.
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        free(data_psar_buf);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        free(data_psp_buf);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        free(data_psp_param_buf);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        free(data_psp_sha1_inbuf);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        free(pbp_header);

            //                        return header_offset;
            //                    }

            //                    public static void print_usage()
            //                    {
            //                        Console.Write("***********************************************************\n\n");
            //                        Console.Write("sign_np v1.0.2 - Convert PSP ISOs to signed PSN PBPs.\n");
            //                        Console.Write("               - Written by Hykem (C).\n\n");
            //                        Console.Write("***********************************************************\n\n");
            //                        Console.Write("Usage: sign_np -pbp [-c] <input> <output> <cid> <key>\n");
            //                        Console.Write("                    <startdat> <opnssmp>\n");
            //                        Console.Write("       sign_np -elf <input> <output> <tag>\n");
            //                        Console.Write("\n");
            //                        Console.Write("- Modes:\n");
            //                        Console.Write("[-pbp]: Encrypt and sign a PSP ISO into a PSN EBOOT.PBP\n");
            //                        Console.Write("[-elf]: Encrypt and sign a ELF file into an EBOOT.BIN\n");
            //                        Console.Write("\n");
            //                        Console.Write("- PBP mode:\n");
            //                        Console.Write("[-c]: Compress data.\n");
            //                        Console.Write("<input>: A valid PSP ISO image with a signed EBOOT.BIN\n");
            //                        Console.Write("<output>: Resulting signed EBOOT.PBP file\n");
            //                        Console.Write("<cid>: Content ID (XXYYYY-AAAABBBBB_CC-DDDDDDDDDDDDDDDD)\n");
            //                        Console.Write("<key>: Version key (16 bytes) or Fixed Key (0)\n");
            //                        Console.Write("<startdat>: PNG image to be used as boot screen (optional)\n");
            //                        Console.Write("<opnssmp>: OPNSSMP.BIN module (optional)\n");
            //                        Console.Write("\n");
            //                        Console.Write("- ELF mode:\n");
            //                        Console.Write("<input>: A valid ELF file\n");
            //                        Console.Write("<output>: Resulting signed EBOOT.BIN file\n");
            //                        Console.Write("<tag>: 0 - EBOOT tag 0xD91609F0\n");
            //                        Console.Write("       1 - EBOOT tag 0xD9160AF0\n");
            //                        Console.Write("       2 - EBOOT tag 0xD9160BF0\n");
            //                        Console.Write("       3 - EBOOT tag 0xD91611F0\n");
            //                        Console.Write("       4 - EBOOT tag 0xD91612F0\n");
            //                        Console.Write("       5 - EBOOT tag 0xD91613F0\n");
            //                        Console.Write("       6 - EBOOT tag 0xD91614F0\n");
            //                        Console.Write("       7 - EBOOT tag 0xD91615F0\n");
            //                        Console.Write("       8 - EBOOT tag 0xD91624F0\n");
            //                        Console.Write("       9 - EBOOT tag 0xD91628F0\n");
            //                        Console.Write("       10 - EBOOT tag 0xD91680F0\n");
            //                        Console.Write("       11 - EBOOT tag 0xD91681F0\n");
            //                    }

            //                    static void Main(int argc, string[] args)
            //                    {
            //                        if ((argc <= 1) || (argc > 9))
            //                        {
            //                            print_usage();
            //                        }

            //                        // Keep track of each argument's offset.
            //                        int arg_offset = 0;


            //                        // ELF signing mode.
            //                        #region << ELF >>

            //                        //if (!string.Compare(args[arg_offset + 1], "-elf") && (argc > (arg_offset + 4)))
            //                        //{
            //                        //    // Skip the mode argument.
            //                        //    arg_offset++;

            //                        //    // Open files.
            //                        //    string elf_name = args[arg_offset + 1];
            //                        //    string bin_name = args[arg_offset + 2];
            //                        //    int tag = Convert.ToInt32(args[arg_offset + 3]);
            //                        //    FILE elf = fopen(elf_name, "rb");
            //                        //    FILE bin = fopen(bin_name, "wb");

            //                        //    // Check input file.
            //                        //    if (elf == null)
            //                        //    {
            //                        //        Console.Write("ERROR: Please check your input file!\n");
            //                        //        fclose(elf);
            //                        //        fclose(bin);
            //                        //    }

            //                        //    // Check output file.
            //                        //    if (bin == null)
            //                        //    {
            //                        //        Console.Write("ERROR: Please check your output file!\n");
            //                        //        fclose(elf);
            //                        //        fclose(bin);
            //                        //    }

            //                        //    // Check tag.
            //                        //    if ((tag < 0) || (tag > 11))
            //                        //    {
            //                        //        Console.Write("ERROR: Invalid EBOOT tag!\n");
            //                        //        fclose(elf);
            //                        //        fclose(bin);
            //                        //    }

            //                        //    // Get ELF size.
            //                        //    fseek(elf, 0, SEEK_END);
            //                        //    int elf_size = ftell(elf);
            //                        //    fseek(elf, 0, SEEK_SET);

            //                        //    // Initialize KIRK.
            //                        //    Console.Write("Initializing KIRK engine...\n\n");
            //                        //    kirk_init();

            //                        //    // Read ELF file.
            //                        //    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //    //ORIGINAL LINE: byte *elf_buf = (byte *) malloc(elf_size);
            //                        //    //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        //    byte elf_buf = (byte)malloc(elf_size);
            //                        //    fread(elf_buf, elf_size, 1, elf);

            //                        //    // Sign the ELF file.
            //                        //    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //    //ORIGINAL LINE: byte *seboot_buf = (byte *) malloc(elf_size + 4096);
            //                        //    //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                        //    byte seboot_buf = (byte)malloc(elf_size + 4096);
            //                        //    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        //    memset(seboot_buf, 0, elf_size + 4096);
            //                        //    int seboot_size = sign_eboot(ref elf_buf, elf_size, tag, ref seboot_buf);

            //                        //    // Exit in case of error.
            //                        //    if (seboot_size < 0)
            //                        //    {
            //                        //        fclose(elf);
            //                        //        fclose(bin);
            //                        //    }

            //                        //    // Write the signed EBOOT.BIN file.
            //                        //    fwrite(seboot_buf, seboot_size, 1, bin);

            //                        //    // Clean up.
            //                        //    fclose(bin);
            //                        //    fclose(elf);
            //                        //    //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        //    free(seboot_buf);
            //                        //    //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                        //    free(elf_buf);

            //                        //    Console.Write("Done!\n");

            //                        //}
            //                        //else
            //                        #endregion << ELF >>

            //                       // if (!string.Compare(args[arg_offset + 1], "-pbp") && (argc > (arg_offset + 5))) // EBOOT signing mode.
            //                        {
            //                            // Skip the mode argument.
            //                            arg_offset++;

            //                            // Check if the data must be compressed.
            //                            int compress = 0;
            //                            if (!string.Compare(args[arg_offset + 1], "-c"))
            //                            {
            //                                compress = 1;
            //                                arg_offset++;
            //                            }

            //                            // Check for enough arguments after the compression flag.
            //                            if (argc < (arg_offset + 5))
            //                            {
            //                                print_usage();
            //                            }

            //                            // Open files.
            //                            string iso_name = args[arg_offset + 1];
            //                            string pbp_name = args[arg_offset + 2];
            //                            FILE iso = fopen(iso_name, "rb");
            //                            FILE pbp = fopen(pbp_name, "wb");

            //                            // Get Content ID from input.
            //                            string cid = args[arg_offset + 3];
            //                            string content_id = new string(new char[0x30]);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(content_id, 0, 0x30);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(content_id, cid, cid.Length);

            //                            // Set version, header and data keys.
            //                            int use_version_key = 0;
            //                            byte[] version_key = new byte[0x10];
            //                            byte[] header_key = new byte[0x10];
            //                            byte[] data_key = new byte[0x10];
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(version_key, 0, 0x10);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(header_key, 0, 0x10);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(data_key, 0, 0x10);

            //                            // Read version key from input.
            //                            string vk = args[arg_offset + 4];
            //                            if (is_hex(vk, 0x20))
            //                            {
            //                                byte[] user_key = new byte[0x10];
            //                                hex_to_bytes(ref user_key, vk, 0x20);
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(version_key, user_key, 0x10);
            //                                use_version_key = 1;
            //                            }

            //                            // Check input file.
            //                            if (iso == null)
            //                            {
            //                                Console.Write("ERROR: Please check your input file!\n");
            //                                fclose(iso);
            //                                fclose(pbp);
            //                            }

            //                            // Check output file.
            //                            if (pbp == null)
            //                            {
            //                                Console.Write("ERROR: Please check your output file!\n");
            //                                fclose(iso);
            //                                fclose(pbp);
            //                            }

            //                            // Get ISO size.
            //                            fseeko64(iso, 0, SEEK_END);
            //                            long iso_size = ftello64(iso);
            //                            fseeko64(iso, 0, SEEK_SET);

            //                            // Initialize KIRK.
            //                            Console.Write("Initializing KIRK engine...\n\n");
            //                            kirk_init();

            //                            // Check for optional files.
            //                            string startdat_name = null;
            //                            string opnssmp_name = null;

            //                            if (argc > (arg_offset + 5))
            //                            {
            //                                string ex_file_name1 = args[arg_offset + 5];
            //                                string ex_file_name2 = args[arg_offset + 6];
            //                                char[] png_magic = { 0x89, 0x50, 0x4E, 0x47 }; // %PNG
            //                                char[] psp_magic = { 0x7E, 0x50, 0x53, 0x50 }; // ~PSP

            //                                // Check the first optional file.
            //                                if (ex_file_name1 != null)
            //                                {
            //                                    // Read the first optional file's header.
            //                                    char[] ex_file1_magic = { 0x00, 0x00, 0x00, 0x00 };
            //                                    FILE ex_file1 = fopen(ex_file_name1, "rb");

            //                                    if (ex_file1 != null)
            //                                    {
            //                                        fread(ex_file1_magic, 4, 1, ex_file1);
            //                                    }

            //                                    fclose(ex_file1);

            //                                    // Check for PNG header.
            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
            //                                    if (!memcmp(ex_file1_magic, png_magic, 4))
            //                                    {
            //                                        if (startdat_name == null)
            //                                        {
            //                                            startdat_name = ex_file_name1;
            //                                        }
            //                                    }
            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
            //                                    else if (!memcmp(ex_file1_magic, psp_magic, 4)) // Check for PSP header.
            //                                    {
            //                                        if (opnssmp_name == null)
            //                                        {
            //                                            opnssmp_name = ex_file_name1;
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        Console.Write("ERROR: Please check your optional files!\n");
            //                                        fclose(iso);
            //                                        fclose(pbp);
            //                                    }
            //                                }

            //                                // Check the second optional file.
            //                                if (ex_file_name2 != null)
            //                                {
            //                                    // Read the second optional file.
            //                                    char[] ex_file2_magic = { 0x00, 0x00, 0x00, 0x00 };
            //                                    FILE ex_file2 = fopen(ex_file_name2, "rb");

            //                                    if (ex_file2 != null)
            //                                    {
            //                                        fread(ex_file2_magic, 4, 1, ex_file2);
            //                                    }

            //                                    fclose(ex_file2);

            //                                    // Check for PNG header.
            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
            //                                    if (!memcmp(ex_file2_magic, png_magic, 4))
            //                                    {
            //                                        if (startdat_name == null)
            //                                        {
            //                                            startdat_name = ex_file_name2;
            //                                        }
            //                                    }
            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
            //                                    else if (!memcmp(ex_file2_magic, psp_magic, 4)) // Check for PSP header.
            //                                    {
            //                                        if (opnssmp_name == null)
            //                                        {
            //                                            opnssmp_name = ex_file_name2;
            //                                        }
            //                                    }
            //                                    else
            //                                    {
            //                                        Console.Write("ERROR: Please check your optional files!\n");
            //                                        fclose(iso);
            //                                        fclose(pbp);
            //                                    }
            //                                }
            //                            }

            //                            // Check for custom OPNSSMP file.
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte *pgd_buf = null;
            //                            byte pgd_buf = null;
            //                            int pgd_size = 0;
            //                            if (opnssmp_name != null)
            //                            {
            //                                // Open file.
            //                                FILE opnssmp = fopen(opnssmp_name, "rb");

            //                                // Check for valid file.
            //                                if (opnssmp == null)
            //                                {
            //                                    Console.Write("ERROR: Please check your OPNSSMP file!\n");
            //                                    fclose(opnssmp);
            //                                    fclose(iso);
            //                                    fclose(pbp);
            //                                }

            //                                // Get OPNSSMP file size.
            //                                fseek(opnssmp, 0, SEEK_END);
            //                                int opnssmp_size = ftell(opnssmp);
            //                                fseek(opnssmp, 0, SEEK_SET);

            //                                // Generate random PGD key.
            //                                byte[] pgd_key = new byte[0x10];
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                                memset(pgd_key, 0, 0x10);
            //                                sceUtilsBufferCopyWithRange(ref pgd_key, 0x10, 0, 0, DefineConstants.KIRK_CMD_PRNG);

            //                                // Prepare PGD buffers.
            //                                int pgd_block_size = 2048;
            //                                int pgd_blocks = ((opnssmp_size + pgd_block_size - 1) & ~(pgd_block_size - 1)) / pgd_block_size;
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                                pgd_buf = (byte)malloc(0x90 + opnssmp_size + pgd_blocks * 16);

            //                                // Read OPNSSMP file.
            //                                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                                //ORIGINAL LINE: byte *opnssmp_buf = (byte *) malloc(opnssmp_size);
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                                byte opnssmp_buf = (byte)malloc(opnssmp_size);
            //                                fread(opnssmp_buf, opnssmp_size, 1, opnssmp);

            //                                // Encrypt OPNSSMP file.
            //                                pgd_size = encrypt_pgd(ref opnssmp_buf, opnssmp_size, pgd_block_size, 1, 1, 2, ref pgd_key, ref pgd_buf);

            //                                // Clean up.
            //                                fclose(opnssmp);
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                                free(opnssmp_buf);
            //                            }

            //                            // Check for custom STARTDAT file.
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte *startdat_buf = null;
            //                            byte startdat_buf = null;
            //                            int startdat_size = 0;
            //                            if (startdat_name != null)
            //                            {
            //                                // Open file.
            //                                FILE png = fopen(startdat_name, "rb");

            //                                // Check for valid file.
            //                                if (png == null)
            //                                {
            //                                    Console.Write("ERROR: Please check your STARTDAT file!\n");
            //                                    fclose(png);
            //                                    fclose(iso);
            //                                    fclose(pbp);
            //                                }

            //                                // Get STARTDAT file size.
            //                                fseek(png, 0, SEEK_END);
            //                                int png_size = ftell(png);
            //                                fseek(png, 0, SEEK_SET);

            //                                // Prepare STARTDAT buffer.
            //                                startdat_size = png_size + 0x50;
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                                startdat_buf = (byte)malloc(startdat_size);

            //                                // Build STARTDAT header.
            //                                STARTDAT_HEADER[] sd_header = Arrays.InitializeWithDefaultInstances<STARTDAT_HEADER>(0x50);
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                                memset(sd_header, 0, 0x50);

            //                                // Set magic STARTDAT.
            //                                sd_header.magic[0] = 0x53;
            //                                sd_header.magic[1] = 0x54;
            //                                sd_header.magic[2] = 0x41;
            //                                sd_header.magic[3] = 0x52;
            //                                sd_header.magic[4] = 0x54;
            //                                sd_header.magic[5] = 0x44;
            //                                sd_header.magic[6] = 0x41;
            //                                sd_header.magic[7] = 0x54;

            //                                // Set unknown flags.
            //                                sd_header.unk1 = 0x1;
            //                                sd_header.unk2 = 0x1;

            //                                // Set header and data size.
            //                                sd_header.header_size = 0x50;
            //                                sd_header.data_size = png_size;

            //                                // Copy the STARTDAT header.
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                                memcpy(startdat_buf, sd_header, 0x50);

            //                                // Read the PNG file.
            //                                fread(startdat_buf + 0x50, png_size, 1, png);

            //                                // Clean up.
            //                                fclose(png);
            //                            }

            //                            // Set keys' context.
            //                            MAC_KEY mkey = new MAC_KEY();
            //                            CIPHER_KEY ckey = new CIPHER_KEY();

            //                            // Set flags and block size data.
            //                            int np_flags = (use_version_key) != 0 ? 0x2 : (0x3 | (0x01000000));
            //                            int block_basis = 0x10;
            //                            int block_size = block_basis * 2048;
            //                            long iso_blocks = (iso_size + block_size - 1) / block_size;

            //                            // Generate random header key.
            //                            sceUtilsBufferCopyWithRange(ref header_key, 0x10, 0, 0, DefineConstants.KIRK_CMD_PRNG);

            //                            // Generate fixed key, if necessary.
            //                            if (use_version_key == 0)
            //                            {
            //                                sceNpDrmGetFixedKey(ref version_key, ref content_id, np_flags);
            //                            }

            //                            // Write PBP data.
            //                            Console.Write("Writing PBP data...\n");
            //                            long table_offset = write_pbp(pbp, ref iso_name, ref content_id, np_flags, ref startdat_buf, startdat_size, ref pgd_buf, pgd_size);
            //                            long table_size = iso_blocks * 0x20;
            //                            long np_offset = table_offset - 0x100;
            //                            int np_size = 0x100;

            //                            // Write NPUMDIMG table.
            //                            //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
            //                            //ORIGINAL LINE: printf("NPUMDIMG table size: %I64d\n", table_size);
            //                            Console.Write("NPUMDIMG table size: %I64d\n", table_size);
            //                            Console.Write("Writing NPUMDIMG table...\n\n");
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte *table_buf = malloc(table_size);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                            byte table_buf = malloc(table_size);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                            memset(table_buf, 0, table_size);
            //                            fwrite(table_buf, table_size, 1, pbp);

            //                            // Write ISO blocks.
            //                            //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
            //                            //ORIGINAL LINE: printf("ISO size: %I64d\n", iso_size);
            //                            Console.Write("ISO size: %I64d\n", iso_size);
            //                            //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
            //                            //ORIGINAL LINE: printf("ISO blocks: %I64d\n", iso_blocks);
            //                            Console.Write("ISO blocks: %I64d\n", iso_blocks);
            //                            long iso_offset = 0x100 + table_size;
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte *iso_buf = malloc(block_size * 2);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                            byte iso_buf = malloc(block_size * 2);
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte *lzrc_buf = malloc(block_size * 2);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                            byte lzrc_buf = malloc(block_size * 2);

            //                            int i;
            //                            for (i = 0; i < iso_blocks; i++)
            //                            {
            //                                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                                //ORIGINAL LINE: byte *tb = table_buf + i * 0x20;
            //                                byte tb = table_buf + i * 0x20;
            //                                //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                                //ORIGINAL LINE: byte *wbuf;
            //                                byte wbuf;
            //                                int wsize;
            //                                int lzrc_size;
            //                                int ratio;

            //                                // Read ISO block.
            //                                //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                                memset(iso_buf, 0, block_size);
            //                                if ((ftello64(iso) + block_size) > iso_size)
            //                                {
            //                                    long remaining = iso_size - ftello64(iso);
            //                                    fread(iso_buf, remaining, 1, iso);
            //                                    wsize = remaining;
            //                                }
            //                                else
            //                                {
            //                                    fread(iso_buf, block_size, 1, iso);
            //                                    wsize = block_size;
            //                                }

            //                                // Set write buffer.
            //                                wbuf = iso_buf;

            //                                // Compress data.
            //                                if (compress == 1)
            //                                {
            //                                    lzrc_size = lzrc_compress(lzrc_buf, block_size * 2, iso_buf, block_size);
            //                                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                                    memset(lzrc_buf + lzrc_size, 0, 16);
            //                                    ratio = (lzrc_size * 100) / block_size;

            //                                    if (ratio < DefineConstants.RATIO_LIMIT)
            //                                    {
            //                                        wbuf = lzrc_buf;
            //                                        wsize = (lzrc_size + 15) & ~15;
            //                                    }
            //                                }

            //                                // Set table entry.
            //                                (uint)(tb + 0x10) = iso_offset;
            //                                (uint)(tb + 0x14) = wsize;
            //                                (uint)(tb + 0x18) = 0;
            //                                (uint)(tb + 0x1C) = 0;

            //                                // Encrypt block.
            //                                sceDrmBBCipherInit(ckey, 1, 2, ref header_key, ref version_key, (iso_offset >> 4));
            //                                sceDrmBBCipherUpdate(ckey, ref wbuf, wsize);
            //                                sceDrmBBCipherFinal(ckey);

            //                                // Build MAC.
            //                                sceDrmBBMacInit(mkey, 3);
            //                                sceDrmBBMacUpdate(mkey, ref wbuf, wsize);
            //                                sceDrmBBMacFinal(mkey, ref tb, ref version_key);
            //                                bbmac_build_final2(3, ref tb);

            //                                // Encrypt table.
            //                                encrypt_table(ref tb);

            //                                // Write ISO data.
            //                                wsize = (wsize + 15) & ~15;
            //                                fwrite(wbuf, wsize, 1, pbp);

            //                                // Update offset.
            //                                iso_offset += wsize;
            //                                //C++ TO C# CONVERTER TODO TASK: The following line has a C format specifier which cannot be directly translated to C#:
            //                                //ORIGINAL LINE: printf("\rWriting ISO blocks: %02I64d%%", i * 100 / iso_blocks);
            //                                Console.Write("\rWriting ISO blocks: %02I64d%", i * 100 / iso_blocks);
            //                            }
            //                            Console.Write("\rWriting ISO blocks: 100%%\n\n");

            //                            // Generate data key.
            //                            sceDrmBBMacInit(mkey, 3);
            //                            sceDrmBBMacUpdate(mkey, ref table_buf, table_size);
            //                            sceDrmBBMacFinal(mkey, ref data_key, ref version_key);
            //                            bbmac_build_final2(3, ref data_key);

            //                            // Forge NPUMDIMG header.
            //                            Console.Write("Forging NPUMDIMG header...\n");
            //                            NPUMDIMG_HEADER npumdimg = forge_npumdimg((int)iso_size, (int)iso_blocks, block_basis, ref content_id, np_flags, ref version_key, ref header_key, ref data_key);
            //                            Console.Write("NPUMDIMG flags: 0x{0:X8}\n", np_flags);
            //                            Console.Write("NPUMDIMG block basis: 0x{0:X8}\n", block_basis);
            //                            Console.Write("NPUMDIMG version key: 0x");
            //                            for (i = 0; i < 0x10; i++)
            //                            {
            //                                Console.Write("{0:X2}", version_key[i]);
            //                            }
            //                            Console.Write("\n");
            //                            Console.Write("NPUMDIMG header key: 0x");
            //                            for (i = 0; i < 0x10; i++)
            //                            {
            //                                Console.Write("{0:X2}", npumdimg.header_key[i]);
            //                            }
            //                            Console.Write("\n");
            //                            Console.Write("NPUMDIMG header hash: 0x");
            //                            for (i = 0; i < 0x10; i++)
            //                            {
            //                                Console.Write("{0:X2}", npumdimg.header_hash[i]);
            //                            }
            //                            Console.Write("\n");
            //                            Console.Write("NPUMDIMG data key: 0x");
            //                            for (i = 0; i < 0x10; i++)
            //                            {
            //                                Console.Write("{0:X2}", npumdimg.data_key[i]);
            //                            }
            //                            Console.Write("\n\n");

            //                            // Update NPUMDIMG header and NP table.
            //                            fseeko64(pbp, np_offset, SEEK_SET);
            //                            fwrite(npumdimg, np_size, 1, pbp);
            //                            fseeko64(pbp, table_offset, SEEK_SET);
            //                            fwrite(table_buf, table_size, 1, pbp);

            //                            // Clean up.
            //                            fclose(iso);
            //                            fclose(pbp);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                            free(table_buf);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                            free(iso_buf);
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
            //                            free(lzrc_buf);
            //                            npumdimg = null;

            //                            Console.Write("Done!\n");

            //                        }
            //                        else
            //                        {
            //                            print_usage();
            //                        }
            //                    }


            //                    public static int lzrc_compress(object @out, int out_len, object in, int in_len)
            //                    {
            //                        LZRC_DECODE re = new LZRC_DECODE();
            //                        int match_step;
            //                        int re_state;
            //                        int len_state;
            //                        int dist_state;
            //                        int i;
            //                        int @byte;
            //                        int last_byte;
            //                        int match_len;
            //                        int len_bits;
            //                        int match_dist;
            //                        int dist_bits;
            //                        int limit;
            //                        int round = -1;

            //                        re_init(re, @out, out_len, in, in_len);
            //                        init_tree();

            //                        re_state = 0;
            //                        last_byte = 0;
            //                        match_len = 0;
            //                        match_dist = 0;

            //                        while (true)
            //                        {
            //                            round += 1;
            //                            match_step = 0;

            //                            fill_buffer(re);
            //                            insert_node(re, t_end, ref match_len, ref match_dist, 1);
            //                            if (match_len < 256)
            //                            {
            //                                if (match_len < 4 && match_dist > 255)
            //                                {
            //                                    match_len = 1;
            //                                }
            //                                update_tree(re, match_len);
            //                            }

            //                            if (match_len == 1 || (match_len < 4 && match_dist > 255))
            //                            {
            //                                re_bit(re, re.bm_match[re_state, match_step], 0);

            //                                if (re_state > 0)
            //                                {
            //                                    re_state -= 1;
            //                                }

            //                                @byte = re_getbyte(re);
            //                                re_bittree(re, ref re.bm_literal[((last_byte >> re.lc) & 0x07), 0], 0x100, @byte);
            //                            }
            //                            else
            //                            {
            //                                re_bit(re, re.bm_match[re_state, match_step], 1);

            //                                len_bits = 0;
            //                                for (i = 1; i < 8; i++)
            //                                {
            //                                    match_step += 1;
            //                                    if ((match_len - 1) < (1 << i))
            //                                    {
            //                                        break;
            //                                    }
            //                                    re_bit(re, re.bm_match[re_state, match_step], 1);
            //                                    len_bits += 1;
            //                                }
            //                                if (i != 8)
            //                                {
            //                                    re_bit(re, re.bm_match[re_state, match_step], 0);
            //                                }

            //                                if (len_bits > 0)
            //                                {
            //                                    len_state = ((len_bits - 1) << 2) + ((re.in_ptr << (len_bits - 1)) & 0x03);
            //                                    re_number(re, ref re.bm_len[re_state, len_state], len_bits, (match_len - 1));
            //                                    if (match_len == 0x100)
            //                                    {
            //                                        re_normalize(re);
            //                                        re_flush(re);
            //                                        return re.out_ptr;
            //                                    }
            //                                }

            //                                dist_state = 0;
            //                                limit = 8;
            //                                if (match_len > 3)
            //                                {
            //                                    dist_state += 7;
            //                                    limit = 44;
            //                                }

            //                                dist_bits = 0;
            //                                while ((match_dist >> dist_bits) != 1)
            //                                {
            //                                    dist_bits += 1;
            //                                }

            //                                re_bittree(re, ref re.bm_dist_bits[len_bits, dist_state], limit, dist_bits);

            //                                if (dist_bits > 0)
            //                                {
            //                                    re_number(re, ref re.bm_dist[dist_bits, 0], dist_bits, match_dist);
            //                                }

            //                                re.in_ptr += match_len;
            //                                re_state = 6 + ((re.in_ptr + 1) & 1);
            //                            }
            //                            last_byte = re.input[re.in_ptr - 1];
            //                        }
            //                    }
            //                    public static int lzrc_decompress(object @out, int out_len, object in, int in_len)
            //                    {
            //                        LZRC_DECODE rc = new LZRC_DECODE();
            //                        int match_step;
            //                        int rc_state;
            //                        int len_state;
            //                        int dist_state;
            //                        int i;
            //                        int bit;
            //                        int @byte;
            //                        int last_byte;
            //                        int match_len;
            //                        int len_bits;
            //                        int match_dist;
            //                        int dist_bits;
            //                        int limit;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *match_src;
            //                        byte match_src;
            //                        int round = -1;

            //                        rc_init(rc, @out, out_len, in, in_len);

            //                        if ((rc.lc & 0x80) != 0)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(rc.output, rc.input + 5, rc.code);
            //                            return rc.code;
            //                        }

            //                        rc_state = 0;
            //                        last_byte = 0;

            //                        while (true)
            //                        {
            //                            round += 1;
            //                            match_step = 0;

            //                            bit = rc_bit(rc, rc.bm_match[rc_state, match_step]);
            //                            if (bit == 0)
            //                            {
            //                                if (rc_state > 0)
            //                                {
            //                                    rc_state -= 1;
            //                                }

            //                                @byte = rc_bittree(rc, ref rc.bm_literal[((last_byte >> rc.lc) & 0x07), 0], 0x100);
            //                                @byte -= 0x100;

            //                                rc_putbyte(rc, @byte);
            //                            }
            //                            else
            //                            {
            //                                len_bits = 0;
            //                                for (i = 0; i < 7; i++)
            //                                {
            //                                    match_step += 1;
            //                                    bit = rc_bit(rc, rc.bm_match[rc_state, match_step]);
            //                                    if (bit == 0)
            //                                    {
            //                                        break;
            //                                    }
            //                                    len_bits += 1;
            //                                }

            //                                if (len_bits == 0)
            //                                {
            //                                    match_len = 1;
            //                                }
            //                                else
            //                                {
            //                                    len_state = ((len_bits - 1) << 2) + ((rc.out_ptr << (len_bits - 1)) & 0x03);
            //                                    match_len = rc_number(rc, ref rc.bm_len[rc_state, len_state], len_bits);
            //                                    if (match_len == 0xFF)
            //                                    {
            //                                        return rc.out_ptr;
            //                                    }
            //                                }

            //                                dist_state = 0;
            //                                limit = 8;
            //                                if (match_len > 2)
            //                                {
            //                                    dist_state += 7;
            //                                    limit = 44;
            //                                }
            //                                dist_bits = rc_bittree(rc, ref rc.bm_dist_bits[len_bits, dist_state], limit);
            //                                dist_bits -= limit;

            //                                if (dist_bits > 0)
            //                                {
            //                                    match_dist = rc_number(rc, ref rc.bm_dist[dist_bits, 0], dist_bits);
            //                                }
            //                                else
            //                                {
            //                                    match_dist = 1;
            //                                }

            //                                if (match_dist > rc.out_ptr || match_dist < 0)
            //                                {
            //                                    Console.Write("match_dist out of range! {0:x8}\n", match_dist);
            //                                    return -1;
            //                                }
            //                                match_src = rc.output + rc.out_ptr - match_dist;
            //                                for (i = 0; i < match_len + 1; i++)
            //                                {
            //                                    rc_putbyte(rc, match_src++);
            //                                }
            //                                rc_state = 6 + ((rc.out_ptr + 1) & 1);
            //                            }
            //                            last_byte = rc.output[rc.out_ptr - 1];
            //                        }
            //                    }

            //                    internal static byte[] text_buf = new byte[65536];
            //                    internal static int t_start;
            //                    internal static int t_end;
            //                    internal static int t_fill;
            //                    internal static int sp_fill;
            //                    internal static int t_len;
            //                    internal static int t_pos;

            //                    internal static int[] prev = new int[65536];
            //                    internal static int[] next = new int[65536];
            //                    internal static int[] root = new int[65536];

            //                    /* 
            //                        LZRC decoder
            //                    */
            //                    internal static byte rc_getbyte(LZRC_DECODE rc)
            //                    {
            //                        if (rc.in_ptr == rc.in_len)
            //                        {
            //                            Console.Write("End of input!\n");
            //                            Environment.Exit(-1);
            //                        }

            //                        return rc.input[rc.in_ptr++];
            //                    }

            //                    internal static void rc_putbyte(LZRC_DECODE rc, byte @byte)
            //                    {
            //                        if (rc.out_ptr == rc.out_len)
            //                        {
            //                            Console.Write("Output overflow!\n");
            //                            Environment.Exit(-1);
            //                        }

            //                        rc.output[rc.out_ptr++] = @byte;
            //                    }

            //                    internal static void rc_init(LZRC_DECODE rc, object @out, int out_len, object _in, int in_len)
            //                    {
            //                        rc.input = in;
            //                        rc.in_len = in_len;
            //                        rc.in_ptr = 0;

            //                        rc.output = @out;
            //                        rc.out_len = out_len;
            //                        rc.out_ptr = 0;

            //                        rc.range = 0xffffffff;
            //                        rc.lc = rc_getbyte(rc);
            //                        rc.code = (rc_getbyte(rc) << 24) | (rc_getbyte(rc) << 16) | (rc_getbyte(rc) << 8) | (rc_getbyte(rc) << 0);
            //                        rc.out_code = 0xffffffff;

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(rc.bm_literal, 0x80, 2048);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(rc.bm_dist_bits, 0x80, 312);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(rc.bm_dist, 0x80, 144);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(rc.bm_match, 0x80, 64);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(rc.bm_len, 0x80, 248);
            //                    }

            //                    internal static void normalize(LZRC_DECODE rc)
            //                    {
            //                        if (rc.range < 0x01000000)
            //                        {
            //                            rc.range <<= 8;
            //                            rc.code = (rc.code << 8) + rc.input[rc.in_ptr];
            //                            rc.in_ptr++;
            //                        }
            //                    }

            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'prob', so pointers on this parameter are left unchanged:
            //                    internal static int rc_bit(LZRC_DECODE rc, byte prob)
            //                    {
            //                        uint bound;

            //                        normalize(rc);

            //                        bound = (rc.range >> 8) * (*prob);
            //                        *prob -= *prob >> 3;

            //                        if (rc.code < bound)
            //                        {
            //                            rc.range = bound;
            //                            *prob += 31;
            //                            return 1;
            //                        }
            //                        else
            //                        {
            //                            rc.code -= bound;
            //                            rc.range -= bound;
            //                            return 0;
            //                        }
            //                    }

            //                    internal static int rc_bittree(LZRC_DECODE rc, ref byte probs, int limit)
            //                    {
            //                        int number = 1;

            //                        do
            //                        {
            //                            number = (number << 1) + rc_bit(rc, probs + number);
            //                        } while (number < limit);

            //                        return number;
            //                    }

            //                    internal static int rc_number(LZRC_DECODE rc, ref byte prob, int n)
            //                    {
            //                        int i;
            //                        int number = 1;

            //                        if (n > 3)
            //                        {
            //                            number = (number << 1) + rc_bit(rc, prob + 3);
            //                            if (n > 4)
            //                            {
            //                                number = (number << 1) + rc_bit(rc, prob + 3);
            //                                if (n > 5)
            //                                {
            //                                    normalize(rc);
            //                                    for (i = 0; i < n - 5; i++)
            //                                    {
            //                                        rc.range >>= 1;
            //                                        number <<= 1;
            //                                        if (rc.code < rc.range)
            //                                        {
            //                                            number += 1;
            //                                        }
            //                                        else
            //                                        {
            //                                            rc.code -= rc.range;
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }

            //                        if (n > 0)
            //                        {
            //                            number = (number << 1) + rc_bit(rc, prob);
            //                            if (n > 1)
            //                            {
            //                                number = (number << 1) + rc_bit(rc, prob + 1);
            //                                if (n > 2)
            //                                {
            //                                    number = (number << 1) + rc_bit(rc, prob + 2);
            //                                }
            //                            }
            //                        }

            //                        return number;
            //                    }

            //                    /* 
            //                        LZRC encoder
            //                    */
            //                    internal static byte re_getbyte(LZRC_DECODE re)
            //                    {
            //                        if (re.in_ptr == re.in_len)
            //                        {
            //                            Console.Write("End of input!\n");
            //                            Environment.Exit(-1);
            //                        }

            //                        return re.input[re.in_ptr++];
            //                    }

            //                    internal static void re_putbyte(LZRC_DECODE re, byte @byte)
            //                    {
            //                        if (re.out_ptr == re.out_len)
            //                        {
            //                            Console.Write("Output overflow!\n");
            //                            Environment.Exit(-1);
            //                        }

            //                        re.output[re.out_ptr++] = @byte;
            //                    }

            //                    internal static void re_init(LZRC_DECODE re, object @out, int out_len, object in, int in_len)
            //                    {
            //                        re.input = in;
            //                        re.in_len = in_len;
            //                        re.in_ptr = 0;

            //                        re.output = @out;
            //                        re.out_len = out_len;
            //                        re.out_ptr = 0;

            //                        re.range = 0xffffffff;
            //                        re.code = 0x00000000;
            //                        re.lc = 5;
            //                        re.out_code = 0xffffffff;

            //                        re_putbyte(re, re.lc);

            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(re.bm_literal, 0x80, 2048);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(re.bm_dist_bits, 0x80, 312);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(re.bm_dist, 0x80, 144);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(re.bm_match, 0x80, 64);
            //                        //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
            //                        memset(re.bm_len, 0x80, 248);
            //                    }

            //                    internal static void re_flush(LZRC_DECODE re)
            //                    {
            //                        re_putbyte(re, (re.out_code) & 0xff);
            //                        re_putbyte(re, (re.code >> 24) & 0xff);
            //                        re_putbyte(re, (re.code >> 16) & 0xff);
            //                        re_putbyte(re, (re.code >> 8) & 0xff);
            //                        re_putbyte(re, (re.code >> 0) & 0xff);
            //                    }

            //                    internal static void re_normalize(LZRC_DECODE re)
            //                    {
            //                        if (re.range < 0x01000000)
            //                        {
            //                            if (re.out_code != 0xffffffff)
            //                            {
            //                                if (re.out_code > 255)
            //                                {
            //                                    int p;
            //                                    int old_c;
            //                                    p = re.out_ptr - 1;
            //                                    do
            //                                    {
            //                                        old_c = re.output[p];
            //                                        re.output[p] += 1;
            //                                        p -= 1;
            //                                    } while (old_c == 0xff);
            //                                }

            //                                re_putbyte(re, re.out_code & 0xff);
            //                            }
            //                            re.out_code = (re.code >> 24) & 0xff;
            //                            re.range <<= 8;
            //                            re.code <<= 8;
            //                        }
            //                    }

            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'prob', so pointers on this parameter are left unchanged:
            //                    internal static void re_bit(LZRC_DECODE re, byte prob, int bit)
            //                    {
            //                        uint bound;
            //                        uint old_r;
            //                        uint old_c;
            //                        byte old_p;

            //                        re_normalize(re);

            //                        old_r = re.range;
            //                        old_c = re.code;
            //                        old_p = prob;

            //                        bound = (re.range >> 8) * (*prob);
            //                        *prob -= *prob >> 3;

            //                        if (bit != 0)
            //                        {
            //                            re.range = bound;
            //                            *prob += 31;
            //                        }
            //                        else
            //                        {
            //                            re.code += bound;
            //                            if (re.code < old_c)
            //                            {
            //                                re.out_code += 1;
            //                            }
            //                            re.range -= bound;
            //                        }
            //                    }

            //                    internal static void re_bittree(LZRC_DECODE re, ref byte probs, int limit, int number)
            //                    {
            //                        int n;
            //                        int tmp;
            //                        int bit;

            //                        number += limit;

            //                        tmp = number;
            //                        n = 0;
            //                        while (tmp > 1)
            //                        {
            //                            tmp >>= 1;
            //                            n++;
            //                        }

            //                        do
            //                        {
            //                            tmp = number >> n;
            //                            bit = (number >> (n - 1)) & 1;
            //                            re_bit(re, probs + tmp, bit);
            //                            n -= 1;
            //                        } while (n != 0);
            //                    }

            //                    internal static void re_number(LZRC_DECODE re, ref byte prob, int n, int number)
            //                    {
            //                        int i;
            //                        uint old_c;

            //                        i = 1;

            //                        if (n > 3)
            //                        {
            //                            re_bit(re, prob + 3, (number >> (n - i)) & 1);
            //                            i += 1;
            //                            if (n > 4)
            //                            {
            //                                re_bit(re, prob + 3, (number >> (n - i)) & 1);
            //                                i += 1;
            //                                if (n > 5)
            //                                {
            //                                    re_normalize(re);
            //                                    for (i = 3; i < n - 2; i++)
            //                                    {
            //                                        re.range >>= 1;
            //                                        if (((number >> (n - i)) & 1) == 0)
            //                                        {
            //                                            old_c = re.code;
            //                                            re.code += re.range;
            //                                            if (re.code < old_c)
            //                                            {
            //                                                re.out_code += 1;
            //                                            }
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }

            //                        if (n > 0)
            //                        {
            //                            re_bit(re, prob + 0, (number >> (n - i - 0)) & 1);
            //                            if (n > 1)
            //                            {
            //                                re_bit(re, prob + 1, (number >> (n - i - 1)) & 1);
            //                                if (n > 2)
            //                                {
            //                                    re_bit(re, prob + 2, (number >> (n - i - 2)) & 1);
            //                                }
            //                            }
            //                        }
            //                    }

            //                    internal static void init_tree()
            //                    {
            //                        int i;

            //                        for (i = 0; i < 65536; i++)
            //                        {
            //                            root[i] = -1;
            //                            prev[i] = -1;
            //                            next[i] = -1;
            //                        }

            //                        t_start = 0;
            //                        t_end = 0;
            //                        t_fill = 0;
            //                        sp_fill = 0;
            //                    }

            //                    internal static void remove_node(LZRC_DECODE re, int p)
            //                    {
            //                        int t;
            //                        int q;

            //                        if (prev[p] == -1)
            //                        {
            //                            return;
            //                        }

            //                        t = text_buf[p + 0];
            //                        t = (t << 8) | text_buf[p + 1];

            //                        q = next[p];
            //                        if (q != -1)
            //                        {
            //                            prev[q] = prev[p];
            //                        }

            //                        if (prev[p] == -2)
            //                        {
            //                            root[t] = q;
            //                        }
            //                        else
            //                        {
            //                            next[prev[p]] = q;
            //                        }

            //                        prev[p] = -1;
            //                        next[p] = -1;
            //                    }

            //                    internal static int insert_node(LZRC_DECODE re, int pos, ref int match_len, ref int match_dist, int do_cmp)
            //                    {
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *src, *win;
            //                        byte src;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *win;
            //                        byte win;
            //                        int i;
            //                        int t;
            //                        int p;
            //                        int content_size;

            //                        src = text_buf + pos;
            //                        win = text_buf + t_start;
            //                        content_size = (t_fill < pos) ? (65280 + t_fill - pos) : (t_fill - pos);
            //                        t_len = 1;
            //                        t_pos = 0;
            //                        match_len = t_len;
            //                        match_dist = t_pos;

            //                        if (re.in_ptr == re.in_len)
            //                        {
            //                            match_len = 256;
            //                            return 0;
            //                        }

            //                        if (re.in_ptr == (re.in_len - 1))
            //                        {
            //                            return 0;
            //                        }

            //                        t = src[0];
            //                        t = (t << 8) | src[1];
            //                        if (root[t] == -1)
            //                        {
            //                            root[t] = pos;
            //                            prev[pos] = -2;
            //                            next[pos] = -1;
            //                            return 0;
            //                        }

            //                        p = root[t];
            //                        root[t] = pos;
            //                        prev[pos] = -2;
            //                        next[pos] = p;

            //                        if (p != -1)
            //                        {
            //                            prev[p] = pos;
            //                        }

            //                        while (do_cmp == 1 && p != -1)
            //                        {
            //                            for (i = 0; (i < 255 && i < content_size); i++)
            //                            {
            //                                if (src[i] != text_buf[p + i])
            //                                {
            //                                    break;
            //                                }
            //                            }

            //                            if (i > t_len)
            //                            {
            //                                t_len = i;
            //                                t_pos = pos - p;
            //                            }
            //                            else if (i == t_len)
            //                            {
            //                                int mp = pos - p;
            //                                if (mp < 0)
            //                                {
            //                                    mp += 65280;
            //                                }
            //                                if (mp < t_pos)
            //                                {
            //                                    t_len = i;
            //                                    t_pos = pos - p;
            //                                }
            //                            }
            //                            if (i == 255)
            //                            {
            //                                remove_node(re, p);
            //                                break;
            //                            }

            //                            p = next[p];
            //                        }

            //                        match_len = t_len;
            //                        match_dist = t_pos;

            //                        return 1;
            //                    }

            //                    internal static void fill_buffer(LZRC_DECODE re)
            //                    {
            //                        int content_size;
            //                        int back_size;
            //                        int front_size;

            //                        if (sp_fill == re.in_len)
            //                        {
            //                            return;
            //                        }

            //                        content_size = (t_fill < t_end) ? (65280 + t_fill - t_end) : (t_fill - t_end);
            //                        if (content_size >= 509)
            //                        {
            //                            return;
            //                        }

            //                        if (t_fill < t_start)
            //                        {
            //                            back_size = t_start - t_fill - 1;
            //                            if (sp_fill + back_size > re.in_len)
            //                            {
            //                                back_size = re.in_len - sp_fill;
            //                            }
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(text_buf + t_fill, re.input + sp_fill, back_size);
            //                            sp_fill += back_size;
            //                            t_fill += back_size;
            //                        }
            //                        else
            //                        {
            //                            back_size = 65280 - t_fill;
            //                            if (t_start == 0)
            //                            {
            //                                back_size -= 1;
            //                            }
            //                            if (sp_fill + back_size > re.in_len)
            //                            {
            //                                back_size = re.in_len - sp_fill;
            //                            }
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(text_buf + t_fill, re.input + sp_fill, back_size);
            //                            sp_fill += back_size;
            //                            t_fill += back_size;

            //                            front_size = t_start;
            //                            if (t_start != 0)
            //                            {
            //                                front_size -= 1;
            //                            }
            //                            if (sp_fill + front_size > re.in_len)
            //                            {
            //                                front_size = re.in_len - sp_fill;
            //                            }
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(text_buf, re.input + sp_fill, front_size);
            //                            sp_fill += front_size;
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(text_buf + 65280, text_buf, 255);
            //                            t_fill += front_size;
            //                            if (t_fill >= 65280)
            //                            {
            //                                t_fill -= 65280;
            //                            }
            //                        }
            //                    }

            //                    internal static void update_tree(LZRC_DECODE re, int length)
            //                    {
            //                        int i;
            //                        int win_size;
            //                        int tmp_len;
            //                        int tmp_pos;

            //                        win_size = (t_end >= t_start) ? (t_end - t_start) : (65280 + t_end - t_start);

            //                        for (i = 0; i < length; i++)
            //                        {
            //                            if (win_size == 16384)
            //                            {
            //                                remove_node(re, t_start);
            //                                t_start += 1;
            //                                if (t_start == 65280)
            //                                {
            //                                    t_start = 0;
            //                                }
            //                            }
            //                            else
            //                            {
            //                                win_size += 1;
            //                            }

            //                            if (i > 0)
            //                            {
            //                                insert_node(re, t_end, ref tmp_len, ref tmp_pos, 0);
            //                            }
            //                            t_end += 1;
            //                            if (t_end >= 65280)
            //                            {
            //                                t_end -= 65280;
            //                            }
            //                        }
            //                    }

            //                    internal static void re_find_match(LZRC_DECODE re, ref int match_len, ref int match_dist)
            //                    {
            //                        int cp;
            //                        int win_p;
            //                        int i;
            //                        int j;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *pbuf, *cbuf;
            //                        byte pbuf;
            //                        //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                        //ORIGINAL LINE: byte *cbuf;
            //                        byte cbuf;

            //                        cp = re.in_ptr;

            //                        if (cp == re.in_len)
            //                        {
            //                            match_len = 256;
            //                            return;
            //                        }
            //                        else
            //                        {
            //                            match_len = 1;
            //                        }

            //                        win_p = (cp < 16384) ? cp : 16384;

            //                        for (i = 1; i <= win_p; i++)
            //                        {
            //                            j = 0;
            //                            cbuf = re.input + cp;
            //                            pbuf = cbuf - i;
            //                            while ((j < 255) && (cp + j < re.in_len) && (pbuf[j] == cbuf[j]))
            //                            {
            //                                j += 1;
            //                            }

            //                            if (j >= 2 && match_len < j)
            //                            {
            //                                match_len = j;
            //                                match_dist = i;
            //                            }
            //                        }
            //                    }

            //                    // Auxiliary functions.
            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt

            //                    // Copyright (C) 2015       Hykem <hykem@hotmail.com>
            //                    // Licensed under the terms of the GNU GPL, version 3
            //                    // http://www.gnu.org/licenses/gpl-3.0.txt



            //                    public static ushort se16(ushort i)
            //                    {
            //                        return (((i & 0xFF00) >> 8) | ((i & 0xFF) << 8));
            //                    }
            //                    public static uint se32(uint i)
            //                    {
            //                        return ((i & 0xFF000000) >> 24) | ((i & 0xFF0000) >> 8) | ((i & 0xFF00) << 8) | ((i & 0xFF) << 24);
            //                    }
            //                    public static ulong se64(ulong i)
            //                    {
            //                        return ((i & 0x00000000000000ff) << 56) | ((i & 0x000000000000ff00) << 40) | ((i & 0x0000000000ff0000) << 24) | ((i & 0x00000000ff000000) << 8) | ((i & 0x000000ff00000000) >> 8) | ((i & 0x0000ff0000000000) >> 24) | ((i & 0x00ff000000000000) >> 40) | ((i & 0xff00000000000000) >> 56);
            //                    }

            //                    public static bool isEmpty(byte[] buf, int buf_size)
            //                    {
            //                        if (buf != null)
            //                        {
            //                            int i;
            //                            for (i = 0; i < buf_size; i++)
            //                            {
            //                                if (buf[i] != 0)
            //                                {
            //                                    return false;
            //                                }
            //                            }
            //                        }
            //                        return true;
            //                    }
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'hex_str', so pointers on this parameter are left unchanged:
            //                    public static ulong hex_to_u64(char hex_str)
            //                    {
            //                        uint length = hex_str.Length;
            //                        ulong tmp = 0;
            //                        ulong result = 0;
            //                        char c;

            //                        while (length-- != 0)
            //                        {
            //                            c = hex_str++;
            //                            if ((c >= '0') && (c <= '9'))
            //                            {
            //                                tmp = c - '0';
            //                            }
            //                            else if ((c >= 'a') && (c <= 'f'))
            //                            {
            //                                tmp = c - 'a' + 10;
            //                            }
            //                            else if ((c >= 'A') && (c <= 'F'))
            //                            {
            //                                tmp = c - 'A' + 10;
            //                            }
            //                            else
            //                            {
            //                                tmp = 0;
            //                            }
            //                            result |= (tmp << (length * 4));
            //                        }

            //                        return result;
            //                    }
            //                    //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'hex_str', so pointers on this parameter are left unchanged:
            //                    public static void hex_to_bytes(ref byte data, char hex_str, uint str_length)
            //                    {
            //                        uint data_length = str_length / 2;
            //                        char[] tmp_buf = { 0, 0, 0 };

            //                        // Don't convert if the string length is odd.
            //                        if ((str_length % 2) == 0)
            //                        {
            //                            //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
            //                            //ORIGINAL LINE: byte *out = (byte *) malloc(str_length * sizeof(byte));
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
            //                            byte @out = (byte)malloc(str_length * sizeof(byte));
            //                            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged:
            //                            byte* pos = @out;

            //                            while (str_length-- != 0)
            //                            {
            //                                tmp_buf[0] = hex_str++;
            //                                tmp_buf[1] = hex_str++;

            //                                *pos++ = (byte)(hex_to_u64(tmp_buf) & 0xFF);
            //                            }

            //                            // Copy back to our array.
            //                            //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
            //                            memcpy(data, @out, data_length);
            //                        }
            //                    }
            //                    public static bool is_hex(string hex_str, uint str_length)
            //                    {
            //                        const string hex_chars = "0123456789abcdefABCDEF";

            //                        if (hex_str == null)
            //                        {
            //                            return false;
            //                        }

            //                        uint i;
            //                        for (i = 0; i < str_length; i++)
            //                        {
            //                            if (StringFunctions.StrChr(hex_chars, hex_str[i]) == 0)
            //                            {
            //                                return false;
            //                            }
            //                        }

            //                        return true;
            //                    }
            //                    /*
            //                        Draan proudly presents:

            //                        With huge help from community:
            //                        coyotebean, Davee, hitchhikr, kgsws, liquidzigong, Mathieulh, Proxima, SilverSpring

            //                        ******************** KIRK-ENGINE ********************
            //                        An Open-Source implementation of KIRK (PSP crypto engine) algorithms and keys.
            //                        Includes also additional routines for hash forging.

            //                        ********************

            //                        This program is free software: you can redistribute it and/or modify
            //                        it under the terms of the GNU General Public License as published by
            //                        the Free Software Foundation, either version 3 of the License, or
            //                        (at your option) any later version.

            //                        This program is distributed in the hope that it will be useful,
            //                        but WITHOUT ANY WARRANTY; without even the implied warranty of
            //                        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
            //                        GNU General Public License for more details.

            //                        You should have received a copy of the GNU General Public License
            //                        along with this program.  If not, see <http://www.gnu.org/licenses/>.
            //                    */


            //                    // KIRK AES keys
            //                    internal static byte[] kirk1_key = { 0x98, 0xC9, 0x40, 0x97, 0x5C, 0x1D, 0x10, 0xE8, 0x7F, 0xE6, 0x0E, 0xA3, 0xFD, 0x03, 0xA8, 0xBA };
            //                    internal static byte[] kirk7_key02 = { 0xB8, 0x13, 0xC3, 0x5E, 0xC6, 0x44, 0x41, 0xE3, 0xDC, 0x3C, 0x16, 0xF5, 0xB4, 0x5E, 0x64, 0x84 };
            //                    internal static byte[] kirk7_key03 = { 0x98, 0x02, 0xC4, 0xE6, 0xEC, 0x9E, 0x9E, 0x2F, 0xFC, 0x63, 0x4C, 0xE4, 0x2F, 0xBB, 0x46, 0x68 };
            //                    internal static byte[] kirk7_key04 = { 0x99, 0x24, 0x4C, 0xD2, 0x58, 0xF5, 0x1B, 0xCB, 0xB0, 0x61, 0x9C, 0xA7, 0x38, 0x30, 0x07, 0x5F };
            //                    internal static byte[] kirk7_key05 = { 0x02, 0x25, 0xD7, 0xBA, 0x63, 0xEC, 0xB9, 0x4A, 0x9D, 0x23, 0x76, 0x01, 0xB3, 0xF6, 0xAC, 0x17 };
            //                    internal static byte[] kirk7_key07 = { 0x76, 0x36, 0x8B, 0x43, 0x8F, 0x77, 0xD8, 0x7E, 0xFE, 0x5F, 0xB6, 0x11, 0x59, 0x39, 0x88, 0x5C };
            //                    internal static byte[] kirk7_key0C = { 0x84, 0x85, 0xC8, 0x48, 0x75, 0x08, 0x43, 0xBC, 0x9B, 0x9A, 0xEC, 0xA7, 0x9C, 0x7F, 0x60, 0x18 };
            //                    internal static byte[] kirk7_key0D = { 0xB5, 0xB1, 0x6E, 0xDE, 0x23, 0xA9, 0x7B, 0x0E, 0xA1, 0x7C, 0xDB, 0xA2, 0xDC, 0xDE, 0xC4, 0x6E };
            //                    internal static byte[] kirk7_key0E = { 0xC8, 0x71, 0xFD, 0xB3, 0xBC, 0xC5, 0xD2, 0xF2, 0xE2, 0xD7, 0x72, 0x9D, 0xDF, 0x82, 0x68, 0x82 };
            //                    internal static byte[] kirk7_key0F = { 0x0A, 0xBB, 0x33, 0x6C, 0x96, 0xD4, 0xCD, 0xD8, 0xCB, 0x5F, 0x4B, 0xE0, 0xBA, 0xDB, 0x9E, 0x03 };
            //                    internal static byte[] kirk7_key10 = { 0x32, 0x29, 0x5B, 0xD5, 0xEA, 0xF7, 0xA3, 0x42, 0x16, 0xC8, 0x8E, 0x48, 0xFF, 0x50, 0xD3, 0x71 };
            //                    internal static byte[] kirk7_key11 = { 0x46, 0xF2, 0x5E, 0x8E, 0x4D, 0x2A, 0xA5, 0x40, 0x73, 0x0B, 0xC4, 0x6E, 0x47, 0xEE, 0x6F, 0x0A };
            //                    internal static byte[] kirk7_key12 = { 0x5D, 0xC7, 0x11, 0x39, 0xD0, 0x19, 0x38, 0xBC, 0x02, 0x7F, 0xDD, 0xDC, 0xB0, 0x83, 0x7D, 0x9D };
            //                    internal static byte[] kirk7_key38 = { 0x12, 0x46, 0x8D, 0x7E, 0x1C, 0x42, 0x20, 0x9B, 0xBA, 0x54, 0x26, 0x83, 0x5E, 0xB0, 0x33, 0x03 };
            //                    internal static byte[] kirk7_key39 = { 0xC4, 0x3B, 0xB6, 0xD6, 0x53, 0xEE, 0x67, 0x49, 0x3E, 0xA9, 0x5F, 0xBC, 0x0C, 0xED, 0x6F, 0x8A };
            //                    internal static byte[] kirk7_key3A = { 0x2C, 0xC3, 0xCF, 0x8C, 0x28, 0x78, 0xA5, 0xA6, 0x63, 0xE2, 0xAF, 0x2D, 0x71, 0x5E, 0x86, 0xBA };
            //                    internal static byte[] kirk7_key44 = { 0x7D, 0xF4, 0x92, 0x65, 0xE3, 0xFA, 0xD6, 0x78, 0xD6, 0xFE, 0x78, 0xAD, 0xBB, 0x3D, 0xFB, 0x63 };
            //                    internal static byte[] kirk7_key4B = { 0x0C, 0xFD, 0x67, 0x9A, 0xF9, 0xB4, 0x72, 0x4F, 0xD7, 0x8D, 0xD6, 0xE9, 0x96, 0x42, 0x28, 0x8B };
            //                    internal static byte[] kirk7_key53 = { 0xAF, 0xFE, 0x8E, 0xB1, 0x3D, 0xD1, 0x7E, 0xD8, 0x0A, 0x61, 0x24, 0x1C, 0x95, 0x92, 0x56, 0xB6 };
            //                    internal static byte[] kirk7_key57 = { 0x1C, 0x9B, 0xC4, 0x90, 0xE3, 0x06, 0x64, 0x81, 0xFA, 0x59, 0xFD, 0xB6, 0x00, 0xBB, 0x28, 0x70 };
            //                    internal static byte[] kirk7_key5D = { 0x11, 0x5A, 0x5D, 0x20, 0xD5, 0x3A, 0x8D, 0xD3, 0x9C, 0xC5, 0xAF, 0x41, 0x0F, 0x0F, 0x18, 0x6F };
            //                    internal static byte[] kirk7_key63 = { 0x9C, 0x9B, 0x13, 0x72, 0xF8, 0xC6, 0x40, 0xCF, 0x1C, 0x62, 0xF5, 0xD5, 0x92, 0xDD, 0xB5, 0x82 };
            //                    internal static byte[] kirk7_key64 = { 0x03, 0xB3, 0x02, 0xE8, 0x5F, 0xF3, 0x81, 0xB1, 0x3B, 0x8D, 0xAA, 0x2A, 0x90, 0xFF, 0x5E, 0x61 };
            //                    internal static byte[] kirk16_key = { 0x47, 0x5E, 0x09, 0xF4, 0xA2, 0x37, 0xDA, 0x9B, 0xEF, 0xFF, 0x3B, 0xC0, 0x77, 0x14, 0x3D, 0x8A };

            //                    /* ECC Curves for Kirk 1 and Kirk 0x11 */
            //                    // Common Curve paramters p and a
            //                    internal static byte[] ec_p = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            //                    internal static byte[] ec_a = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFC }; // mon

            //                    // Kirk 0xC,0xD,0x10,0x11,(likely 0x12)- Unique curve parameters for b, N, and base point G for Kirk 0xC,0xD,0x10,0x11,(likely 0x12) service
            //                    // Since public key is variable, it is not specified here
            //                    internal static byte[] ec_b2 = { 0xA6, 0x8B, 0xED, 0xC3, 0x34, 0x18, 0x02, 0x9C, 0x1D, 0x3C, 0xE3, 0x3B, 0x9A, 0x32, 0x1F, 0xCC, 0xBB, 0x9E, 0x0F, 0x0B }; // mon
            //                    internal static byte[] ec_N2 = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0xFF, 0xFF, 0xB5, 0xAE, 0x3C, 0x52, 0x3E, 0x63, 0x94, 0x4F, 0x21, 0x27 };
            //                    internal static byte[] Gx2 = { 0x12, 0x8E, 0xC4, 0x25, 0x64, 0x87, 0xFD, 0x8F, 0xDF, 0x64, 0xE2, 0x43, 0x7B, 0xC0, 0xA1, 0xF6, 0xD5, 0xAF, 0xDE, 0x2C };
            //                    internal static byte[] Gy2 = { 0x59, 0x58, 0x55, 0x7E, 0xB1, 0xDB, 0x00, 0x12, 0x60, 0x42, 0x55, 0x24, 0xDB, 0xC3, 0x79, 0xD5, 0xAC, 0x5F, 0x4A, 0xDF };

            //                    // KIRK 1 - Unique curve parameters for b, N, and base point G
            //                    // Since public key is hard coded, it is also included
            //                    internal static byte[] ec_b1 = { 0x65, 0xD1, 0x48, 0x8C, 0x03, 0x59, 0xE2, 0x34, 0xAD, 0xC9, 0x5B, 0xD3, 0x90, 0x80, 0x14, 0xBD, 0x91, 0xA5, 0x25, 0xF9 };
            //                    internal static byte[] ec_N1 = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01, 0xB5, 0xC6, 0x17, 0xF2, 0x90, 0xEA, 0xE1, 0xDB, 0xAD, 0x8F };
            //                    internal static byte[] Gx1 = { 0x22, 0x59, 0xAC, 0xEE, 0x15, 0x48, 0x9C, 0xB0, 0x96, 0xA8, 0x82, 0xF0, 0xAE, 0x1C, 0xF9, 0xFD, 0x8E, 0xE5, 0xF8, 0xFA };
            //                    internal static byte[] Gy1 = { 0x60, 0x43, 0x58, 0x45, 0x6D, 0x0A, 0x1C, 0xB2, 0x90, 0x8D, 0xE9, 0x0F, 0x27, 0xD7, 0x5C, 0x82, 0xBE, 0xC1, 0x08, 0xC0 };

            //                    internal static byte[] Px1 = { 0xED, 0x9C, 0xE5, 0x82, 0x34, 0xE6, 0x1A, 0x53, 0xC6, 0x85, 0xD6, 0x4D, 0x51, 0xD0, 0x23, 0x6B, 0xC3, 0xB5, 0xD4, 0xB9 };
            //                    internal static byte[] Py1 = { 0x04, 0x9D, 0xF1, 0xA0, 0x75, 0xC0, 0xE0, 0x4F, 0xB3, 0x44, 0x85, 0x8B, 0x61, 0xB7, 0x9B, 0x69, 0xA6, 0x3D, 0x2C, 0x39 };


            //                    public static byte module_ver_lo;
            //                    public static byte module_ver_hi;
            //                    public static string modname = new string(new char[28]);
            //                    public static byte version; // 26
            //                    public static byte nsegments; // 27
            //                    public static int elf_size; // 28
            //                    public static int psp_size; // 2C
            //                    public static uint entry; // 30
            //                    public static uint modinfo_offset; // 34
            //                    public static int bss_size; // 38
            //                    public static ushort[] seg_align = new ushort[4]; // 3C
            //                    public static uint[] seg_address = new uint[4]; // 44
            //                    public static int[] seg_size = new int[4]; // 54
            //                    public static uint[] reserved = new uint[5]; // 64
            //                    public static uint devkitversion; // 78
            //                    public static uint decrypt_mode; // 7C
            //                    public static byte[] key_data0 = new byte[0x30]; // 80
            //                    public static int comp_size; // B0
            //                    public static int _80; // B4
            //                    public static int[] reserved2 = new int[2]; // B8
            //                    public static byte[] key_data1 = new byte[0x10]; // C0
            //                    public static uint tag; // D0
            //                    public static byte[] scheck = new byte[0x58]; // D4
            //                    public static uint key_data2; // 12C
            //                    public static uint oe_tag; // 130
            //                    public static byte[] key_data3 = new byte[0x1C]; // 134
            //                }
            //                //C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
            //                //PSP_Header;
            //            }


            // }


            //


            //


            /*Build PBP File Powered By Leecherman*/

            public unsafe class PSN
            {

                public static PARAM_SFO sfo = new PARAM_SFO(@"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\PARAM.SFO");

                #region << Structs >>
                [Serializable]
                public class NPUMDIMG_HEADER_BODY
                {
                    public ushort sector_size; // 0x0800
                    public ushort unk_2; // 0xE000
                    public uint unk_4;
                    public uint unk_8;
                    public uint unk_12;
                    public uint unk_16;
                    public uint lba_start;
                    public uint unk_24;
                    public uint nsectors;
                    public uint unk_32;
                    public uint lba_end;
                    public uint unk_40;
                    public uint block_entry_offset;
                    public string disc_id = new string(new char[0x10]);
                    public uint header_start_offset;
                    public uint unk_68;
                    public byte unk_72;
                    public byte bbmac_param;
                    public byte unk_74;
                    public byte unk_75;
                    public uint unk_76;
                    public uint unk_80;
                    public uint unk_84;
                    public uint unk_88;
                    public uint unk_92;
                }
                [Serializable]
                public class NPUMDIMG_HEADER
                {
                    public byte[] magic = new byte[0x08]; // NPUMDIMG
                    public uint np_flags;
                    public uint block_basis;
                    public byte[] content_id = new byte[0x30];
                    public NPUMDIMG_HEADER_BODY body = new NPUMDIMG_HEADER_BODY();
                    public byte[] header_key = new byte[0x10];
                    public byte[] data_key = new byte[0x10];
                    public byte[] header_hash = new byte[0x10];
                    public byte[] padding = new byte[0x8];
                    public byte[] ecdsa_sig = new byte[0x28];
                }

                internal static class DefineConstants
                {
                    public const int KIRK_OPERATION_SUCCESS = 0;
                    public const int KIRK_NOT_ENABLED = 1;
                    public const int KIRK_INVALID_MODE = 2;
                    public const int KIRK_HEADER_HASH_INVALID = 3;
                    public const int KIRK_DATA_HASH_INVALID = 4;
                    public const int KIRK_SIG_CHECK_INVALID = 5;
                    public const int KIRK_UNK_1 = 6;
                    public const int KIRK_UNK_2 = 7;
                    public const int KIRK_UNK_3 = 8;
                    public const int KIRK_UNK_4 = 9;
                    public const int KIRK_UNK_5 = 0xA;
                    public const int KIRK_UNK_6 = 0xB;
                    public const int KIRK_NOT_INITIALIZED = 0xC;
                    public const int KIRK_INVALID_OPERATION = 0xD;
                    public const int KIRK_INVALID_SEED_CODE = 0xE;
                    public const int KIRK_INVALID_SIZE = 0xF;
                    public const int KIRK_DATA_SIZE_ZERO = 0x10;
                    public const int KIRK_CMD_DECRYPT_PRIVATE = 1;
                    public const int KIRK_CMD_2 = 2;
                    public const int KIRK_CMD_3 = 3;
                    public const int KIRK_CMD_ENCRYPT_IV_0 = 4;
                    public const int KIRK_CMD_ENCRYPT_IV_FUSE = 5;
                    public const int KIRK_CMD_ENCRYPT_IV_USER = 6;
                    public const int KIRK_CMD_DECRYPT_IV_0 = 7;
                    public const int KIRK_CMD_DECRYPT_IV_FUSE = 8;
                    public const int KIRK_CMD_DECRYPT_IV_USER = 9;
                    public const int KIRK_CMD_PRIV_SIGN_CHECK = 10;
                    public const int KIRK_CMD_SHA1_HASH = 11;
                    public const int KIRK_CMD_ECDSA_GEN_KEYS = 12;
                    public const int KIRK_CMD_ECDSA_MULTIPLY_POINT = 13;
                    public const int KIRK_CMD_PRNG = 14;
                    public const int KIRK_CMD_15 = 15;
                    public const int KIRK_CMD_ECDSA_SIGN = 16;
                    public const int KIRK_CMD_ECDSA_VERIFY = 17;
                    public const int KIRK_MODE_CMD1 = 1;
                    public const int KIRK_MODE_CMD2 = 2;
                    public const int KIRK_MODE_CMD3 = 3;
                    public const int KIRK_MODE_ENCRYPT_CBC = 4;
                    public const int KIRK_MODE_DECRYPT_CBC = 5;
                    public const int SUBCWR_NOT_16_ALGINED = 0x90A;
                    public const int SUBCWR_HEADER_HASH_INVALID = 0x920;
                    public const int SUBCWR_BUFFER_TOO_SMALL = 0x1000;
                    public const int AES_KEY_LEN_128 = 128;
                    public const int AES_KEY_LEN_192 = 192;
                    public const int AES_KEY_LEN_256 = 256;
                    public const int AES_BUFFER_SIZE = 16;
                    public const int AES_MAXKEYBITS = 256;
                    public const int AES_MAXROUNDS = 14;
                    public const int AES_128 = 0;
                    public const int _GLOBAL_H_ = 1;
                    public const int FALSE = 0;
                    public const int _SHA_H_ = 1;
                    public const int _ENDIAN_H_ = 1;
                    public const int SHS_DATASIZE = 64;
                    public const int SHS_DIGESTSIZE = 20;
                    public const int K1 = 0x5A827999; // Rounds  0-19
                    public const int K2 = 0x6ED9EBA1; // Rounds 20-39
                    public const uint K3 = 0x8F1BBCDC; // Rounds 40-59
                    public const uint K4 = 0xCA62C1D6; // Rounds 60-79
                    public const int h0init = 0x67452301;
                    public const uint h1init = 0xEFCDAB89;
                    public const uint h2init = 0x98BADCFE;
                    public const int h3init = 0x10325476;
                    public const uint h4init = 0xC3D2E1F0;
                    public const int PT_LOAD = 1; // Loadable segment.
                    public const int PF_X = 0x1; // Executable.
                    public const int PF_W = 0x2; // Writable.
                    public const int PF_R = 0x4; // Readable.
                    public const int SECTOR_SIZE = 0x800;
                    public const int ISO9660_FILEFLAGS_FILE = 1;
                    public const int ISO9660_FILEFLAGS_DIR = 2;
                    public const int MAX_RETRIES = 1;
                    public const int MAX_DIR_LEVEL = 8;
                    public const int CISO_IDX_BUFFER_SIZE = 0x200;
                    public const int CISO_DEC_BUFFER_SIZE = 0x2000;
                    public const string ISO_STANDARD_ID = "CD001";
                    public const int RATIO_LIMIT = 90;
                    public const int PSF_MAGIC = 0x46535000;
                }

                public class KIRK_CMD1_HEADER
                {
                    public byte[] AES_key = new byte[16];
                    public byte[] CMAC_key = new byte[16];
                    public byte[] CMAC_header_hash = new byte[16];
                    public byte[] CMAC_data_hash = new byte[16];
                    public byte[] unused = new byte[32];
                    public uint mode;
                    public byte ecdsa_hash;
                    public byte[] unk3 = new byte[11];
                    public uint data_size;
                    public uint data_offset;
                    public byte[] unk4 = new byte[8];
                    public byte[] unk5 = new byte[16];
                }

                #endregion << Structs >>

                public class header_keys
                {
                    public byte[] AES = new byte[16];
                    public byte[] CMAC = new byte[16];
                }

                public static NPUMDIMG_HEADER forge_npumdimg(int iso_size, int iso_blocks, int block_basis, ref string discid, ref string content_id, int np_flags, ref byte version_key, ref byte[] header_key, ref byte[] data_key)
                {
                    // Build NPUMDIMG header.
                    NPUMDIMG_HEADER np_header = new NPUMDIMG_HEADER();

                    //just testing write to file see what it pops out 


                    //c# style binarry writer is amazing !
                    BinaryWriter br = new BinaryWriter(new FileStream(@"C:\\temp\temp.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite));



                    // Set magic NPUMDIMG.
                    np_header.magic[0] = 0x4E;
                    np_header.magic[1] = 0x50;
                    np_header.magic[2] = 0x55;
                    np_header.magic[3] = 0x4D;
                    np_header.magic[4] = 0x44;
                    np_header.magic[5] = 0x49;
                    np_header.magic[6] = 0x4D;
                    np_header.magic[7] = 0x47;
                    br.Write(np_header.magic);

                    // Set flags and block basis.
                    np_header.np_flags = (uint)np_flags;
                    br.Write(np_header.np_flags);

                    np_header.block_basis = (uint)block_basis;
                    br.Write(np_header.block_basis);

                    // Set content ID.
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(np_header.content_id, content_id, content_id.Length);
                    np_header.content_id = Encoding.ASCII.GetBytes(content_id);
                    br.Write(np_header.content_id);
                    // Set inner body parameters.
                    np_header.body.sector_size = 0x800;
                    br.Write(np_header.body.sector_size);
                    if (iso_size > 0x40000000)
                    {
                        np_header.body.unk_2 = 0xE001;
                        br.Write(np_header.body.unk_2);
                    }
                    else
                    {
                        np_header.body.unk_2 = 0xE000;

                        br.Write(np_header.body.unk_2);
                    }

                    np_header.body.unk_4 = 0x0;

                    br.Write(np_header.body.unk_4);
                    np_header.body.unk_8 = 0x1010;

                    br.Write(np_header.body.unk_8);
                    np_header.body.unk_12 = 0x0;

                    br.Write(np_header.body.unk_12);
                    np_header.body.unk_16 = 0x0;

                    br.Write(np_header.body.unk_16);
                    np_header.body.lba_start = 0x0;

                    br.Write(np_header.body.lba_start);
                    np_header.body.unk_24 = 0x0;

                    br.Write(np_header.body.unk_24);

                    if (((iso_blocks * block_basis) - 1) > 0x6C0BF)
                    {
                        np_header.body.nsectors = 0x6C0BF;

                        br.Write(np_header.body.nsectors);
                    }
                    else
                    {
                        np_header.body.nsectors = (uint)((uint)iso_blocks * (uint)block_basis) - 1;

                        br.Write(np_header.body.nsectors);
                    }

                    np_header.body.unk_32 = 0x0;

                    br.Write(np_header.body.unk_32);
                    np_header.body.lba_end = (uint)((uint)iso_blocks * (uint)block_basis) - 1;

                    br.Write(np_header.body.lba_end);
                    np_header.body.unk_40 = 0x01003FFE;

                    br.Write(np_header.body.unk_40);
                    np_header.body.block_entry_offset = 0x100;

                    br.Write(np_header.body.block_entry_offset);

                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(np_header.body.disc_id, content_id.Substring(7), 4);

                    np_header.body.disc_id = content_id.Substring(7);

                    np_header.body.disc_id = StringFunctions.ChangeCharacter(np_header.body.disc_id, 4, '-');
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(np_header.body.disc_id.Substring(5), content_id.Substring(11), 5);
                    np_header.body.disc_id = discid;

                    br.Write(np_header.body.disc_id);
                    np_header.body.header_start_offset = 0x0;

                    br.Write(np_header.body.header_start_offset);
                    np_header.body.unk_68 = 0x0;

                    br.Write(np_header.body.unk_68);
                    np_header.body.unk_72 = 0x0;

                    br.Write(np_header.body.unk_72);
                    np_header.body.bbmac_param = 0x0;

                    br.Write(np_header.body.bbmac_param);
                    np_header.body.unk_74 = 0x0;

                    br.Write(np_header.body.unk_74);
                    np_header.body.unk_75 = 0x0;

                    br.Write(np_header.body.unk_75);
                    np_header.body.unk_76 = 0x0;

                    br.Write(np_header.body.unk_76);
                    np_header.body.unk_80 = 0x0;

                    br.Write(np_header.body.unk_80);
                    np_header.body.unk_84 = 0x0;

                    br.Write(np_header.body.unk_84);
                    np_header.body.unk_88 = 0x0;

                    br.Write(np_header.body.unk_88);
                    np_header.body.unk_92 = 0x0;

                    br.Write(np_header.body.unk_92);

                    // Set keys.
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(np_header.header_key, 0, 0x10);
                    np_header.header_key = new byte[10];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(np_header.data_key, 0, 0x10);
                    np_header.data_key = new byte[10];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(np_header.header_hash, 0, 0x10);
                    np_header.header_hash = new byte[10];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(np_header.padding, 0, 0x8);
                    np_header.padding = new byte[8];
                    // Copy header and data keys.
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(np_header.header_key, header_key, 0x10);
                    
                    np_header.header_key = header_key;

                    br.Write(np_header.header_key);
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(np_header.data_key, data_key, 0x10);
                    np_header.data_key = data_key;

                    br.Write(np_header.data_key);

                    br.Close();
                    Console.WriteLine("Header Forged");
                    return np_header;
                    // Generate random padding.
                    //sceUtilsBufferCopyWithRange(ref np_header.padding, 0x8, 0, 0, DefineConstants.KIRK_CMD_PRNG);

                    //// Prepare buffers to encrypt the NPUMDIMG body.
                    //MAC_KEY mck = new MAC_KEY();
                    //CIPHER_KEY bck = new CIPHER_KEY();

                    //// Encrypt NPUMDIMG body.
                    //sceDrmBBCipherInit(bck, 1, 2, ref np_header.header_key, ref version_key, 0);
                    //sceDrmBBCipherUpdate(bck, ref (byte)(np_header) + 0x40, 0x60);
                    //sceDrmBBCipherFinal(bck);

                    //// Generate header hash.
                    //sceDrmBBMacInit(mck, 3);
                    //sceDrmBBMacUpdate(mck, ref (byte)np_header, 0xC0);
                    //sceDrmBBMacFinal(mck, ref np_header.header_hash, ref version_key);
                    //bbmac_build_final2(3, ref np_header.header_hash);

                    //// Prepare the signature hash input buffer.
                    //byte[] npumdimg_sha1_inbuf = new byte[0xD8 + 0x4];
                    //byte[] npumdimg_sha1_outbuf = new byte[0x14];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(npumdimg_sha1_inbuf, 0, 0xD8 + 0x4);
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(npumdimg_sha1_outbuf, 0, 0x14);

                    //// Set SHA1 data size.
                    //npumdimg_sha1_inbuf[0] = 0xD8;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(npumdimg_sha1_inbuf + 0x4, (byte)np_header, 0xD8);

                    //// Hash the input buffer.
                    //if (sceUtilsBufferCopyWithRange(ref npumdimg_sha1_outbuf, 0x14, ref npumdimg_sha1_inbuf, 0xD8 + 0x4, DefineConstants.KIRK_CMD_SHA1_HASH) != 0)
                    //{
                    //    Console.Write("ERROR: Failed to generate SHA1 hash for NPUMDIMG header!\n");
                    //    return null;
                    //}

                    //// Prepare ECDSA signature buffer.
                    //byte[] npumdimg_sign_buf_in = new byte[0x34];
                    //byte[] npumdimg_sign_buf_out = new byte[0x28];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(npumdimg_sign_buf_in, 0, 0x34);
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(npumdimg_sign_buf_out, 0, 0x28);

                    //// Create ECDSA key pair.
                    //byte[] npumdimg_keypair = new byte[0x3C];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(npumdimg_keypair, npumdimg_private_key, 0x14);
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(npumdimg_keypair + 0x14, npumdimg_public_key, 0x28);

                    //// Encrypt NPUMDIMG private key.
                    //byte[] npumdimg_private_key_enc = new byte[0x20];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(npumdimg_private_key_enc, 0, 0x20);
                    //encrypt_kirk16_private(ref npumdimg_private_key_enc, ref npumdimg_keypair);

                    //// Generate ECDSA signature.
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(npumdimg_sign_buf_in, npumdimg_private_key_enc, 0x20);
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(npumdimg_sign_buf_in + 0x20, npumdimg_sha1_outbuf, 0x14);
                    //if (sceUtilsBufferCopyWithRange(ref npumdimg_sign_buf_out, 0x28, ref npumdimg_sign_buf_in, 0x34, DefineConstants.KIRK_CMD_ECDSA_SIGN) != 0)
                    //{
                    //    Console.Write("ERROR: Failed to generate ECDSA signature for NPUMDIMG header!\n");
                    //    return null;
                    //}

                    //// Verify the generated ECDSA signature.
                    //byte[] test_npumdimg_sign = new byte[0x64];
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(test_npumdimg_sign, npumdimg_public_key, 0x28);
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(test_npumdimg_sign + 0x28, npumdimg_sha1_outbuf, 0x14);
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(test_npumdimg_sign + 0x3C, npumdimg_sign_buf_out, 0x28);
                    //if (sceUtilsBufferCopyWithRange(0, 0, ref test_npumdimg_sign, 0x64, DefineConstants.KIRK_CMD_ECDSA_VERIFY) != 0)
                    //{
                    //    Console.Write("ERROR: ECDSA signature for NPUMDIMG header is invalid!\n");
                    //    return null;
                    //}
                    //else
                    //{
                    //    Console.Write("ECDSA signature for NPUMDIMG header is valid!\n");
                    //}

                    //// Store the signature.
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(np_header.ecdsa_sig, npumdimg_sign_buf_out, 0x28);

                    return np_header;
                }


                //I need to take another course on cryptogrphy 
                public static void Create_DATA_PSARC(long param_sfo_size, long icon0_size, long icon1_size, long pic0_size, long pic1_size, long snd0_size, long data_psp_size)
                {

                    //Build empty DATA.PSAR.
                    Console.Write("Building DATA.PSAR...\n");
                    int data_psar_size = 0x100;
                    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                    //ORIGINAL LINE: byte *data_psar_buf = (byte *) malloc(data_psar_size);
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
                    byte[] data_psar_buf = new byte[(data_psar_size)];
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //memset(data_psar_buf, 0, data_psar_size);
                    UMD_Util.Memset(data_psar_buf, 0, data_psar_size);

                    // Calculate header size.
                    long header_size = icon0_size + icon1_size + pic0_size + pic1_size + snd0_size + param_sfo_size + data_psp_size;

                    // Allocate PBP header.
                    //C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
                    //ORIGINAL LINE: byte *pbp_header = malloc(header_size + 4096);
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
                    byte[] pbp_header = new byte[header_size + 4096];
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
                    //UMD_Util.Memset(pbp_header, 0, header_size + 4096);
                    BinaryWriter br = new BinaryWriter(new FileStream(@"C:\\temp\temp.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite));
                    // Write magic.
                    //(uint)(pbp_header + 0) = 0x50425000;
                    br.Write(0x50425000);
                    //(uint)(pbp_header + 4) = 0x00010001;
                    br.Write(0x00010001);
                    // Set header offset.
                    int header_offset = 0x28;

                    // Write PARAM.SFO
                    if (param_sfo_size != 0)
                    {
                        Console.Write("Writing PARAM.SFO...\n");
                    }
                    //(uint)(pbp_header + 0x08) = header_offset;
                    br.Write(header_offset);
                    //C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, param_sfo_buf, param_sfo_size);
                    // UMD_Util.Memset(pbp_header + header_offset)


                    //now write the param.sfo 
                    //br.Write()
                    var utf8 = new UTF8Encoding(false);//encoding
                    BinaryWriter writer = br;
                    //so lets start writing the info
                    writer.Write(PARAM_SFO.Header.Magic);//write magic "\0PSF" 
                    writer.Write(PARAM_SFO.Header.version);//write version info this is mayjor and minor (01 01 00 00	1.01)
                    PARAM_SFO.Header.KeyTableStart = 0x14 + PARAM_SFO.Header.IndexTableEntries * 0x10;/*we can write all this lovely info from the tables back*/
                    writer.Write(PARAM_SFO.Header.KeyTableStart);

                    PARAM_SFO.Header.DataTableStart = Convert.ToUInt32(PARAM_SFO.Header.KeyTableStart + sfo.Tables.Sum(i => i.Name.Length + 1));//needs to be Uint
                    if (PARAM_SFO.Header.DataTableStart % 4 != 0)
                        PARAM_SFO.Header.DataTableStart = (PARAM_SFO.Header.DataTableStart / 4 + 1) * 4;
                    writer.Write(PARAM_SFO.Header.DataTableStart);
                    PARAM_SFO.Header.IndexTableEntries = Convert.ToUInt32(sfo.Tables.Count);
                    writer.Write(PARAM_SFO.Header.IndexTableEntries);

                    int lastKeyOffset = Convert.ToInt32(PARAM_SFO.Header.KeyTableStart);
                    int lastValueOffset = Convert.ToInt32(PARAM_SFO.Header.DataTableStart);
                    for (var i = 0; i < sfo.Tables.Count; i++)
                    {
                        var entry = sfo.Tables[i];

                        writer.BaseStream.Seek(0x14 + i * 0x10, SeekOrigin.Begin);
                        writer.Write((ushort)(lastKeyOffset - PARAM_SFO.Header.KeyTableStart));


                        writer.Write((ushort)entry.Indextable.param_data_fmt);

                        writer.Write(entry.Indextable.param_data_len);
                        writer.Write(entry.Indextable.param_data_max_len);
                        writer.Write(lastValueOffset - PARAM_SFO.Header.DataTableStart);

                        writer.BaseStream.Seek(lastKeyOffset, SeekOrigin.Begin);
                        writer.Write(utf8.GetBytes(entry.Name));
                        writer.Write((byte)0);
                        lastKeyOffset = (int)writer.BaseStream.Position;

                        writer.BaseStream.Seek(lastValueOffset, SeekOrigin.Begin);
                        writer.Write(entry.ValueBuffer);
                        lastValueOffset = (int)writer.BaseStream.Position;
                    }

                    //I'm doing this to just rewrite the first item (Some Cleanup will be needed)
                    //Or maybe not as when I checked this gives a 1 - 1 match with how the Sony tool works
                    //we need to rewrite that first item (PS4/PS3/PSV should be APP-VER)
                    lastKeyOffset = Convert.ToInt32(PARAM_SFO.Header.KeyTableStart);
                    lastValueOffset = Convert.ToInt32(PARAM_SFO.Header.DataTableStart);

                    var tableentry = sfo.Tables[0];

                    writer.BaseStream.Seek(lastKeyOffset, SeekOrigin.Begin);
                    writer.Write(utf8.GetBytes(tableentry.Name));
                    writer.Write((byte)0);
                    lastKeyOffset = (int)writer.BaseStream.Position;
                    br.Close();
                    Console.WriteLine("Debuging WriterComplete");
                    return;

                    //header_offset += param_sfo_size;

                    //// Write ICON0.PNG
                    //if (icon0_size != 0)
                    //{
                    //    Console.Write("Writing ICON0.PNG...\n");
                    //}
                    //(uint)(pbp_header + 0x0C) = header_offset;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, icon0_buf, icon0_size);
                    //header_offset += icon0_size;

                    //// Write ICON1.PMF
                    //if (icon1_size != 0)
                    //{
                    //    Console.Write("Writing ICON1.PNG...\n");
                    //}
                    //(uint)(pbp_header + 0x10) = header_offset;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, icon1_buf, icon1_size);
                    //header_offset += icon1_size;

                    //// Write PIC0.PNG
                    //if (pic0_size != 0)
                    //{
                    //    Console.Write("Writing PIC0.PNG...\n");
                    //}
                    //(uint)(pbp_header + 0x14) = header_offset;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, pic0_buf, pic0_size);
                    //header_offset += pic0_size;

                    //// Write PIC1.PNG
                    //if (pic1_size != 0)
                    //{
                    //    Console.Write("Writing PIC1.PNG...\n");
                    //}
                    //(uint)(pbp_header + 0x18) = header_offset;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, pic1_buf, pic1_size);
                    //header_offset += pic1_size;

                    //// Write SND0.AT3
                    //if (snd0_size != 0)
                    //{
                    //    Console.Write("Writing SND0.AT3...\n");
                    //}
                    //(uint)(pbp_header + 0x1C) = header_offset;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, snd0_buf, snd0_size);
                    //header_offset += snd0_size;

                    //// Write DATA.PSP
                    //Console.Write("Writing DATA.PSP...\n");
                    //(uint)(pbp_header + 0x20) = header_offset;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, data_psp_buf, data_psp_size);
                    //header_offset += data_psp_size;

                    //// DATA.PSAR is 0x100 aligned.
                    //header_offset = (header_offset + 15) & ~15;
                    //while (header_offset % 0x100 != 0)
                    //{
                    //    header_offset += 0x10;
                    //}

                    //// Write DATA.PSAR
                    //Console.Write("Writing DATA.PSAR...\n\n");
                    //(uint)(pbp_header + 0x24) = header_offset;
                    ////C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
                    //memcpy(pbp_header + header_offset, data_psar_buf, data_psar_size);
                    //header_offset += data_psar_size;

                }

                public void Create_PSP_Signed(string ISOFile,string OutPutLocation,string cid)
                {

                    // Set version, header and data keys.
                    int use_version_key = 0;
                    byte[] version_key = new byte[0x10];
                    byte[] header_key = new byte[0x10];
                    byte[] data_key = new byte[0x10];

                    // Get Content ID from input.
                    string content_id = cid;

                    var iso_size = new FileInfo(ISOFile).Length;

                    // Initialize KIRK.
                    Console.Write("Initializing KIRK engine...\n\n");
                    CSPspEmu.Core.Components.Crypto.Kirk kirk = new CSPspEmu.Core.Components.Crypto.Kirk();
                    
                    
                    byte pgd_buf = 0;
                    int pgd_size = 0;

                    //// Set keys' context.
                    //MAC_KEY mkey = new MAC_KEY();
                    //CIPHER_KEY ckey = new CIPHER_KEY();

                    // Set flags and block size data.
                    int np_flags = (use_version_key) != 0 ? 0x2 : (0x3 | (0x01000000));
                    int block_basis = 0x10;
                    int block_size = block_basis * 2048;
                    long iso_blocks = (iso_size + block_size - 1) / block_size;
                    // Generate random header key.
                    //byte* ptr = null;
                    //byte* zerobyte = null;

                    //Marshal.Copy((IntPtr)ptr, header_key, 0, header_key.Length);
                    //Marshal.Copy((IntPtr)zerobyte, new byte[1],0,1);
                    byte[] temp = new byte[0x10];
                    fixed (byte* headerptr = header_key) 
                    fixed (byte* zeroptr = new byte[0]) 

                    kirk.SceUtilsBufferCopyWithRange(headerptr, 0x10,zeroptr , 0, 0xE);

                    // Generate fixed key, if necessary.
                    if (use_version_key == 0)
                    {
                       // kirk.sceNpDrmGetFixedKey(ref version_key, ref content_id, np_flags);
                    }
                }

                //public static void Create_NPUMDIMG_Header()
                //{
                //    BinaryWriter br = new BinaryWriter(new FileStream(@"C:\\temp\temp.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite));

                //    NPUMDIMG_HEADER header = new NPUMDIMG_HEADER();

                //    br.Write(header.magic);
                //    br.Write(header.)

                //}


                

                public class Utils
                {
                    public static System.Drawing.Image ResizeImage(System.Drawing.Image image, System.Drawing.Size size, bool preserveAspectRatio = false)
                    {
                        checked
                        {
                            int width2;
                            int height2;
                            if (preserveAspectRatio)
                            {
                                int width = image.Width;
                                int height = image.Height;
                                float num = (float)size.Width / (float)width;
                                float num2 = (float)size.Height / (float)height;
                                float num3 = (num2 < num) ? num2 : num;
                                width2 = (int)Math.Round((double)(unchecked((float)width * num3)));
                                height2 = (int)Math.Round((double)(unchecked((float)height * num3)));
                            }
                            else
                            {
                                width2 = size.Width;
                                height2 = size.Height;
                            }
                            System.Drawing.Image image2 = new System.Drawing.Bitmap(width2, height2);
                            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image2))
                            {
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                                graphics.DrawImage(image, 0, 0, width2, height2);
                            }
                            return image2;
                        }
                    }

                    public static bool CheckImageSize(string inputimg, System.Drawing.Size imgsize)
                    {
                        using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(inputimg))
                        {
                            if (bitmap.Width == imgsize.Width && bitmap.Height == imgsize.Height)
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    public static string ResizeImagesFilename(string inputimgfile, string outputimgfile, System.Drawing.Size imgsize)
                    {
                        string result = inputimgfile;
                        try
                        {
                            byte[] buffer = File.ReadAllBytes(inputimgfile);
                            System.Drawing.Image image = Utils.ImageConverter.byteArrayToImage(ref buffer, System.Drawing.Imaging.ImageFormat.Png);
                            if (image.Width != imgsize.Width | image.Height != imgsize.Height)
                            {
                                System.Drawing.Image image2 = image;
                                System.Drawing.Size size = new System.Drawing.Size(imgsize.Width, imgsize.Height);
                                System.Drawing.Image image3 = Utils.ResizeImage(image2, size, false);
                                MemoryStream memoryStream = new MemoryStream();
                                image3.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                                File.WriteAllBytes(outputimgfile, memoryStream.ToArray());
                                result = outputimgfile;
                            }
                            else
                            {
                                MemoryStream memoryStream2 = new MemoryStream(buffer);
                                byte[] array = new byte[4];
                                memoryStream2.Read(array, 0, array.Length);
                                if (array[0] != 137 && array[1] != 80 && array[2] != 78 && array[3] != 71)
                                {
                                    memoryStream2 = new MemoryStream();
                                    image.Save(memoryStream2, System.Drawing.Imaging.ImageFormat.Png);
                                    File.WriteAllBytes(outputimgfile, memoryStream2.ToArray());
                                    result = outputimgfile;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        return result;
                    }
                    public class ImageConverter
                    {
                        // Token: 0x06000065 RID: 101 RVA: 0x00020658 File Offset: 0x0001E858
                        public static byte[] imageToByteArray(ref System.Drawing.Image imageIn, System.Drawing.Imaging.ImageFormat fmt)
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            imageIn.Save(memoryStream, fmt);
                            return memoryStream.ToArray();
                        }

                        // Token: 0x06000066 RID: 102 RVA: 0x0002067C File Offset: 0x0001E87C
                        public static System.Drawing.Image byteArrayToImage(ref byte[] byteArrayIn, System.Drawing.Imaging.ImageFormat fmt)
                        {
                            MemoryStream stream = new MemoryStream(byteArrayIn);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                            fmt = image.RawFormat;
                            return image;
                        }
                    }
                }

                internal sealed class Datas
                {
                    // Token: 0x06000230 RID: 560 RVA: 0x00031E2C File Offset: 0x0003002C
                    // Note: this type is marked as 'beforefieldinit'.
                    static Datas()
                    {
                        checked
                        {
                            Datas.pbpDataBytes = new Array[Enum.GetNames(typeof(Datas.pbpData)).Length - 1 + 1];
                            Datas.pbpDataSizes = new long[Enum.GetNames(typeof(Datas.pbpData)).Length - 1 + 1];
                            Datas.pbpDataOffsets = new int[Enum.GetNames(typeof(Datas.pbpData)).Length - 1 + 1];
                            Datas.pbpImages = new System.Drawing.Image[Enum.GetNames(typeof(Datas.pbpDataImage)).Length - 1 + 1];
                            Datas.PbpDatasNames = new string[]
                            {
                    "PARAM.SFO",
                    "ICON0.PNG",
                    "ICON1.PMF",
                    "PIC0.PNG",
                    "PIC1.PNG",
                    "SND0.AT3",
                    "DATA.PSP",
                    "DATA.PSAR"
                            };
                            Datas.GetSetFormType = Datas.FormType.PBPCreator;
                        }
                    }

                    // Token: 0x04000121 RID: 289
                    public static bool abortOperation = false;

                    // Token: 0x04000122 RID: 290
                    public static bool pbpModified = false;

                    // Token: 0x04000123 RID: 291
                    public static bool IsAnotherForm = false;

                    // Token: 0x04000124 RID: 292
                    public static Array[] pbpDataBytes;

                    // Token: 0x04000125 RID: 293
                    public static long[] pbpDataSizes;

                    // Token: 0x04000126 RID: 294
                    public static int[] pbpDataOffsets;

                    // Token: 0x04000127 RID: 295
                    public static System.Drawing.Image[] pbpImages;

                    // Token: 0x04000128 RID: 296
                    public static readonly string[] PbpDatasNames;

                    // Token: 0x04000129 RID: 297
                    public static Datas.FormType GetSetFormType;

                    // Token: 0x0400012A RID: 298
                    public static System.Drawing.Point GetSetMyLocation;

                    // Token: 0x02000025 RID: 37
                    [Flags]
                    public enum pbpData
                    {
                        // Token: 0x0400012C RID: 300
                        sfobytes = 0,
                        // Token: 0x0400012D RID: 301
                        icon0bytes = 1,
                        // Token: 0x0400012E RID: 302
                        icon1bytes = 2,
                        // Token: 0x0400012F RID: 303
                        pic0bytes = 3,
                        // Token: 0x04000130 RID: 304
                        pic1bytes = 4,
                        // Token: 0x04000131 RID: 305
                        at3bytes = 5,
                        // Token: 0x04000132 RID: 306
                        pspbytes = 6,
                        // Token: 0x04000133 RID: 307
                        psardata = 7
                    }

                    // Token: 0x02000026 RID: 38
                    [Flags]
                    public enum pbpDataImage
                    {
                        // Token: 0x04000135 RID: 309
                        icon0 = 0,
                        // Token: 0x04000136 RID: 310
                        pic0 = 1,
                        // Token: 0x04000137 RID: 311
                        pic1 = 2,
                        // Token: 0x04000138 RID: 312
                        picmerged = 3
                    }

                    // Token: 0x02000027 RID: 39
                    public enum FormType
                    {
                        // Token: 0x0400013A RID: 314
                        PBPCreator,
                        // Token: 0x0400013B RID: 315
                        DATCreator,
                        // Token: 0x0400013C RID: 316
                        SFOCreator,
                        // Token: 0x0400013D RID: 317
                        SFOEditor
                    }
                }

                // Token: 0x040000A1 RID: 161
                public string pbpFile;

                // Token: 0x040000A2 RID: 162
                public long pbptotalSize;

                // Token: 0x040000A3 RID: 163
                public object datas;

                // Token: 0x040000A4 RID: 164
                public string[] pbpDataFiles;

                // Token: 0x040000A5 RID: 165
                public long[] pbpDataSizes;

                // Token: 0x040000A6 RID: 166
                public string tmp_image_icon0;

                // Token: 0x040000A7 RID: 167
                public string tmp_image_pic0;

                // Token: 0x040000A8 RID: 168
                public string tmp_image_pic1;

                // Token: 0x040000A9 RID: 169
                public bool ReturntoMain;

                public PSN(string SfoLocation, string Icon0location,string ICON1_PMFLocation,string PIC0Location,string PIC1Location,string SND0_AT3, string DATA_PSP , string DATA_PSAR)
                {
                    this.datas = Enum.GetValues(typeof(pbpData));
                    this.pbpDataFiles = new string[Enum.GetNames(typeof(pbpData)).Length - 1 + 1];
                    this.pbpDataSizes = new long[Enum.GetNames(typeof(pbpData)).Length - 1 + 1];

                    #region << SFO >>

                    this.pbpDataFiles[0] = SfoLocation;
                    this.pbpDataSizes[0] = this.GetSize(SfoLocation);

                    #endregion << SFO >>

                    #region << Icon0 >>

                    this.pbpDataFiles[1] = Icon0location;
                    string fileName = Icon0location;
                    System.Drawing.Size imgsize = new System.Drawing.Size(144, 80);
                    bool flag = !Utils.CheckImageSize(fileName, imgsize);
                    string fileName2 = Icon0location;
                    System.Drawing.Size imgsize2 = new System.Drawing.Size(80, 80);
                    if (flag | !Utils.CheckImageSize(fileName2, imgsize2))
                    {
                        string[] array = this.pbpDataFiles;
                        int num = 1;
                        string fileName3 = Icon0location;
                        string outputimgfile = this.tmp_image_pic0;
                        System.Drawing.Size imgsizeicon0 = new System.Drawing.Size(144, 80);
                        array[num] = Utils.ResizeImagesFilename(fileName3, outputimgfile, imgsizeicon0);
                    }
                    this.pbpDataSizes[1] = this.GetSize(Icon0location);


                    #endregion << Icon0 >>

                    #region << Icon1 PMF >>

                    this.pbpDataFiles[2] = ICON1_PMFLocation;
                    this.pbpDataSizes[2] = this.GetSize(ICON1_PMFLocation);

                    #endregion << Icon 1 PMF >>

                    #region << Pic0 >>

                    string[] array2 = this.pbpDataFiles;
                    int num2 = 3;
                    string fileName4 = PIC0Location;
                    string outputimgfile2 = this.tmp_image_pic0;
                    System.Drawing.Size imgsize3 = new System.Drawing.Size(480, 272);
                    array2[num2] = Utils.ResizeImagesFilename(fileName4, outputimgfile2, imgsize3);
                    this.pbpDataSizes[3] = this.GetSize(PIC0Location);

                    #endregion << Pic0 >>

                    #region << PIC1 >>

                    string[] array3 = this.pbpDataFiles;
                    int num3 = 4;
                    string fileName5 = PIC1Location;
                    string outputimgfile3 = this.tmp_image_pic1;
                    System.Drawing.Size imgsize4 = new System.Drawing.Size(480, 272);
                    array3[num3] = Utils.ResizeImagesFilename(fileName5, outputimgfile3, imgsize4);
                    this.pbpDataSizes[4] = this.GetSize(PIC1Location);

                    #endregion << PIC1>>

                    #region << SND0.AT3 >>

                    this.pbpDataFiles[5] = SND0_AT3;
                    this.pbpDataSizes[5] = this.GetSize(SND0_AT3);

                    #endregion << SND0.AT3 >>

                    #region << DATA_PSP >>

                    this.pbpDataFiles[6] = DATA_PSP;
                    this.pbpDataSizes[6] = this.GetSize(DATA_PSP);

                    #endregion << DATA_PSP >>

                    #region << DATA.PSAR >>

                    this.pbpDataFiles[7] = DATA_PSAR;
                    this.pbpDataSizes[7] = this.GetSize(DATA_PSAR);

                    #endregion << DATA.PSAR >>
                }
                public long GetSize(string FilePath)
                {
                    if (File.Exists(FilePath))
                    {
                        return new FileInfo(FilePath).Length;
                    }
                    return 0L;
                }

                public void CreatePBPFile(string OutputPath)
                {
                    try
                    {
                        foreach (object obj in ((IEnumerable)this.datas))
                        {
                            object objectValue = RuntimeHelpers.GetObjectValue(obj);
                            this.pbptotalSize += this.pbpDataSizes[Convert.ToInt32(objectValue)];
                        }
                    }
                    finally
                    {

                    }

                    //Delete file if already exisits in output path
                    if (File.Exists(OutputPath))
                    {
                        File.Delete(OutputPath);
                    }

                    //Create_DATA_PSARC(pbpDataSizes[0],pbpDataSizes[1],)

                    byte[] array = new byte[]
                    {
                        0,
                        80,
                        66,
                        80,
                        1,
                        0,
                        1,
                        0,
                        40,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0
                    };

                    using (FileStream fileStream = new FileStream(OutputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                    {
                        fileStream.Write(array, 0, array.Length);
                        int num = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[0]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[0], fileStream, 1024L);
                        }
                        int num2 = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[1]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[1], fileStream, 1024L);
                        }
                        int num3 = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[2]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[2], fileStream, 1024L);
                        }
                        int num4 = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[3]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[3], fileStream, 1024L);
                        }
                        int num5 = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[4]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[4], fileStream, 1024L);
                        }
                        int num6 = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[5]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[5], fileStream, 1024L);
                        }
                        int num7 = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[6]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[6], fileStream, 1024L);
                        }
                        int num8 = (int)fileStream.Position;
                        if (File.Exists(this.pbpDataFiles[7]))
                        {
                            this.ReadMergeFile(this.pbpDataFiles[7], fileStream, 4194304L);
                        }
                        fileStream.Position = 12L;
                        int value = num2;
                        byte[] bytes = BitConverter.GetBytes(value);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Position = 16L;
                        value = num3;
                        bytes = BitConverter.GetBytes(value);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Position = 20L;
                        value = num4;
                        bytes = BitConverter.GetBytes(value);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Position = 24L;
                        value = num5;
                        bytes = BitConverter.GetBytes(value);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Position = 28L;
                        value = num6;
                        bytes = BitConverter.GetBytes(value);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Position = 32L;
                        value = num7;
                        bytes = BitConverter.GetBytes(value);
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Position = 36L;
                        value = num8;
                        bytes = BitConverter.GetBytes(value);
                        fileStream.Write(bytes, 0, bytes.Length);
                    }
                    this.ReportEvent("Creating PBP is Complete.", "");
                }

                private void ReportEvent(string str, string percent)
                {
                    Console.WriteLine(str + " " + percent);
                }    

                private void ReadMergeFile(string filePath, FileStream fw, long buffersize)
                {
                    checked
                    {
                        try
                        {
                            using (BinaryReader binaryReader = new BinaryReader(new StreamReader(filePath).BaseStream))
                            {
                                byte[] array = new byte[(int)buffersize + 1];
                                int m_iBytes;
                                do
                                {
                                    this.ReportEvent("Creating", "");
                                    Array.Clear(array, 0, array.Length);
                                    m_iBytes = binaryReader.Read(array, 0, array.Length);
                                    fw.Write(array, 0, m_iBytes);   
                                }
                                while (m_iBytes > 0);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.ReportEvent("Error while Creating PBP Package!", "");
                            throw new Exception("Error While Create PBP Package! \n" + ex.Message);
                        }
                    }
                }

                [Flags]
                public enum pbpData
                {
                    // Token: 0x040000AB RID: 171
                    sfobytes = 0,
                    // Token: 0x040000AC RID: 172
                    icon0bytes = 1,
                    // Token: 0x040000AD RID: 173
                    icon1bytes = 2,
                    // Token: 0x040000AE RID: 174
                    pic0bytes = 3,
                    // Token: 0x040000AF RID: 175
                    pic1bytes = 4,
                    // Token: 0x040000B0 RID: 176
                    at3bytes = 5,
                    // Token: 0x040000B1 RID: 177
                    pspbytes = 6,
                    // Token: 0x040000B2 RID: 178
                    psardata = 7
                }

            }


            #endregion << Uncomment for Kirk Engine and PSN Fake Signing WIP >>
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


            public static void DecompressCSO(string CSOPath,string ISOFile)
            {
                byte[] cisotool = Properties.Resources.ciso;
                byte[] zlib = Properties.Resources.zlib;
                File.WriteAllBytes(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("PSP_Tools.dll","") + "ciso.exe", cisotool);
                File.WriteAllBytes(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("PSP_Tools.dll", "") + "zlib.dll", zlib);
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("PSP_Tools.dll", "") + "ciso.exe";
                start.Arguments = " 0 \"" + CSOPath + "\" \"" + ISOFile + "\"";
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;
                using (Process process = Process.Start(start))
                {
                    //process.ErrorDataReceived += Process_ErrorDataReceived;
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        if (result.Contains("[Error]"))
                        {
                            //System.Windows.Forms.MessageBox.Show(result);
                            Console.WriteLine(result);
                        }

                        Thread.Sleep(100);

                        File.Delete(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("PSP_Tools.dll", "") + "ciso.exe");
                        File.Delete(System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("PSP_Tools.dll", "") + "zlib.dll");
                        return;
                    }
                }



            }
        }
    }

    public class PSV
    {

        public static class Arrays
        {
            public static T[] InitializeWithDefaultInstances<T>(int length) where T : new()
            {
                T[] array = new T[length];
                for (int i = 0; i < length; i++)
                {
                    array[i] = new T();
                }
                return array;
            }

            public static void DeleteArray<T>(T[] array) where T : System.IDisposable
            {
                foreach (T element in array)
                {
                    if (element != null)
                        element.Dispose();
                }
            }
        }
        /// <summary>
        /// Orginal Code By Yifanlu ported to C# for Silca
        /// https://github.com/yifanlu/psvimgtools
        /// </summary>
        public class ImageTools
        {
            /** Access modes for st_mode in SceIoStat (confirm?). */
            /// <summary>
            ///  Access modes for st_mode in SceIoStat (confirm?). 
            /// Enums need to be named in c#
            /// </summary>
            public enum AnonymousEnum
            {
                /** Format bits mask */
                SCE_S_IFMT = 0xF000,
                /** Symbolic link */
                SCE_S_IFLNK = 0x4000,
                /** Directory */
                SCE_S_IFDIR = 0x1000,
                /** Regular file */
                SCE_S_IFREG = 0x2000,

                /** Set UID */
                SCE_S_ISUID = 0x0800,
                /** Set GID */
                SCE_S_ISGID = 0x0400,
                /** Sticky */
                SCE_S_ISVTX = 0x0200,

                /** Others access rights mask */
                SCE_S_IRWXO = 0x01C0,
                /** Others read permission */
                SCE_S_IROTH = 0x0100,
                /** Others write permission */
                SCE_S_IWOTH = 0x0080,
                /** Others execute permission */
                SCE_S_IXOTH = 0x0040,

                /** Group access rights mask */
                SCE_S_IRWXG = 0x0038,
                /** Group read permission */
                SCE_S_IRGRP = 0x0020,
                /** Group write permission */
                SCE_S_IWGRP = 0x0010,
                /** Group execute permission */
                SCE_S_IXGRP = 0x0008,

                /** User access rights mask */
                SCE_S_IRWXU = 0x0007,
                /** User read permission */
                SCE_S_IRUSR = 0x0004,
                /** User write permission */
                SCE_S_IWUSR = 0x0002,
                /** User execute permission */
                SCE_S_IXUSR = 0x0001,
            }

            /// <summary>
            /// /** File modes, used for the st_attr parameter in SceIoStat (confirm?). */
            /// </summary>
            public enum AnonymousEnum2
            {
                /** Format mask */
                SCE_SO_IFMT = 0x0038, // Format mask
                                      /** Symlink */
                SCE_SO_IFLNK = 0x0008, // Symbolic link
                                       /** Directory */
                SCE_SO_IFDIR = 0x0010, // Directory
                                       /** Regular file */
                SCE_SO_IFREG = 0x0020, // Regular file

                /** Hidden read permission */
                SCE_SO_IROTH = 0x0004, // read
                                       /** Hidden write permission */
                SCE_SO_IWOTH = 0x0002, // write
                                       /** Hidden execute permission */
                SCE_SO_IXOTH = 0x0001, // execute
            }

            public class SceDateTime
            {
                public ushort year = new ushort();
                public ushort month = new ushort();
                public ushort day = new ushort();
                public ushort hour = new ushort();
                public ushort minute = new ushort();
                public ushort second = new ushort();
                public uint microsecond = new uint();
            }

            /** Structure to hold the status information about a file */
            public class SceIoStat
            {
                public uint sst_mode = new uint();
                public uint sst_attr;
                /** Size of the file in bytes. */
                public UInt64 sst_size = new UInt64();
                /** Creation time. */
                public SceDateTime sst_ctime = new SceDateTime();
                /** Access time. */
                public SceDateTime sst_atime = new SceDateTime();
                /** Modification time. */
                public SceDateTime sst_mtime = new SceDateTime();
                /** Device-specific data. */
                public uint[] sst_private = Arrays.InitializeWithDefaultInstances<uint>(6);
            }

            public class PsvMd
            {
                public uint magic = new uint();
                public uint type = new uint();
                public UInt64 fw_version = new UInt64();
                public byte[] psid = Arrays.InitializeWithDefaultInstances<byte>(16);
                public string name = new string(new char[64]);
                public UInt64 psvimg_size = new UInt64();
                public UInt64 version = new UInt64(); // only support 2
                public UInt64 total_size = new UInt64();
                public byte[] iv = Arrays.InitializeWithDefaultInstances<byte>(16);
                public UInt64 ux0_info = new UInt64();
                public UInt64 ur0_info = new UInt64();
                public UInt64 unused_98 = new UInt64();
                public UInt64 unused_A0 = new UInt64();
                public uint add_data = new uint();
            }

            /** This file (and backup) can only be restored with the same PSID */
            public class PsvImgHeader
            {
                public UInt64 systime = new UInt64();
                public UInt64 flags = new UInt64();
                public SceIoStat stat = new SceIoStat();
                public string path_parent = new string(new char[256]);
                public uint unk_16C = new uint(); // set to 1
                public string path_rel = new string(new char[256]);
                public string unused = new string(new char[904]);
                public string end = new string(new char[12]);
            }
            /** The file/directory will be _removed_ (not restored). */
            public class PsvImgTailer
            {
                public UInt64 flags = new UInt64();
                public string unused = new string(new char[1004]);
                public string end = new string(new char[12]);
            }

            public class status_t
            {
                public uint found = new uint();
                public uint at = new uint();
            }

            /*
	         * PSP Software Development Kit - http://www.pspdev.org
	         * -----------------------------------------------------------------------
	         * Licensed under the BSD license, see LICENSE in PSPSDK root for details.
	         *
	         * font.c - Debug Font.
	         *
	         * Copyright (c) 2005 Marcus R. Brown <mrbrown@ocgnet.org>
	         * Copyright (c) 2005 James Forshaw <tyranid@gmail.com>
	         * Copyright (c) 2005 John Kelley <ps2dev@kelley.ca>
	         *
	         * $Id: font.c 540 2005-07-08 19:35:10Z warren $
	         */

            //public static byte[] psvDebugScreenFont = "\x00\x00\x00\x00\x00\x00\x00\x00\x3c\x42\xa5\x81\xa5\x99\x42\x3c" + "\x3c\x7e\xdb\xff\xff\xdb\x66\x3c\x6c\xfe\xfe\xfe\x7c\x38\x10\x00" + "\x10\x38\x7c\xfe\x7c\x38\x10\x00\x10\x38\x54\xfe\x54\x10\x38\x00" + "\x10\x38\x7c\xfe\xfe\x10\x38\x00\x00\x00\x00\x30\x30\x00\x00\x00" + "\xff\xff\xff\xe7\xe7\xff\xff\xff\x38\x44\x82\x82\x82\x44\x38\x00" + "\xc7\xbb\x7d\x7d\x7d\xbb\xc7\xff\x0f\x03\x05\x79\x88\x88\x88\x70" + "\x38\x44\x44\x44\x38\x10\x7c\x10\x30\x28\x24\x24\x28\x20\xe0\xc0" + "\x3c\x24\x3c\x24\x24\xe4\xdc\x18\x10\x54\x38\xee\x38\x54\x10\x00" + "\x10\x10\x10\x7c\x10\x10\x10\x10\x10\x10\x10\xff\x00\x00\x00\x00" + "\x00\x00\x00\xff\x10\x10\x10\x10\x10\x10\x10\xf0\x10\x10\x10\x10" + "\x10\x10\x10\x1f\x10\x10\x10\x10\x10\x10\x10\xff\x10\x10\x10\x10" + "\x10\x10\x10\x10\x10\x10\x10\x10\x00\x00\x00\xff\x00\x00\x00\x00" + "\x00\x00\x00\x1f\x10\x10\x10\x10\x00\x00\x00\xf0\x10\x10\x10\x10" + "\x10\x10\x10\x1f\x00\x00\x00\x00\x10\x10\x10\xf0\x00\x00\x00\x00" + "\x81\x42\x24\x18\x18\x24\x42\x81\x01\x02\x04\x08\x10\x20\x40\x80" + "\x80\x40\x20\x10\x08\x04\x02\x01\x00\x10\x10\xff\x10\x10\x00\x00" + "\x00\x00\x00\x00\x00\x00\x00\x00\x20\x20\x20\x20\x00\x00\x20\x00" + "\x50\x50\x50\x00\x00\x00\x00\x00\x50\x50\xf8\x50\xf8\x50\x50\x00" + "\x20\x78\xa0\x70\x28\xf0\x20\x00\xc0\xc8\x10\x20\x40\x98\x18\x00" + "\x40\xa0\x40\xa8\x90\x98\x60\x00\x10\x20\x40\x00\x00\x00\x00\x00" + "\x10\x20\x40\x40\x40\x20\x10\x00\x40\x20\x10\x10\x10\x20\x40\x00" + "\x20\xa8\x70\x20\x70\xa8\x20\x00\x00\x20\x20\xf8\x20\x20\x00\x00" + "\x00\x00\x00\x00\x00\x20\x20\x40\x00\x00\x00\x78\x00\x00\x00\x00" + "\x00\x00\x00\x00\x00\x60\x60\x00\x00\x00\x08\x10\x20\x40\x80\x00" + "\x70\x88\x98\xa8\xc8\x88\x70\x00\x20\x60\xa0\x20\x20\x20\xf8\x00" + "\x70\x88\x08\x10\x60\x80\xf8\x00\x70\x88\x08\x30\x08\x88\x70\x00" + "\x10\x30\x50\x90\xf8\x10\x10\x00\xf8\x80\xe0\x10\x08\x10\xe0\x00" + "\x30\x40\x80\xf0\x88\x88\x70\x00\xf8\x88\x10\x20\x20\x20\x20\x00" + "\x70\x88\x88\x70\x88\x88\x70\x00\x70\x88\x88\x78\x08\x10\x60\x00" + "\x00\x00\x20\x00\x00\x20\x00\x00\x00\x00\x20\x00\x00\x20\x20\x40" + "\x18\x30\x60\xc0\x60\x30\x18\x00\x00\x00\xf8\x00\xf8\x00\x00\x00" + "\xc0\x60\x30\x18\x30\x60\xc0\x00\x70\x88\x08\x10\x20\x00\x20\x00" + "\x70\x88\x08\x68\xa8\xa8\x70\x00\x20\x50\x88\x88\xf8\x88\x88\x00" + "\xf0\x48\x48\x70\x48\x48\xf0\x00\x30\x48\x80\x80\x80\x48\x30\x00" + "\xe0\x50\x48\x48\x48\x50\xe0\x00\xf8\x80\x80\xf0\x80\x80\xf8\x00" + "\xf8\x80\x80\xf0\x80\x80\x80\x00\x70\x88\x80\xb8\x88\x88\x70\x00" + "\x88\x88\x88\xf8\x88\x88\x88\x00\x70\x20\x20\x20\x20\x20\x70\x00" + "\x38\x10\x10\x10\x90\x90\x60\x00\x88\x90\xa0\xc0\xa0\x90\x88\x00" + "\x80\x80\x80\x80\x80\x80\xf8\x00\x88\xd8\xa8\xa8\x88\x88\x88\x00" + "\x88\xc8\xc8\xa8\x98\x98\x88\x00\x70\x88\x88\x88\x88\x88\x70\x00" + "\xf0\x88\x88\xf0\x80\x80\x80\x00\x70\x88\x88\x88\xa8\x90\x68\x00" + "\xf0\x88\x88\xf0\xa0\x90\x88\x00\x70\x88\x80\x70\x08\x88\x70\x00" + "\xf8\x20\x20\x20\x20\x20\x20\x00\x88\x88\x88\x88\x88\x88\x70\x00" + "\x88\x88\x88\x88\x50\x50\x20\x00\x88\x88\x88\xa8\xa8\xd8\x88\x00" + "\x88\x88\x50\x20\x50\x88\x88\x00\x88\x88\x88\x70\x20\x20\x20\x00" + "\xf8\x08\x10\x20\x40\x80\xf8\x00\x70\x40\x40\x40\x40\x40\x70\x00" + "\x00\x00\x80\x40\x20\x10\x08\x00\x70\x10\x10\x10\x10\x10\x70\x00" + "\x20\x50\x88\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\xf8\x00" + "\x40\x20\x10\x00\x00\x00\x00\x00\x00\x00\x70\x08\x78\x88\x78\x00" + "\x80\x80\xb0\xc8\x88\xc8\xb0\x00\x00\x00\x70\x88\x80\x88\x70\x00" + "\x08\x08\x68\x98\x88\x98\x68\x00\x00\x00\x70\x88\xf8\x80\x70\x00" + "\x10\x28\x20\xf8\x20\x20\x20\x00\x00\x00\x68\x98\x98\x68\x08\x70" + "\x80\x80\xf0\x88\x88\x88\x88\x00\x20\x00\x60\x20\x20\x20\x70\x00" + "\x10\x00\x30\x10\x10\x10\x90\x60\x40\x40\x48\x50\x60\x50\x48\x00" + "\x60\x20\x20\x20\x20\x20\x70\x00\x00\x00\xd0\xa8\xa8\xa8\xa8\x00" + "\x00\x00\xb0\xc8\x88\x88\x88\x00\x00\x00\x70\x88\x88\x88\x70\x00" + "\x00\x00\xb0\xc8\xc8\xb0\x80\x80\x00\x00\x68\x98\x98\x68\x08\x08" + "\x00\x00\xb0\xc8\x80\x80\x80\x00\x00\x00\x78\x80\xf0\x08\xf0\x00" + "\x40\x40\xf0\x40\x40\x48\x30\x00\x00\x00\x90\x90\x90\x90\x68\x00" + "\x00\x00\x88\x88\x88\x50\x20\x00\x00\x00\x88\xa8\xa8\xa8\x50\x00" + "\x00\x00\x88\x50\x20\x50\x88\x00\x00\x00\x88\x88\x98\x68\x08\x70" + "\x00\x00\xf8\x10\x20\x40\xf8\x00\x18\x20\x20\x40\x20\x20\x18\x00" + "\x20\x20\x20\x00\x20\x20\x20\x00\xc0\x20\x20\x10\x20\x20\xc0\x00" + "\x40\xa8\x10\x00\x00\x00\x00\x00\x00\x00\x20\x50\xf8\x00\x00\x00" + "\x70\x88\x80\x80\x88\x70\x20\x60\x90\x00\x00\x90\x90\x90\x68\x00" + "\x10\x20\x70\x88\xf8\x80\x70\x00\x20\x50\x70\x08\x78\x88\x78\x00" + "\x48\x00\x70\x08\x78\x88\x78\x00\x20\x10\x70\x08\x78\x88\x78\x00" + "\x20\x00\x70\x08\x78\x88\x78\x00\x00\x70\x80\x80\x80\x70\x10\x60" + "\x20\x50\x70\x88\xf8\x80\x70\x00\x50\x00\x70\x88\xf8\x80\x70\x00" + "\x20\x10\x70\x88\xf8\x80\x70\x00\x50\x00\x00\x60\x20\x20\x70\x00" + "\x20\x50\x00\x60\x20\x20\x70\x00\x40\x20\x00\x60\x20\x20\x70\x00" + "\x50\x00\x20\x50\x88\xf8\x88\x00\x20\x00\x20\x50\x88\xf8\x88\x00" + "\x10\x20\xf8\x80\xf0\x80\xf8\x00\x00\x00\x6c\x12\x7e\x90\x6e\x00" + "\x3e\x50\x90\x9c\xf0\x90\x9e\x00\x60\x90\x00\x60\x90\x90\x60\x00" + "\x90\x00\x00\x60\x90\x90\x60\x00\x40\x20\x00\x60\x90\x90\x60\x00" + "\x40\xa0\x00\xa0\xa0\xa0\x50\x00\x40\x20\x00\xa0\xa0\xa0\x50\x00" + "\x90\x00\x90\x90\xb0\x50\x10\xe0\x50\x00\x70\x88\x88\x88\x70\x00" + "\x50\x00\x88\x88\x88\x88\x70\x00\x20\x20\x78\x80\x80\x78\x20\x20" + "\x18\x24\x20\xf8\x20\xe2\x5c\x00\x88\x50\x20\xf8\x20\xf8\x20\x00" + "\xc0\xa0\xa0\xc8\x9c\x88\x88\x8c\x18\x20\x20\xf8\x20\x20\x20\x40" + "\x10\x20\x70\x08\x78\x88\x78\x00\x10\x20\x00\x60\x20\x20\x70\x00" + "\x20\x40\x00\x60\x90\x90\x60\x00\x20\x40\x00\x90\x90\x90\x68\x00" + "\x50\xa0\x00\xa0\xd0\x90\x90\x00\x28\x50\x00\xc8\xa8\x98\x88\x00" + "\x00\x70\x08\x78\x88\x78\x00\xf8\x00\x60\x90\x90\x90\x60\x00\xf0" + "\x20\x00\x20\x40\x80\x88\x70\x00\x00\x00\x00\xf8\x80\x80\x00\x00" + "\x00\x00\x00\xf8\x08\x08\x00\x00\x84\x88\x90\xa8\x54\x84\x08\x1c" + "\x84\x88\x90\xa8\x58\xa8\x3c\x08\x20\x00\x00\x20\x20\x20\x20\x00" + "\x00\x00\x24\x48\x90\x48\x24\x00\x00\x00\x90\x48\x24\x48\x90\x00" + "\x28\x50\x20\x50\x88\xf8\x88\x00\x28\x50\x70\x08\x78\x88\x78\x00" + "\x28\x50\x00\x70\x20\x20\x70\x00\x28\x50\x00\x20\x20\x20\x70\x00" + "\x28\x50\x00\x70\x88\x88\x70\x00\x50\xa0\x00\x60\x90\x90\x60\x00" + "\x28\x50\x00\x88\x88\x88\x70\x00\x50\xa0\x00\xa0\xa0\xa0\x50\x00" + "\xfc\x48\x48\x48\xe8\x08\x50\x20\x00\x50\x00\x50\x50\x50\x10\x20" + "\xc0\x44\xc8\x54\xec\x54\x9e\x04\x10\xa8\x40\x00\x00\x00\x00\x00" + "\x00\x20\x50\x88\x50\x20\x00\x00\x88\x10\x20\x40\x80\x28\x00\x00" + "\x7c\xa8\xa8\x68\x28\x28\x28\x00\x38\x40\x30\x48\x48\x30\x08\x70" + "\x00\x00\x00\x00\x00\x00\xff\xff\xf0\xf0\xf0\xf0\x0f\x0f\x0f\x0f" + "\x00\x00\xff\xff\xff\xff\xff\xff\xff\xff\x00\x00\x00\x00\x00\x00" + "\x00\x00\x00\x3c\x3c\x00\x00\x00\xff\xff\xff\xff\xff\xff\x00\x00" + "\xc0\xc0\xc0\xc0\xc0\xc0\xc0\xc0\x0f\x0f\x0f\x0f\xf0\xf0\xf0\xf0" + "\xfc\xfc\xfc\xfc\xfc\xfc\xfc\xfc\x03\x03\x03\x03\x03\x03\x03\x03" + "\x3f\x3f\x3f\x3f\x3f\x3f\x3f\x3f\x11\x22\x44\x88\x11\x22\x44\x88" + "\x88\x44\x22\x11\x88\x44\x22\x11\xfe\x7c\x38\x10\x00\x00\x00\x00" + "\x00\x00\x00\x00\x10\x38\x7c\xfe\x80\xc0\xe0\xf0\xe0\xc0\x80\x00" + "\x01\x03\x07\x0f\x07\x03\x01\x00\xff\x7e\x3c\x18\x18\x3c\x7e\xff" + "\x81\xc3\xe7\xff\xff\xe7\xc3\x81\xf0\xf0\xf0\xf0\x00\x00\x00\x00" + "\x00\x00\x00\x00\x0f\x0f\x0f\x0f\x0f\x0f\x0f\x0f\x00\x00\x00\x00" + "\x00\x00\x00\x00\xf0\xf0\xf0\xf0\x33\x33\xcc\xcc\x33\x33\xcc\xcc" + "\x00\x20\x20\x50\x50\x88\xf8\x00\x20\x20\x70\x20\x70\x20\x20\x00" + "\x00\x00\x00\x50\x88\xa8\x50\x00\xff\xff\xff\xff\xff\xff\xff\xff" + "\x00\x00\x00\x00\xff\xff\xff\xff\xf0\xf0\xf0\xf0\xf0\xf0\xf0\xf0" + "\x0f\x0f\x0f\x0f\x0f\x0f\x0f\x0f\xff\xff\xff\xff\x00\x00\x00\x00" + "\x00\x00\x68\x90\x90\x90\x68\x00\x30\x48\x48\x70\x48\x48\x70\xc0" + "\xf8\x88\x80\x80\x80\x80\x80\x00\xf8\x50\x50\x50\x50\x50\x98\x00" + "\xf8\x88\x40\x20\x40\x88\xf8\x00\x00\x00\x78\x90\x90\x90\x60\x00" + "\x00\x50\x50\x50\x50\x68\x80\x80\x00\x50\xa0\x20\x20\x20\x20\x00" + "\xf8\x20\x70\xa8\xa8\x70\x20\xf8\x20\x50\x88\xf8\x88\x50\x20\x00" + "\x70\x88\x88\x88\x50\x50\xd8\x00\x30\x40\x40\x20\x50\x50\x50\x20" + "\x00\x00\x00\x50\xa8\xa8\x50\x00\x08\x70\xa8\xa8\xa8\x70\x80\x00" + "\x38\x40\x80\xf8\x80\x40\x38\x00\x70\x88\x88\x88\x88\x88\x88\x00" + "\x00\xf8\x00\xf8\x00\xf8\x00\x00\x20\x20\xf8\x20\x20\x00\xf8\x00" + "\xc0\x30\x08\x30\xc0\x00\xf8\x00\x18\x60\x80\x60\x18\x00\xf8\x00" + "\x10\x28\x20\x20\x20\x20\x20\x20\x20\x20\x20\x20\x20\x20\xa0\x40" + "\x00\x20\x00\xf8\x00\x20\x00\x00\x00\x50\xa0\x00\x50\xa0\x00\x00" + "\x00\x18\x24\x24\x18\x00\x00\x00\x00\x30\x78\x78\x30\x00\x00\x00" + "\x00\x00\x00\x00\x30\x00\x00\x00\x3e\x20\x20\x20\xa0\x60\x20\x00" + "\xa0\x50\x50\x50\x00\x00\x00\x00\x40\xa0\x20\x40\xe0\x00\x00\x00" + "\x00\x38\x38\x38\x38\x38\x38\x00\x00\x00\x00\x00\x00\x00\x00";

            /* Copyright (C) 2017 Yifan Lu
	         *
	         * This software may be modified and distributed under the terms
	         * of the MIT license.  See the LICENSE file for details.
	         */





            //public static class 

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
