using System;
using System.Collections.Generic;
using System.Diagnostics;
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
}
