/* Copyright (c) 2015 - 2018 TheDarkporgramer
*
* This was originally done by Leecherman https://sites.google.com/site/theleecherman (I have no idea who you are but you do some great work !)
* All modifications have been TheDarkporgramer (sfo ext ext ) https://github.com/xXxTheDarkprogramerxXx
* 
* This(software Is provided) 'as-is', without any express or implied
* warranty. In no event will the authors be held liable for any damages arising from the use of this software.
*
* Permission is granted to anyone to use this software for any purpose,
* including commercial applications*, and to alter it and redistribute it
* freely, subject to the following restrictions:
*
* 1. The origin of this software must not be misrepresented; you must not
*   claim that you wrote the original software. If you use this software
*   in a product, an acknowledge in the product documentation is required.
*
* 2. Altered source versions must be plainly marked as such, and must not
*    be misrepresented as being the original software.
*
* 3. This notice may not be removed or altered from any source distribution.
*
* *Contact must be made to discuses permission and terms.
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PSP_Tools
{
    public class Pbp : IDisposable
    {
        public enum DataType
        {
            ParamSfo,
            Icon0Png,
            Icon1Pmf,
            Pic0Png,
            Pic1Png,
            Snd0At3,
            PspData,
            PsarData
        }

        private readonly uint PbpMagic;

        private int PbpVersion;

        private long PbpSize;

        private Dictionary<string, int> PbpFiles;

        private Dictionary<string, long> PbpFilesSize;

        private readonly string[] Names;

        public Stream PbpStream;

        public int Version
        {
            get
            {
                return this.PbpVersion;
            }
        }

        public long Size
        {
            get
            {
                return this.PbpSize;
            }
        }

        public void Dispose()
        {
            ((IDisposable)this.PbpStream).Dispose();
        }

        public Pbp()
        {
            this.PbpMagic = 1346523136u;
            this.PbpFiles = new Dictionary<string, int>();
            this.PbpFilesSize = new Dictionary<string, long>();
            //names of items inside PBP
            this.Names = new string[]
            {
                "param.sfo",
                "icon0.png",
                "icon1.pmf",
                "pic0.png",
                "pic1.png",
                "snd0.at3",
                "psp.data",
                "psar.data"
            };
        }

        public void LoadPbp(Stream PbpStrm, long Pos)
        {
            this.PbpFiles.Clear();

            PbpStrm.Seek(Pos, SeekOrigin.Begin);
            this.PbpStream = PbpStrm;
            byte[] array = new byte[4];
            this.PbpStream.Read(array, 0, array.Length);
            if (BitConverter.ToUInt32(array, 0) != this.PbpMagic)
            {
                throw new Exception("Not a PBP file!");
            }
            this.PbpStream.Read(array, 0, array.Length);
            this.PbpVersion = this.byteArrayToLittleEndianInteger(array);
            checked
            {
                this.PbpSize = PbpStrm.Length - Pos;
                int num = 8;
                int num2 = 0;
                do
                {
                    this.PbpStream.Seek(unchecked((long)num), SeekOrigin.Begin);
                    this.PbpStream.Read(array, 0, array.Length);
                    this.PbpFiles.Add(this.Names[num2], (int)BitConverter.ToUInt32(array, 0));
                    num += 4;
                    num2++;
                }
                while (num2 <= 7);
                int num3 = 0;
                do
                {
                    if (num3 == 7)
                    {
                        this.PbpFilesSize.Add(this.Names[num3], this.PbpSize - unchecked((long)this.PbpFiles[this.Names[num3]]));
                    }
                    else
                    {
                        this.PbpFilesSize.Add(this.Names[num3], unchecked((long)(checked(this.PbpFiles[this.Names[num3 + 1]] - this.PbpFiles[this.Names[num3]]))));
                    }
                    num3++;
                }
                while (num3 <= 7);
                this.PbpStream.Seek(0L, SeekOrigin.Begin);
            }
        }

        public void LoadPbp(string Pbp)
        {
            this.PbpStream = new FileStream(Pbp, FileMode.Open, FileAccess.Read, FileShare.Read);
            this.LoadPbp(this.PbpStream, 0L);
        }

        public void SeekPbp(long seeOffset, SeekOrigin seekPosition)
        {
            this.PbpStream.Seek(seeOffset, seekPosition);
        }

        public void ReadBytes(byte[] buffer)
        {
            this.PbpStream.Read(buffer, 0, buffer.Length);
        }

        public void ClosePbp()
        {
            this.PbpStream.Close();
            this.PbpStream.Dispose();
        }

        public bool ContainsKey(Pbp.DataType Type)
        {
            return this.PbpFiles.ContainsKey(this.Names[(int)Type]);
        }

        public bool ContainsKey(string Key)
        {
            return this.PbpFiles.ContainsKey(Key);
        }

        public int FileOffset(Pbp.DataType Type)
        {
            return this.PbpFiles[this.Names[(int)Type]];
        }

        public int FileOffset(string Key)
        {
            return this.PbpFiles[Key];
        }

        public long FileSize(Pbp.DataType Type)
        {
            return this.PbpFilesSize[this.Names[(int)Type]];
        }

        public long FileSize(string Key)
        {
            return this.PbpFilesSize[Key];
        }

        private int byteArrayToLittleEndianInteger(byte[] bits)
        {
            return (int)(bits[0] | (byte)(bits[1] << 0) | (byte)(bits[2] << 0) | (byte)(bits[3] << 0));
        }

        public void WritePBPFiles(string SaveDir)
        {
            byte[] array = this.ReadFileFromPBP(DataType.ParamSfo);
            if (array != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.ParamSfo], array);
            }
            byte[] arraypspdaya = this.ReadFileFromPBP(DataType.PspData);
            if (arraypspdaya != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.PspData], arraypspdaya);
            }

            /*	ParamSfo,
			Icon0Png,
			Icon1Pmf,
			Pic0Png,
			Pic1Png,
			Snd0At3,
			PspData,
			PsarData*/
            byte[] Icon0Png = this.ReadFileFromPBP(DataType.Icon0Png);
            if (Icon0Png != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.Icon0Png], Icon0Png);
            }

            byte[] Icon1Pmf = this.ReadFileFromPBP(DataType.Icon1Pmf);
            if (Icon1Pmf != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.Icon1Pmf], Icon0Png);
            }

            byte[] Pic0Png = this.ReadFileFromPBP(DataType.Pic0Png);
            if (Pic0Png != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.Pic0Png], Pic0Png);
            }

            byte[] Pic1Png = this.ReadFileFromPBP(DataType.Pic1Png);
            if (Pic1Png != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.Pic1Png], Pic1Png);
            }

            byte[] Snd0At3 = this.ReadFileFromPBP(DataType.Snd0At3);
            if (Snd0At3 != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.Snd0At3], Snd0At3);
            }

            byte[] PsarData = this.ReadFileFromPBP(DataType.PsarData);
            if (PsarData != null)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\" + this.Names[(int)DataType.PsarData], PsarData);
            }
        }


        public void WritePBPFiles(string SaveDir,string pspdata = "PSP.DATA", string psrdata = "psar.data",bool make_eboot_boot = false)
        {

            if (!Directory.Exists(SaveDir))
            {
                Directory.CreateDirectory(SaveDir);
            }

            if(!Directory.Exists(SaveDir +"\\PSP_GAME"))
            {
                Directory.CreateDirectory(SaveDir + "\\PSP_GAME");
            }

            if (!Directory.Exists(SaveDir + "\\PSP_GAME\\"))
            {
                Directory.CreateDirectory(SaveDir + "\\PSP_GAME");
            }

            if(!Directory.Exists(SaveDir + "\\PSP_GAME\\SYSDIR\\"))
            {
                Directory.CreateDirectory(SaveDir  + "\\PSP_GAME\\SYSDIR\\");
            }

            byte[] array = this.ReadFileFromPBP(DataType.ParamSfo);
            if (array != null &&array.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\" + this.Names[(int)DataType.ParamSfo].ToUpper(), array);
            }


            byte[] arraypspdaya = this.ReadFileFromPBP(DataType.PspData);
            if (arraypspdaya != null &&arraypspdaya.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\SYSDIR\\"+ pspdata.ToUpper(), arraypspdaya);
                if(make_eboot_boot == true)
                {
                    System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\SYSDIR\\BOOT.BIN", arraypspdaya);
                }
            }

            

            /*	ParamSfo,
			Icon0Png,
			Icon1Pmf,
			Pic0Png,
			Pic1Png,
			Snd0At3,
			PspData,
			PsarData*/
            byte[] Icon0Png = this.ReadFileFromPBP(DataType.Icon0Png);
            if (Icon0Png != null &&Icon0Png.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\" + this.Names[(int)DataType.Icon0Png].ToUpper(), Icon0Png);
            }

            byte[] Icon1Pmf = this.ReadFileFromPBP(DataType.Icon1Pmf);
            if (Icon1Pmf != null &&Icon1Pmf.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\" + this.Names[(int)DataType.Icon1Pmf].ToUpper(), Icon0Png);
            }

            byte[] Pic0Png = this.ReadFileFromPBP(DataType.Pic0Png);
            if (Pic0Png != null &&Pic0Png.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\" + this.Names[(int)DataType.Pic0Png].ToUpper(), Pic0Png);
            }

            byte[] Pic1Png = this.ReadFileFromPBP(DataType.Pic1Png);
            if (Pic1Png != null &&Pic1Png.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\" + this.Names[(int)DataType.Pic1Png].ToUpper(), Pic1Png);
            }

            byte[] Snd0At3 = this.ReadFileFromPBP(DataType.Snd0At3);
            if (Snd0At3 != null &&Snd0At3.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\" + this.Names[(int)DataType.Snd0At3].ToUpper(), Snd0At3);
            }

            byte[] PsarData = this.ReadFileFromPBP(DataType.PsarData);
            if (PsarData != null &&PsarData.Length != 0)
            {
                System.IO.File.WriteAllBytes(SaveDir + "\\PSP_GAME\\SYSDIR\\" + psrdata.ToUpper(), PsarData);
            }
        }

        public byte[] ReadFileFromPBP(Pbp.DataType dataType)
        {
            byte[] result;
            try
            {
                byte[] array = null;

                this.SeekPbp((long)this.FileOffset(dataType), SeekOrigin.Begin);
                array = new byte[checked((int)(this.FileSize(dataType) - 1L) + 1)];
                this.ReadBytes(array);

                result = array;
            }
            catch (Exception arg_48_0)
            {
                result = null;
            }

            return result;
        }

        public string GetPBPDiscID()
        {
            string @string;

            this.PbpStream.Seek((long)(checked(this.FileOffset(Pbp.DataType.PsarData) + 23)), SeekOrigin.Begin);
            byte[] array = new byte[9];
            this.ReadBytes(array);
            @string = Encoding.ASCII.GetString(array);//this will return 000000 and stuff but meh

            return @string;
        }

    }

    /// <summary>
    /// This is from my PARAM.SFO Editor
    /// This has been stripped down to only work for the psp file
    /// If Used Please Give Credit 
    /// </summary>
    public class PARAM_SFO
    {
        #region << Enums >>
        public enum DataTypes : uint
        {
            PSN_Game = 18248,
            GameData = 0x4744,
            SaveData = 0x5344,
            AppPhoto = 0x4150,
            AppMusic = 0x414D,
            AppVideo = 0x4156,
            BroadCastVideo = 0x4256,
            AppleTV = 4154,
            WebTV = 5754,
            CellBE = 0x4342,
            Home = 0x484D,
            StoreFronted = 0x5346,
            HDDGame = 0x4847,
            DiscGame = 0x4447,
            AutoInstallRoot = 0x4152,
            DiscPackage = 0x4450,
            ExtraRoot = 0x5852,
            VideoRoot = 0x5652,
            ThemeRoot = 0x5452,
            DiscMovie = 0x444D,
            Game_Digital_Application = 0x4081AC0,//GD
            PS4_Game_Application_Patch = 28775,
            Additional_Content = 25441,//PSvita PS4
            GameContent = 25447,//PSVITA
            Blu_Ray_Disc = 25698,//PS4
            None
        }

        public enum FMT : ushort
        {
            UTF_8 = 0x0004,
            ASCII = 0x0402,
            Utf8Null = 0x0204,
            UINT32 = 0x0404,
        }

        #endregion << Enums >>

        #region << Vars>>
        public List<Table> Tables { get; set; }

        #endregion << Vars>>

        #region << Example Of Calling Functions >>
        public DataTypes DataType
        {
            get
            {
                if (Tables == null)
                    return DataTypes.None;
                foreach (Table t in Tables)
                    if (t.Name == "CATEGORY")
                        return ((DataTypes)BitConverter.ToUInt16(Encoding.UTF8.GetBytes(t.Value), 0));
                return DataTypes.None;
            }
        }

        public string PSP_SYSTEM_VER
        {
            get
            {
                if (Tables == null)
                    return "";
                foreach (Table t in Tables)
                    if (t.Name == "PSP_SYSTEM_VER")
                        return t.Value;
                return "";
            }
        }

        public string DISC_ID
        {
            get
            {
                if (Tables == null)
                    return "";
                foreach (Table t in Tables)
                    if (t.Name == "DISC_ID")
                        return t.Value;
                return "";
            }
        }

        public string Title
        {
            get
            {
                if (Tables == null)
                    return "";
                foreach (Table t in Tables)
                    if (t.Name == "TITLE")
                        return t.Value;
                return "";
            }
        }

        public string CATEGORY
        {
            get
            {
                if (Tables == null)
                    return "";
                foreach (Table t in Tables)
                    if (t.Name == "CATEGORY")
                        return t.Value;
                return "";
            }
        }

        #endregion << Example Of Calling Functions >>

        #region Param.SFO Struct 

        public struct Header
        {
            public static byte[] Magic = { 0, 0x50, 0x53, 0x46 };
            public static byte[] version = { 01, 01, 0, 0 };
            public static uint KeyTableStart = 0;
            public static uint DataTableStart = 0;
            public static uint IndexTableEntries = 0;

            private static byte[] Buffer
            {
                get
                {
                    var header = new byte[20];
                    Array.Copy(Magic, 0, header, 0, 4);
                    Array.Copy(version, 0, header, 4, 4);
                    Array.Copy(BitConverter.GetBytes(KeyTableStart), 0, header, 8, 4);
                    Array.Copy(BitConverter.GetBytes(DataTableStart), 0, header, 12, 4);
                    Array.Copy(BitConverter.GetBytes(IndexTableEntries), 0, header, 16, 4);
                    return header;
                }
            }

            public static void Read(BinaryReader input)
            {
                input.BaseStream.Seek(0, SeekOrigin.Begin);
                input.Read(Magic, 0, 4);
                input.Read(version, 0, 4);
                KeyTableStart = input.ReadUInt32();
                DataTableStart = input.ReadUInt32();
                IndexTableEntries = input.ReadUInt32();
            }

        }
        [Serializable]
        public struct Table : IComparable
        {
            public index_table Indextable;
            public string Name;
            public string Value;
            public int index;

            public byte[] NameBuffer
            {
                get
                {
                    var buffer = new byte[Name.Length + 1];
                    Array.Copy(Encoding.UTF8.GetBytes(Name), 0, buffer, 0, Encoding.UTF8.GetBytes(Name).Length);
                    return buffer;
                }
            }

            public byte[] ValueBuffer
            {
                get
                {
                    byte[] buffer;
                    switch (Indextable.param_data_fmt)
                    {
                        case FMT.ASCII:
                            buffer = new byte[Indextable.param_data_max_len];
                            Array.Copy(Encoding.ASCII.GetBytes(Value), 0, buffer, 0, Encoding.UTF8.GetBytes(Value).Length);
                            return buffer;
                        case FMT.UINT32:
                            return BitConverter.GetBytes(uint.Parse(Value));
                        case FMT.UTF_8:
                            buffer = new byte[Indextable.param_data_max_len];
                            Array.Copy(Encoding.UTF8.GetBytes(Value), 0, buffer, 0, Encoding.UTF8.GetBytes(Value).Length);
                            return buffer;
                        case FMT.Utf8Null:
                            buffer = new byte[Indextable.param_data_max_len];
                            Array.Copy(Encoding.UTF8.GetBytes(Value), 0, buffer, 0, Encoding.UTF8.GetBytes(Value).Length);/*write the length of the array*/
                            return buffer;
                        default:
                            return null;
                    }
                }
            }

            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }
        }
        [Serializable]
        public struct index_table
        {
            public FMT param_data_fmt; /* param_data data type */
            public uint param_data_len; /* param_data used bytes */
            public uint param_data_max_len; /* param_data total reserved bytes */
            public uint param_data_offset; /* param_data offset (relative to start offset of data_table) */
            public ushort param_key_offset; /* param_key offset (relative to start offset of key_table) */


            private byte[] Buffer
            {
                get
                {
                    var data = new byte[16];
                    Array.Copy(BitConverter.GetBytes(param_key_offset), 0, data, 0, 2);
                    Array.Copy(BitConverter.GetBytes(((ushort)param_data_fmt)), 0, data, 2, 2);
                    Array.Copy(BitConverter.GetBytes(param_data_len), 0, data, 4, 4);
                    Array.Copy(BitConverter.GetBytes(param_data_max_len), 0, data, 8, 4);
                    Array.Copy(BitConverter.GetBytes(param_data_offset), 0, data, 12, 4);
                    return data;
                }
            }

            public void Read(BinaryReader input)
            {
                param_key_offset = input.ReadUInt16();
                param_data_fmt = (FMT)input.ReadUInt16();
                param_data_len = input.ReadUInt32();
                param_data_max_len = input.ReadUInt32();
                param_data_offset = input.ReadUInt32();
            }
        }

        [Serializable]
        private enum DATA_TYPE : byte
        {
            BinaryData = 0,
            Utf8Text = 2,
            Si32Integer = 4
        }

        #endregion Param.SFO Struct

        #region << Methods >>


        public PARAM_SFO()
        {

        }

        public PARAM_SFO(string filepath)
        {
            Init(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public PARAM_SFO(byte[] inputdata)
        {
            Init(new MemoryStream(inputdata));
        }

        public PARAM_SFO(Stream input)
        {
            Init(input);
        }

        private string ReadValue(BinaryReader br, index_table table)
        {
            br.BaseStream.Position = ((Header.DataTableStart) + table.param_data_offset);
            switch (table.param_data_fmt)
            {
                case FMT.ASCII:
                    //return Encoding.GetEncoding(1252).GetString(br.ReadBytes((int) table.param_data_max_len)).Replace("\0", "");
                    return Encoding.UTF8.GetString(br.ReadBytes((int)table.param_data_max_len)).Replace("\0", "");
                case FMT.UINT32:
                    return br.ReadUInt32().ToString();
                case FMT.UTF_8:
                    return Encoding.UTF8.GetString(br.ReadBytes((int)table.param_data_max_len)).Replace("\0", "");
                case FMT.Utf8Null:
                    return Encoding.UTF8.GetString(br.ReadBytes((int)table.param_data_max_len)).Replace("\0", "");
                default:
                    return null;
            }
        }

        private string ReadValueSpecialChars(BinaryReader br, index_table table)
        {
            br.BaseStream.Position = ((Header.DataTableStart) + table.param_data_offset);
            switch (table.param_data_fmt)
            {
                case FMT.ASCII:
                    return Encoding.UTF8.GetString(br.ReadBytes((int)table.param_data_max_len)).Replace("\0", "");
                case FMT.UINT32:
                    return br.ReadUInt32().ToString();
                case FMT.UTF_8:
                    return Encoding.UTF8.GetString(br.ReadBytes((int)table.param_data_max_len)).Replace("\0", "");
                default:
                    return null;
            }
        }

        private string ReadName(BinaryReader br, index_table table)
        {
            br.BaseStream.Position = (Header.KeyTableStart + table.param_key_offset);
            string name = "";
            while (((byte)br.PeekChar()) != 0)
                name += br.ReadChar();
            br.BaseStream.Position++;
            return name;
        }

        /// <summary>
        /// Start Reading the Parameter file
        /// </summary>
        /// <param name="input">Input Stream</param>
        private void Init(Stream input)
        {
            using (var br = new BinaryReader(input))
            {
                Header.Read(br);
                if (!Functions.CompareBytes(Header.Magic, new byte[] { 0, 0x50, 0x53, 0x46 }))
                    throw new Exception("Invalid PARAM.SFO Header Magic");
                var tables = new List<index_table>();
                for (int i = 0; i < Header.IndexTableEntries; i++)
                {
                    var t = new index_table();
                    t.Read(br);
                    tables.Add(t);
                }
                var xtables = new List<Table>();
                int count = 0;
                foreach (index_table t in tables)
                {
                    var x = new Table();
                    x.index = count;
                    x.Indextable = t;
                    x.Name = ReadName(br, t);
                    x.Value = ReadValue(br, t);
                    count++;
                    xtables.Add(x);
                }
                Tables = xtables;
                br.Close();
            }
        }


        #endregion << Methods >>

    }

    /// <summary>
    /// Byte Functions
    /// </summary>
    internal static class Functions
    {
        public static UInt16 SwapByteOrder(this UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        public static UInt32 SwapByteOrder(this UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static UInt64 SwapByteOrder(this UInt64 value)
        {
            return
                ((value & 0xff00000000000000L) >> 56) |
                ((value & 0x00ff000000000000L) >> 40) |
                ((value & 0x0000ff0000000000L) >> 24) |
                ((value & 0x000000ff00000000L) >> 8) |
                ((value & 0x00000000ff000000L) << 8) |
                ((value & 0x0000000000ff0000L) << 24) |
                ((value & 0x000000000000ff00L) << 40) |
                ((value & 0x00000000000000ffL) << 56);
        }

        public static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        public static byte[] StringToByteArray(this string hex)
        {
            if ((hex.Length % 2) != 0) hex = hex.PadLeft(hex.Length + 1, '0');
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }

    #region << Gim >>

    public interface IPixelOrderIterator
    {
        int X { get; }
        int Y { get; }
        void Next();
    }

    public class TiledPixelOrderIterator : IPixelOrderIterator
    {
        int Width;
        int Height;

        int CurrentTileX;
        int CurrentTileY;
        int CounterInTile;

        int TileWidth;
        int TileHeight;

        public TiledPixelOrderIterator(int width, int height, int tileWidth, int tileHeight)
        {
            Width = width;
            Height = height;
            CurrentTileX = 0;
            CurrentTileY = 0;
            CounterInTile = 0;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
        }

        public int X { get { return CurrentTileX + (CounterInTile % TileWidth); } }
        public int Y { get { return CurrentTileY + (CounterInTile / TileWidth); } }

        public void Next()
        {
            ++CounterInTile;

            if (CounterInTile == TileWidth * TileHeight)
            {
                CounterInTile = 0;
                CurrentTileX += TileWidth;
                if (CurrentTileX >= Width)
                {
                    CurrentTileX = 0;
                    CurrentTileY += TileHeight;
                }
            }
        }
    }

    public class LinearPixelOrderIterator : IPixelOrderIterator
    {
        int Width;
        int Height;
        int Counter;

        public LinearPixelOrderIterator(int width, int height)
        {
            Width = width;
            Height = height;
            Counter = 0;
        }

        public int X { get { return Counter % Width; } }
        public int Y { get { return Counter / Width; } }

        public void Next()
        {
            ++Counter;
        }
    }

    class Util
    {
        public static void CopyByteArrayPart(IList<byte> from, int locationFrom, IList<byte> to, int locationTo, int count)
        {
            for (int i = 0; i < count; i++)
            {
                to[locationTo + i] = from[locationFrom + i];
            }
        }
    }


    class FileInfoSection : ISection
    {
        public ushort Type;
        public ushort Unknown;
        public uint PartSizeDuplicate;
        public uint PartSize;
        public uint Unknown2;

        public byte[] FileInfo;

        public FileInfoSection(byte[] File, int Offset)
        {
            Type = BitConverter.ToUInt16(File, Offset);
            Unknown = BitConverter.ToUInt16(File, Offset + 0x02);
            PartSizeDuplicate = BitConverter.ToUInt32(File, Offset + 0x04);
            PartSize = BitConverter.ToUInt32(File, Offset + 0x08);
            Unknown2 = BitConverter.ToUInt32(File, Offset + 0x0C);

            uint size = PartSize - 0x10;
            FileInfo = new byte[size];
            Util.CopyByteArrayPart(File, Offset + 0x10, FileInfo, 0, (int)size);
        }

        public uint GetPartSize()
        {
            return PartSize;
        }


        public void Recalculate(int NewFilesize)
        {
            PartSize = (uint)FileInfo.Length + 0x10;
            PartSizeDuplicate = PartSize;
        }


        public byte[] Serialize()
        {
            List<byte> serialized = new List<byte>((int)PartSize);
            serialized.AddRange(BitConverter.GetBytes(Type));
            serialized.AddRange(BitConverter.GetBytes(Unknown));
            serialized.AddRange(BitConverter.GetBytes(PartSizeDuplicate));
            serialized.AddRange(BitConverter.GetBytes(PartSize));
            serialized.AddRange(BitConverter.GetBytes(Unknown2));
            serialized.AddRange(FileInfo);
            return serialized.ToArray();
        }
    }

    class Splitter
    {
        public static int Split(List<string> args)
        {
            string Filename = args[0];
            GIM[] gims = new GIM[3];
            gims[0] = new GIM(Filename); ;
            gims[1] = new GIM(Filename); ;
            gims[2] = new GIM(Filename); ;
            System.IO.File.WriteAllBytes(
                System.IO.Path.GetDirectoryName(Filename) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Filename) + "_resave" + System.IO.Path.GetExtension(Filename),
                gims[0].Serialize());

            for (int i = 0; i < gims.Length; ++i)
            {
                GIM gim = gims[i];
                gim.ReduceToOneImage(i);
                System.IO.File.WriteAllBytes(
                    System.IO.Path.GetDirectoryName(Filename) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Filename) + i.ToString() + System.IO.Path.GetExtension(Filename),
                    gim.Serialize());
            }

            return 0;
        }
    }

    class Program
    {
        public static int Homogenize(List<string> args)
        {
            if (args.Count == 0)
            {
                Console.WriteLine("HomogenizePalette in.gim [out.gim]");
                Console.WriteLine("Overwrites in.gim when no out.gim is provided.");
            }

            string infilename = args[0];
            string outfilename = args.Count > 1 ? args[1] : args[0];

            GIM gim = new GIM(infilename);
            gim.HomogenizePalette();
            System.IO.File.WriteAllBytes(outfilename, gim.Serialize());


            return 0;
        }
    }

    public class GimToPng
    {
        public static int Execute(List<string> args)
        {
            if (args.Count == 0)
            {
                Console.WriteLine("Usage: GimToPng file.gim");
                return -1;
            }

            string filename = args[0];
            List<string> convertedFilenames = ConvertGimFileToPngFiles(filename);
            return (convertedFilenames != null && convertedFilenames.Count > 0) ? 0 : -1;
        }

        public static List<string> ConvertGimFileToPngFiles(string filename)
        {
            GIM gim = new GIM(filename);
            int filenum = 0;
            List<string> names = new List<string>();
            foreach (Bitmap bmp in gim.ConvertToBitmaps())
            {
                string newname = filename + "." + filenum + ".png";
                bmp.Save(newname);
                names.Add(newname);
            }
            return names;
        }
    }

    class EndOfImageSection : ISection
    {
        public ushort Type;
        public ushort Unknown;
        public uint EndOfImageAddress;
        public uint PartSize;
        public uint Unknown2;

        public EndOfImageSection(byte[] File, int Offset)
        {
            Type = BitConverter.ToUInt16(File, Offset);
            Unknown = BitConverter.ToUInt16(File, Offset + 0x02);
            EndOfImageAddress = BitConverter.ToUInt32(File, Offset + 0x04);
            PartSize = BitConverter.ToUInt32(File, Offset + 0x08);
            Unknown2 = BitConverter.ToUInt32(File, Offset + 0x0C);
        }


        public uint GetPartSize()
        {
            return PartSize;
        }


        public void Recalculate(int NewFilesize)
        {
            EndOfImageAddress = (uint)NewFilesize;
        }


        public byte[] Serialize()
        {
            List<byte> serialized = new List<byte>((int)PartSize);
            serialized.AddRange(BitConverter.GetBytes(Type));
            serialized.AddRange(BitConverter.GetBytes(Unknown));
            serialized.AddRange(BitConverter.GetBytes(EndOfImageAddress));
            serialized.AddRange(BitConverter.GetBytes(PartSize));
            serialized.AddRange(BitConverter.GetBytes(Unknown2));
            return serialized.ToArray();
        }
    }


    class EndOfFileSection : ISection
    {

        public ushort Type;
        public ushort Unknown;
        public uint EndOfFileAddress;
        public uint PartSize;
        public uint Unknown2;

        public EndOfFileSection(byte[] File, int Offset)
        {
            Type = BitConverter.ToUInt16(File, Offset);
            Unknown = BitConverter.ToUInt16(File, Offset + 0x02);
            EndOfFileAddress = BitConverter.ToUInt32(File, Offset + 0x04);
            PartSize = BitConverter.ToUInt32(File, Offset + 0x08);
            Unknown2 = BitConverter.ToUInt32(File, Offset + 0x0C);
        }


        public uint GetPartSize()
        {
            return PartSize;
        }


        public void Recalculate(int NewFilesize)
        {
            EndOfFileAddress = (uint)NewFilesize;
        }


        public byte[] Serialize()
        {
            List<byte> serialized = new List<byte>((int)PartSize);
            serialized.AddRange(BitConverter.GetBytes(Type));
            serialized.AddRange(BitConverter.GetBytes(Unknown));
            serialized.AddRange(BitConverter.GetBytes(EndOfFileAddress));
            serialized.AddRange(BitConverter.GetBytes(PartSize));
            serialized.AddRange(BitConverter.GetBytes(Unknown2));
            return serialized.ToArray();
        }
    }

    class HeaderSection : ISection
    {
        public byte[] Header;
        public HeaderSection(byte[] File, int Offset)
        {
            Header = new byte[0x10];

            Util.CopyByteArrayPart(File, Offset, Header, 0, 0x10);
        }

        public uint GetPartSize()
        {
            return 0x10;
        }

        public void Recalculate(int NewFilesize)
        {
            return;
        }


        public byte[] Serialize()
        {
            return Header;
        }
    }

    class PaletteSection : ISection
    {
        public int Offset;

        public ushort Type;
        public ushort Unknown;
        public uint PartSizeDuplicate;
        public uint PartSize;
        public uint Unknown2;

        public ushort DataOffset;
        public ushort Unknown3;
        public ImageFormat Format;
        public ushort Unknown4;
        public ushort ColorDepth;
        public ushort Unknown5;
        public ushort Unknown6;
        public ushort Unknown7;

        public ushort Unknown8;
        public ushort Unknown9;
        public ushort Unknown10;
        public ushort Unknown11;
        public uint Unknown12;
        public uint Unknown13;

        public uint PartSizeMinus0x10;
        public uint Unknown14;
        public ushort Unknown15;
        public ushort LayerCount;
        public ushort Unknown17;
        public ushort FrameCount;

        public uint[] PaletteOffsets;
        public byte[][] PalettesRawBytes;
        public List<List<uint>> Palettes;


        public uint PaletteCount;

        public PaletteSection(byte[] File, int Offset)
        {
            this.Offset = Offset;


            Type = BitConverter.ToUInt16(File, Offset);
            Unknown = BitConverter.ToUInt16(File, Offset + 0x02);
            PartSizeDuplicate = BitConverter.ToUInt32(File, Offset + 0x04);
            PartSize = BitConverter.ToUInt32(File, Offset + 0x08);
            Unknown2 = BitConverter.ToUInt32(File, Offset + 0x0C);

            DataOffset = BitConverter.ToUInt16(File, Offset + 0x10);
            Unknown3 = BitConverter.ToUInt16(File, Offset + 0x12);
            Format = (ImageFormat)BitConverter.ToUInt16(File, Offset + 0x14);
            Unknown4 = BitConverter.ToUInt16(File, Offset + 0x16);
            ColorDepth = BitConverter.ToUInt16(File, Offset + 0x18);
            Unknown5 = BitConverter.ToUInt16(File, Offset + 0x1A);
            Unknown6 = BitConverter.ToUInt16(File, Offset + 0x1C);
            Unknown7 = BitConverter.ToUInt16(File, Offset + 0x1E);

            Unknown8 = BitConverter.ToUInt16(File, Offset + 0x20);
            Unknown9 = BitConverter.ToUInt16(File, Offset + 0x22);
            Unknown10 = BitConverter.ToUInt16(File, Offset + 0x24);
            Unknown11 = BitConverter.ToUInt16(File, Offset + 0x26);
            Unknown12 = BitConverter.ToUInt32(File, Offset + 0x28);
            Unknown13 = BitConverter.ToUInt32(File, Offset + 0x2C);

            PartSizeMinus0x10 = BitConverter.ToUInt32(File, Offset + 0x30);
            Unknown14 = BitConverter.ToUInt32(File, Offset + 0x34);
            Unknown15 = BitConverter.ToUInt16(File, Offset + 0x38);
            LayerCount = BitConverter.ToUInt16(File, Offset + 0x3A);
            Unknown17 = BitConverter.ToUInt16(File, Offset + 0x3C);
            FrameCount = BitConverter.ToUInt16(File, Offset + 0x3E);

            PaletteCount = Math.Max(LayerCount, FrameCount);
            PaletteOffsets = new uint[PaletteCount];
            for (int i = 0; i < PaletteCount; ++i)
            {
                PaletteOffsets[i] = BitConverter.ToUInt32(File, Offset + 0x40 + i * 0x04);
            }


            PalettesRawBytes = new byte[PaletteCount][];
            for (int i = 0; i < PaletteOffsets.Length; ++i)
            {
                uint poffs = PaletteOffsets[i];
                int size = ColorDepth * GetBytePerColor();
                PalettesRawBytes[i] = new byte[size];

                Util.CopyByteArrayPart(File, Offset + (int)poffs + 0x10, PalettesRawBytes[i], 0, size);
            }


            Palettes = new List<List<uint>>();
            foreach (byte[] pal in PalettesRawBytes)
            {
                int BytePerColor = GetBytePerColor();
                List<uint> IndividualPalette = new List<uint>();
                for (int i = 0; i < pal.Length; i += BytePerColor)
                {
                    uint color = 0;
                    if (BytePerColor == 4)
                    {
                        color = BitConverter.ToUInt32(pal, i);
                    }
                    else if (BytePerColor == 2)
                    {
                        color = BitConverter.ToUInt16(pal, i);
                    }
                    IndividualPalette.Add(color);
                }
                Palettes.Add(IndividualPalette);
            }


            return;
        }

        public int GetBytePerColor()
        {
            if (Format == ImageFormat.RGBA4444)
            {
                return 2;
            }
            return 4;
        }


        public uint GetPartSize()
        {
            return PartSize;
        }
        public void Recalculate(int NewFilesize)
        {
            if (PaletteOffsets.Length != PalettesRawBytes.Length)
            {
                PaletteOffsets = new uint[PalettesRawBytes.Length];
            }
            uint totalLength = 0;
            for (int i = 0; i < PalettesRawBytes.Length; ++i)
            {
                PaletteOffsets[i] = totalLength + 0x40;
                totalLength += (uint)PalettesRawBytes[i].Length;
            }

            PartSize = totalLength + 0x50;
            PartSizeDuplicate = totalLength + 0x50;
            PartSizeMinus0x10 = totalLength + 0x40;
            LayerCount = 1;
            FrameCount = 1;
        }


        public byte[] Serialize()
        {
            List<byte> serialized = new List<byte>((int)PartSize);
            serialized.AddRange(BitConverter.GetBytes(Type));
            serialized.AddRange(BitConverter.GetBytes(Unknown));
            serialized.AddRange(BitConverter.GetBytes(PartSizeDuplicate));
            serialized.AddRange(BitConverter.GetBytes(PartSize));
            serialized.AddRange(BitConverter.GetBytes(Unknown2));

            serialized.AddRange(BitConverter.GetBytes(DataOffset));
            serialized.AddRange(BitConverter.GetBytes(Unknown3));
            serialized.AddRange(BitConverter.GetBytes((ushort)Format));
            serialized.AddRange(BitConverter.GetBytes(Unknown4));
            serialized.AddRange(BitConverter.GetBytes(ColorDepth));
            serialized.AddRange(BitConverter.GetBytes(Unknown5));
            serialized.AddRange(BitConverter.GetBytes(Unknown6));
            serialized.AddRange(BitConverter.GetBytes(Unknown7));

            serialized.AddRange(BitConverter.GetBytes(Unknown8));
            serialized.AddRange(BitConverter.GetBytes(Unknown9));
            serialized.AddRange(BitConverter.GetBytes(Unknown10));
            serialized.AddRange(BitConverter.GetBytes(Unknown11));
            serialized.AddRange(BitConverter.GetBytes(Unknown12));
            serialized.AddRange(BitConverter.GetBytes(Unknown13));

            serialized.AddRange(BitConverter.GetBytes(PartSizeMinus0x10));
            serialized.AddRange(BitConverter.GetBytes(Unknown14));
            serialized.AddRange(BitConverter.GetBytes(Unknown15));
            serialized.AddRange(BitConverter.GetBytes(LayerCount));
            serialized.AddRange(BitConverter.GetBytes(Unknown17));
            serialized.AddRange(BitConverter.GetBytes(FrameCount));

            for (int i = 0; i < PaletteOffsets.Length; ++i)
            {
                serialized.AddRange(BitConverter.GetBytes(PaletteOffsets[i]));
            }
            while (serialized.Count % 16 != 0)
            {
                serialized.Add(0x00);
            }
            int BytePerColor = GetBytePerColor();
            foreach (List<uint> pal in Palettes)
            {
                foreach (uint col in pal)
                {
                    if (BytePerColor == 4)
                    {
                        serialized.AddRange(BitConverter.GetBytes(col));
                    }
                    else if (BytePerColor == 2)
                    {
                        serialized.AddRange(BitConverter.GetBytes((ushort)col));
                    }
                }
            }
            return serialized.ToArray();
        }
    }

    enum ImageFormat : short
    {
        RGBA5650 = 0,
        RGBA5551 = 1,
        RGBA4444 = 2,
        RGBA8888 = 3,
        Index4 = 4,
        Index8 = 5,
        Index16 = 6,
        Index32 = 7,
    }

    enum PixelOrder : short
    {
        Normal = 0,
        Faster = 1
    }

    class ImageSection : ISection
    {
        public int Offset;

        public ushort Type;
        public ushort Unknown;
        public uint PartSizeDuplicate;
        public uint PartSize;
        public uint Unknown2;

        public ushort DataOffset;
        public ushort Unknown3;
        public ImageFormat Format;
        public PixelOrder PxOrder;
        public ushort Width;
        public ushort Height;
        public ushort ColorDepth;
        public ushort Unknown7;

        public ushort Unknown8;
        public ushort Unknown9;
        public ushort Unknown10;
        public ushort Unknown11;
        public uint Unknown12;
        public uint Unknown13;

        public uint PartSizeMinus0x10;
        public uint Unknown14;
        public ushort Unknown15;
        public ushort LayerCount;
        public ushort Unknown17;
        public ushort FrameCount;

        public uint[] ImageOffsets;
        public byte[][] ImagesRawBytes;
        public List<List<uint>> Images;


        public uint ImageCount;

        public ImageSection(byte[] File, int Offset)
        {
            this.Offset = Offset;


            Type = BitConverter.ToUInt16(File, Offset);
            Unknown = BitConverter.ToUInt16(File, Offset + 0x02);
            PartSizeDuplicate = BitConverter.ToUInt32(File, Offset + 0x04);
            PartSize = BitConverter.ToUInt32(File, Offset + 0x08);
            Unknown2 = BitConverter.ToUInt32(File, Offset + 0x0C);

            DataOffset = BitConverter.ToUInt16(File, Offset + 0x10);
            Unknown3 = BitConverter.ToUInt16(File, Offset + 0x12);
            Format = (ImageFormat)BitConverter.ToUInt16(File, Offset + 0x14);
            PxOrder = (PixelOrder)BitConverter.ToUInt16(File, Offset + 0x16);
            Width = BitConverter.ToUInt16(File, Offset + 0x18);
            Height = BitConverter.ToUInt16(File, Offset + 0x1A);
            ColorDepth = BitConverter.ToUInt16(File, Offset + 0x1C);
            Unknown7 = BitConverter.ToUInt16(File, Offset + 0x1E);

            Unknown8 = BitConverter.ToUInt16(File, Offset + 0x20);
            Unknown9 = BitConverter.ToUInt16(File, Offset + 0x22);
            Unknown10 = BitConverter.ToUInt16(File, Offset + 0x24);
            Unknown11 = BitConverter.ToUInt16(File, Offset + 0x26);
            Unknown12 = BitConverter.ToUInt32(File, Offset + 0x28);
            Unknown13 = BitConverter.ToUInt32(File, Offset + 0x2C);

            PartSizeMinus0x10 = BitConverter.ToUInt32(File, Offset + 0x30);
            Unknown14 = BitConverter.ToUInt32(File, Offset + 0x34);
            Unknown15 = BitConverter.ToUInt16(File, Offset + 0x38);
            LayerCount = BitConverter.ToUInt16(File, Offset + 0x3A);
            Unknown17 = BitConverter.ToUInt16(File, Offset + 0x3C);
            FrameCount = BitConverter.ToUInt16(File, Offset + 0x3E);

            ImageCount = Math.Max(LayerCount, FrameCount);
            ImageOffsets = new uint[ImageCount];
            for (int i = 0; i < ImageCount; ++i)
            {
                ImageOffsets[i] = BitConverter.ToUInt32(File, Offset + 0x40 + i * 0x04);
            }


            ImagesRawBytes = new byte[ImageCount][];
            for (int i = 0; i < ImageOffsets.Length; ++i)
            {
                uint poffs = ImageOffsets[i];
                uint nextpoffs;
                if (i == ImageOffsets.Length - 1)
                {
                    nextpoffs = PartSizeMinus0x10;
                }
                else
                {
                    nextpoffs = ImageOffsets[i + 1];
                }
                uint size = nextpoffs - poffs;
                ImagesRawBytes[i] = new byte[size];

                Util.CopyByteArrayPart(File, Offset + (int)poffs + 0x10, ImagesRawBytes[i], 0, (int)size);
            }



            Images = new List<List<uint>>();
            foreach (byte[] img in ImagesRawBytes)
            {
                int BitPerPixel = GetBitPerPixel();
                List<uint> IndividualImage = new List<uint>();
                for (int cnt = 0; cnt < img.Length * 8; cnt += BitPerPixel)
                {
                    uint color = 0;
                    int i = cnt / 8;
                    switch (BitPerPixel)
                    {
                        case 4:
                            if (cnt % 8 != 0)
                            {
                                color = (img[i] & 0xF0u) >> 4;
                            }
                            else
                            {
                                color = (img[i] & 0x0Fu);
                            }
                            break;
                        case 8:
                            color = img[i];
                            break;
                        case 16:
                            color = BitConverter.ToUInt16(img, i);
                            break;
                        case 32:
                            color = BitConverter.ToUInt32(img, i);
                            break;
                    }
                    IndividualImage.Add(color);
                }
                Images.Add(IndividualImage);
            }


            return;
        }

        public int GetBitPerPixel()
        {
            switch (Format)
            {
                case ImageFormat.Index4:
                    return 4;
                case ImageFormat.Index8:
                    return 8;
                case ImageFormat.Index16:
                case ImageFormat.RGBA4444:
                case ImageFormat.RGBA5551:
                case ImageFormat.RGBA5650:
                    return 16;
                case ImageFormat.Index32:
                case ImageFormat.RGBA8888:
                    return 32;
            }
            return 0;
        }

        public uint GetPartSize()
        {
            return PartSize;
        }


        public void Recalculate(int NewFilesize)
        {
            if (ImageOffsets.Length != ImagesRawBytes.Length)
            {
                ImageOffsets = new uint[ImagesRawBytes.Length];
            }

            uint totalLength = 0;
            for (int i = 0; i < ImagesRawBytes.Length; ++i)
            {
                ImageOffsets[i] = totalLength + 0x40;
                totalLength += (uint)ImagesRawBytes[i].Length;
            }

            PartSize = totalLength + 0x50;
            PartSizeDuplicate = totalLength + 0x50;
            PartSizeMinus0x10 = totalLength + 0x40;
            LayerCount = 1;
            FrameCount = 1;

        }

        private static Color ColorFromRGBA5650(uint color)
        {
            int r = (int)(((color & 0x0000001F)) << 3);
            int g = (int)(((color & 0x000007E0) >> 5) << 2);
            int b = (int)(((color & 0x0000F800) >> 11) << 3);
            return Color.FromArgb(0, r, g, b);
        }
        private static Color ColorFromRGBA5551(uint color)
        {
            int r = (int)(((color & 0x0000001F)) << 3);
            int g = (int)(((color & 0x000003E0) >> 5) << 3);
            int b = (int)(((color & 0x00007C00) >> 10) << 3);
            int a = (int)(((color & 0x00008000) >> 15) << 7);
            return Color.FromArgb(a, r, g, b);
        }
        private static Color ColorFromRGBA4444(uint color)
        {
            int r = (int)(((color & 0x0000000F)) << 4);
            int g = (int)(((color & 0x000000F0) >> 4) << 4);
            int b = (int)(((color & 0x00000F00) >> 8) << 4);
            int a = (int)(((color & 0x0000F000) >> 12) << 4);
            return Color.FromArgb(a, r, g, b);
        }
        private static Color ColorFromRGBA8888(uint color)
        {
            int r = (int)((color & 0x000000FF));
            int g = (int)((color & 0x0000FF00) >> 8);
            int b = (int)((color & 0x00FF0000) >> 16);
            int a = (int)((color & 0xFF000000) >> 24);
            return Color.FromArgb(a, r, g, b);
        }

        public List<Bitmap> ConvertToBitmaps(PaletteSection psec)
        {
            List<Bitmap> bitmaps = new List<Bitmap>();
            for (int i = 0; i < Images.Count; ++i)
            {
                int w = (ushort)(Width >> i);
                int h = (ushort)(Height >> i);

                Bitmap bmp = new Bitmap(w, h);

                IPixelOrderIterator pixelPosition;
                switch (PxOrder)
                {
                    case PixelOrder.Normal:
                        pixelPosition = new LinearPixelOrderIterator(w, h);
                        break;
                    case PixelOrder.Faster:
                        pixelPosition = new GimPixelOrderFasterIterator(w, h, GetBitPerPixel());
                        break;
                    default:
                        throw new Exception("Unexpected pixel order: " + PxOrder);
                }

                for (int idx = 0; idx < Images[i].Count; ++idx)
                {
                    uint rawcolor = Images[i][idx];
                    Color color;

                    switch (Format)
                    {
                        case ImageFormat.RGBA5650:
                            color = ColorFromRGBA5650(rawcolor);
                            break;
                        case ImageFormat.RGBA5551:
                            color = ColorFromRGBA5551(rawcolor);
                            break;
                        case ImageFormat.RGBA4444:
                            color = ColorFromRGBA4444(rawcolor);
                            break;
                        case ImageFormat.RGBA8888:
                            color = ColorFromRGBA8888(rawcolor);
                            break;
                        case ImageFormat.Index4:
                        case ImageFormat.Index8:
                        case ImageFormat.Index16:
                        case ImageFormat.Index32:
                            switch (psec.Format)
                            {
                                case ImageFormat.RGBA5650:
                                    color = ColorFromRGBA5650(psec.Palettes[i][(int)rawcolor]);
                                    break;
                                case ImageFormat.RGBA5551:
                                    color = ColorFromRGBA5551(psec.Palettes[i][(int)rawcolor]);
                                    break;
                                case ImageFormat.RGBA4444:
                                    color = ColorFromRGBA4444(psec.Palettes[i][(int)rawcolor]);
                                    break;
                                case ImageFormat.RGBA8888:
                                    color = ColorFromRGBA8888(psec.Palettes[i][(int)rawcolor]);
                                    break;
                                default:
                                    throw new Exception("Unexpected palette color type: " + psec.Format);
                            }
                            break;
                        default:
                            throw new Exception("Unexpected image color type: " + psec.Format);
                    }

                    if (pixelPosition.X < w && pixelPosition.Y < h)
                    {
                        bmp.SetPixel(pixelPosition.X, pixelPosition.Y, color);
                    }
                    pixelPosition.Next();
                }
                bitmaps.Add(bmp);
            }
            return bitmaps;
        }

        public byte[] Serialize()
        {
            List<byte> serialized = new List<byte>((int)PartSize);
            serialized.AddRange(BitConverter.GetBytes(Type));
            serialized.AddRange(BitConverter.GetBytes(Unknown));
            serialized.AddRange(BitConverter.GetBytes(PartSizeDuplicate));
            serialized.AddRange(BitConverter.GetBytes(PartSize));
            serialized.AddRange(BitConverter.GetBytes(Unknown2));

            serialized.AddRange(BitConverter.GetBytes(DataOffset));
            serialized.AddRange(BitConverter.GetBytes(Unknown3));
            serialized.AddRange(BitConverter.GetBytes((ushort)Format));
            serialized.AddRange(BitConverter.GetBytes((ushort)PxOrder));
            serialized.AddRange(BitConverter.GetBytes(Width));
            serialized.AddRange(BitConverter.GetBytes(Height));
            serialized.AddRange(BitConverter.GetBytes(ColorDepth));
            serialized.AddRange(BitConverter.GetBytes(Unknown7));

            serialized.AddRange(BitConverter.GetBytes(Unknown8));
            serialized.AddRange(BitConverter.GetBytes(Unknown9));
            serialized.AddRange(BitConverter.GetBytes(Unknown10));
            serialized.AddRange(BitConverter.GetBytes(Unknown11));
            serialized.AddRange(BitConverter.GetBytes(Unknown12));
            serialized.AddRange(BitConverter.GetBytes(Unknown13));

            serialized.AddRange(BitConverter.GetBytes(PartSizeMinus0x10));
            serialized.AddRange(BitConverter.GetBytes(Unknown14));
            serialized.AddRange(BitConverter.GetBytes(Unknown15));
            serialized.AddRange(BitConverter.GetBytes(LayerCount));
            serialized.AddRange(BitConverter.GetBytes(Unknown17));
            serialized.AddRange(BitConverter.GetBytes(FrameCount));

            for (int i = 0; i < ImageOffsets.Length; ++i)
            {
                serialized.AddRange(BitConverter.GetBytes(ImageOffsets[i]));
            }
            while (serialized.Count % 16 != 0)
            {
                serialized.Add(0x00);
            }

            int BitPerPixel = GetBitPerPixel();
            foreach (List<uint> img in Images)
            {
                for (int i = 0; i < img.Count; ++i)
                {
                    uint col = img[i];
                    switch (BitPerPixel)
                    {
                        case 4:
                            col = (img[i + 1] << 4) | (img[i]);
                            serialized.Add((byte)col);
                            ++i;
                            break;
                        case 8:
                            serialized.Add((byte)col);
                            break;
                        case 16:
                            serialized.AddRange(BitConverter.GetBytes((ushort)col));
                            break;
                        case 32:
                            serialized.AddRange(BitConverter.GetBytes(col));
                            break;
                    }
                }
            }

            return serialized.ToArray();
        }

        public void ConvertToTruecolor(int imageNumber, List<uint> Palette)
        {
            for (int i = 0; i < Images[imageNumber].Count; ++i)
            {
                uint index = Images[imageNumber][i];
                Images[imageNumber][i] = Palette[(int)index];
            }
        }

        public void CovertToPaletted(int imageNumber, uint[] NewPalette)
        {
            Dictionary<uint, uint> PaletteDict = new Dictionary<uint, uint>(NewPalette.Length);
            for (uint i = 0; i < NewPalette.Length; ++i)
            {
                try
                {
                    PaletteDict.Add(NewPalette[i], i);
                }
                catch (System.ArgumentException)
                {
                    // if we reach a duplicate we *should* be at the end of our colors
                    break;
                }
            }

            for (int i = 0; i < Images[imageNumber].Count; ++i)
            {
                uint color = Images[imageNumber][i];
                uint index = PaletteDict[color];
                Images[imageNumber][i] = index;
            }
        }

        public void DiscardUnusedColorsPaletted(int imageNumber, PaletteSection paletteSection, int paletteNumber)
        {
            List<uint> pal = paletteSection.Palettes[paletteNumber];
            List<uint> img = Images[imageNumber];

            bool[] usedPaletteEntries = new bool[pal.Count];
            for (int i = 0; i < usedPaletteEntries.Length; ++i)
            {
                usedPaletteEntries[i] = false; // initialize array to false
            }
            for (int i = 0; i < img.Count; ++i)
            {
                usedPaletteEntries[img[i]] = true; // all used palette entries get set to true
            }

            // remap old palette entries to new ones by essentially skipping over all unused colors
            uint[] remapTable = new uint[pal.Count];
            uint counter = 0;
            for (int i = 0; i < usedPaletteEntries.Length; ++i)
            {
                if (usedPaletteEntries[i])
                {
                    remapTable[i] = counter;
                    counter++;
                }
                else
                {
                    remapTable[i] = 0xFFFFFFFFu; // just making sure these aren't used
                }
            }

            // remap the image
            for (int i = 0; i < img.Count; ++i)
            {
                img[i] = remapTable[img[i]];
            }

            // generate the new palette
            List<uint> newPal = new List<uint>((int)counter);
            for (int i = 0; i < usedPaletteEntries.Length; ++i)
            {
                if (usedPaletteEntries[i])
                {
                    newPal.Add(pal[i]);
                }
            }

            paletteSection.Palettes[paletteNumber] = newPal;
        }
    }

    class GimPixelOrderFasterIterator : TiledPixelOrderIterator
    {
        public GimPixelOrderFasterIterator(int width, int height, int bpp) : base(width, height, 0x80 / bpp, 0x08) { }
    }

    interface ISection
    {
        uint GetPartSize();
        void Recalculate(int NewFilesize);
        byte[] Serialize();
    }
    public class GIM
    {

        byte[] File;
        List<ISection> Sections;

        public GIM(byte[] File)
        {
            Initialize(File);
        }

        public GIM(string Filename)
        {
            Initialize(System.IO.File.ReadAllBytes(Filename));
        }

        public void Initialize(byte[] File)
        {
            this.File = File;
            uint location = 0x10;

            Sections = new List<ISection>();
            Sections.Add(new HeaderSection(File, 0));
            while (location < File.Length)
            {
                ushort CurrentType = BitConverter.ToUInt16(File, (int)location);
                ISection section;
                switch (CurrentType)
                {
                    case 0x02:
                        section = new EndOfFileSection(File, (int)location);
                        break;
                    case 0x03:
                        section = new EndOfImageSection(File, (int)location);
                        break;
                    case 0x04:
                        section = new ImageSection(File, (int)location);
                        break;
                    case 0x05:
                        section = new PaletteSection(File, (int)location);
                        break;
                    case 0xFF:
                        section = new FileInfoSection(File, (int)location);
                        break;
                    default:
                        throw new Exception("Invalid Section Type");
                }

                Sections.Add(section);
                location += section.GetPartSize();
            }
        }

        public uint GetTotalFilesize()
        {
            uint totalFilesize = 0;
            foreach (var section in Sections)
            {
                totalFilesize += section.GetPartSize();
            }
            return totalFilesize;
        }

        public void ReduceToOneImage(int imageNumber)
        {
            foreach (var section in Sections)
            {
                if (section.GetType() == typeof(ImageSection))
                {
                    ImageSection isec = (ImageSection)section;
                    byte[] img = isec.ImagesRawBytes[imageNumber];
                    isec.ImagesRawBytes = new byte[1][];
                    isec.ImagesRawBytes[0] = img;
                    isec.Width = (ushort)(isec.Width >> imageNumber);
                    isec.Height = (ushort)(isec.Height >> imageNumber);
                }
                if (section.GetType() == typeof(PaletteSection))
                {
                    PaletteSection psec = (PaletteSection)section;
                    byte[] pal = psec.PalettesRawBytes[imageNumber];
                    psec.PalettesRawBytes = new byte[1][];
                    psec.PalettesRawBytes[0] = pal;
                }
            }

            uint fileinfosection = 0;
            foreach (var section in Sections)
            {
                section.Recalculate(0);
                if (section.GetType() == typeof(FileInfoSection))
                {
                    fileinfosection = section.GetPartSize();
                }
            }
            uint Filesize = GetTotalFilesize();
            foreach (var section in Sections)
            {
                if (section.GetType() == typeof(EndOfFileSection))
                {
                    section.Recalculate((int)Filesize - 0x10);
                }
                if (section.GetType() == typeof(EndOfImageSection))
                {
                    section.Recalculate((int)Filesize - 0x20 - (int)fileinfosection);
                }
            }
        }

        public List<System.Drawing.Bitmap> ConvertToBitmaps()
        {
            ImageSection isec = null;
            PaletteSection psec = null;
            foreach (var section in Sections)
            {
                if (section.GetType() == typeof(ImageSection))
                {
                    isec = (ImageSection)section;
                }
                if (section.GetType() == typeof(PaletteSection))
                {
                    psec = (PaletteSection)section;
                }
            }

            return isec.ConvertToBitmaps(psec);
        }

        public void HomogenizePalette()
        {
            ImageSection isec = null;
            PaletteSection psec = null;
            foreach (var section in Sections)
            {
                if (section.GetType() == typeof(ImageSection))
                {
                    isec = (ImageSection)section;
                }
                if (section.GetType() == typeof(PaletteSection))
                {
                    psec = (PaletteSection)section;
                }
            }

            for (int i = 0; i < isec.ImageCount; ++i)
            {
                isec.DiscardUnusedColorsPaletted(i, psec, i);
            }

            List<uint> PaletteList = new List<uint>();
            foreach (List<uint> pal in psec.Palettes)
            {
                PaletteList.AddRange(pal);
            }
            List<uint> NewPalette = PaletteList.Distinct().ToList();

            int maxColors = 1 << isec.ColorDepth;
            if (NewPalette.Count > maxColors)
            {
                string err = "ERROR: Combined Palette over the amount of allowed colors. (" + NewPalette.Count + " > " + maxColors + ")";
                Console.WriteLine(err);
                throw new Exception(err);
            }
            while (NewPalette.Count < maxColors)
            {
                NewPalette.Add(0);
            }

            for (int i = 0; i < isec.ImageCount; ++i)
            {
                isec.ConvertToTruecolor(i, psec.Palettes[i]);
                isec.CovertToPaletted(i, NewPalette.ToArray());
                psec.Palettes[i] = NewPalette.ToList();
            }
        }


        public byte[] Serialize()
        {
            List<byte> newfile = new List<byte>(File.Length);
            foreach (var section in Sections)
            {
                newfile.AddRange(section.Serialize());
            }
            return newfile.ToArray();
        }
    }

    #endregion << Gim >>
}
