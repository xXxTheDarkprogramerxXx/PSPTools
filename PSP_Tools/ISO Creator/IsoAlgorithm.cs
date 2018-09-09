using System;
using System.IO;
using System.Text;
namespace IsoCreator {
	internal static class IsoAlgorithm {

		#region Constants

		public static UInt32 SectorSize {
			get {
				return 0x800;
			}
		}

		public static byte DefaultDirectoryRecordLength {
			get {
				return (byte)34;
			}
		}

		public static byte DefaultPathTableRecordLength {
			get {
				return (byte)10;
			}
		}

		public static byte AsciiBlank {
			get {
				return (byte)32;
			}
		}

		public static byte[] UnicodeBlank {
			get {
				return new byte[] { 0, AsciiBlank };
			}
		}

		public static int SystemIdLength {
			get {
				return 32;
			}
		}

		public static int VolumeIdLength {
			get {
				return 32;
			}
		}

		public static int VolumeSetIdLength {
			get {
				return 128;
			}
		}

		public static int PublisherIdLength {
			get {
				return 128;
			}
		}

		public static int PreparerIdLength {
			get {
				return 128;
			}
		}

		public static int ApplicationIdLength {
			get {
				return 128;
			}
		}

		public static int CopyrightFileIdLength {
			get {
				return 37;
			}
		}

		public static int AbstractFileIdLength {
			get {
				return 37;
			}
		}

		public static int BibliographicFileIdLength {
			get {
				return 37;
			}
		}

		public static DateTime NoDate {
			get {
				return new DateTime( 1900, 1, 1, 0, 0, 0, 0 );
			}
		}

		public static int FileNameMaxLength {
			get {
				return 101; // 101*2+33+1 = 236
			}
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Checks whether a file name begins with ".". If it does, returns true. Otherwise false.
		/// </summary>
		/// <param name="fileName">A string representing the file name</param>
		/// <returns>True if is subdir, false otherwise.</returns>
		public static bool IsSubDir( String fileName ) {
			return ( !fileName.StartsWith( "." ) );
		}

		/// <summary>
		/// Transforms a 4 byte unsigned int into a both endian 8 byte unsigned int.
		/// </summary>
		/// <param name="value">A 4 byte unsigned int.</param>
		/// <returns>A 8 byte both endian unsigned int.</returns>
		public static UInt64 BothEndian( UInt32 value ) {
			UInt64 mask0 = 0xFF000000;
			UInt64 mask1 = 0x00FF0000;
			UInt64 mask2 = 0x0000FF00;
			UInt64 mask3 = 0x000000FF;

			return (UInt64)value | 
				   (UInt64)( ( value & mask0 ) << 8 ) | 
				   (UInt64)( ( value & mask1 ) << 24 ) | 
				   (UInt64)( ( value & mask2 ) << 40 ) | 
				   (UInt64)( ( value & mask3 ) << 56 );
		}

		/// <summary>
		/// Transforms a 2 byte unsigned int into a both endian 4 byte unsigned int.
		/// </summary>
		/// <param name="value">A 2 byte unsigned int.</param>
		/// <returns>A 4 byte both endian unsigned int.</returns>
		public static UInt32 BothEndian( UInt16 value ) {
			UInt32 mask0 = 0xFF00;
			UInt32 mask1 = 0x00FF;

			return (UInt32)value | 
				   (UInt32)( ( value & mask0 ) << 8 ) | 
				   (UInt32)( ( value & mask1 ) << 24 );
		}

		/// <summary>
		/// Extracts a 4 byte unsigned int from an 8 byte both endian unsigned int.
		/// </summary>
		/// <param name="value">An 8 byte both endian unsigned int.</param>
		/// <returns>A 4 byte unsigned int.</returns>
		public static UInt32 ValueFromBothEndian( UInt64 value ) {
			return (UInt32)( value & ( ( (UInt64)1 << 32 ) - 1 ) );
		}

		/// <summary>
		/// Extracts a 2 byte unsigned int from an 4 byte both endian unsigned int.
		/// </summary>
		/// <param name="value">An 4 byte both endian unsigned int.</param>
		/// <returns>A 2 byte unsigned int.</returns>
		public static UInt16 ValueFromBothEndian( UInt32 value ) {
			return (UInt16)( value & ( ( 1 << 16 ) - 1 ) );
		}

		/// <summary>
		/// Creates an array containing a specified number of bytes of a certain value.
		/// </summary>
		/// <param name="count">Te number of bytes to fill.</param>
		/// <param name="value">The value which to fill the array with.</param>
		/// <returns>A byte array.</returns>
		public static byte[] MemSet( int count, byte value ) {
			byte[] result = new byte[count];
			for ( int i=0; i<count; i++ ) {
				result[i] = value;
			}

			return result;
		}

		/// <summary>
		/// Creates a byte array containing the specified value a specified number of times.
		/// </summary>
		/// <param name="count">The number of times which the value will be found in the byte array.</param>
		/// <param name="value">The value which to fill the array with.</param>
		/// <returns>A byte array.</returns>
		public static byte[] MemSet( int count, byte[] value ) {
			byte[] result = new byte[count*value.Length];

			for ( int i=0; i<count; i++ ) {
				value.CopyTo( result, i*value.Length );
			}

			return result;
		}

		/// <summary>
		/// Converts an ascii text (8 bit string) to a Unicode text (16 bit string).
		/// </summary>
		/// <param name="asciiText">A normal text.</param>
		/// <returns>A byte array representing the text turned into Unicode.</returns>
		public static byte[] AsciiToUnicode( string asciiText ) {
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter( stream, Encoding.BigEndianUnicode );

			writer.Write( asciiText );
			writer.Close();

			byte[] buffer = stream.GetBuffer();
			byte[] result = new byte[asciiText.Length*2];
			for ( int i=0; i<result.Length && i+1<buffer.Length; i++ ) {
				// the first value in the buffer is the buffer length.
				result[i] = buffer[i+1];
			}

			return result;
		}

		public static byte[] AsciiToUnicode( string asciiText, int size ) {

			byte[] buffer = AsciiToUnicode( asciiText );
			byte[] result = MemSet( size/2, UnicodeBlank );

			if ( size%2 == 1 ) {
				Array.Resize( ref result, result.Length+1 );
			}

			Array.Copy( buffer, result, Math.Min( size, buffer.Length ) );
			/*
			int i;
			for ( i=0; i<size && i<buffer.Length; i++ ) {
				result[i] = buffer[i];
			}
			 */
			if ( buffer.Length<size-2 ) {
				result[buffer.Length] = result[buffer.Length+1] = 0;
			}

			return result;
		}

		public static byte[] AsciiToUnicode( byte[] asciiText ) {
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter( stream, Encoding.BigEndianUnicode );

			for ( int i=0; i<asciiText.Length; i++ ) {
				writer.Write( (char)asciiText[i] );
			}
			writer.Close();

			byte[] buffer = stream.GetBuffer();
			byte[] result = new byte[asciiText.Length*2];

			Array.Copy( buffer, result, Math.Min( result.Length, buffer.Length ) );
			/*
			for ( int i=0; i<result.Length && i<buffer.Length; i++ ) {
				result[i] = buffer[i];
			}
			*/
			return result;
		}

		public static byte[] AsciiToUnicode( byte[] asciiText, int size ) {

			byte[] buffer = AsciiToUnicode( asciiText );
			byte[] result = MemSet( size/2, UnicodeBlank );

			if ( size%2 == 1 ) {
				Array.Resize( ref result, result.Length+1 );
			}

			Array.Copy( buffer, result, Math.Min( result.Length, buffer.Length ) );
			/*
			for ( int i=0; i<size && i<buffer.Length; i++ ) {
				result[i] = buffer[i];
			}
			*/
			return result;
		}

		public static byte[] UnicodeToAscii( byte[] unicodeText ) {
			byte[] ascii = new byte[unicodeText.Length/2];
			for ( int i=0; i<ascii.Length; i++ ) {
				ascii[i] = unicodeText[i*2];
			}
			return ascii;
		}

		public static byte[] UnicodeToAscii( byte[] unicodeText, int size ) {
			byte[] ascii = MemSet( size, AsciiBlank );
			for ( int i=0; i<ascii.Length && i<unicodeText.Length/2; i++ ) {
				ascii[i] = unicodeText[i*2];
			}

			return ascii;
		}

		public static byte[] StringToByteArray( string text ) {
			byte[] result = new byte[text.Length];
			for ( int i=0; i<result.Length; i++ ) {
				result[i] = (byte)text[i];
			}

			return result;
		}

		public static byte[] StringToByteArray( string text, int size ) {
			byte[] buffer = StringToByteArray( text );
			byte[] result = MemSet( size, AsciiBlank );

			Array.Copy( buffer, result, Math.Min( result.Length, buffer.Length ) );
			/*
			for ( int i=0; i<buffer.Length && i<result.Length; i++ ) {
				result[i] = buffer[i];
			}
			*/
			return result;
		}

		public static string ByteArrayToString( byte[] array ) {
			char[] text = new char[array.Length];
			for ( int i=0; i<text.Length; i++ ) {
				text[i] = (char)array[i];
			}
			return new string( text );
		}

		/// <summary>
		/// Writes an empty sector (2048 bytes of 0) to a specified writer.
		/// </summary>
		/// <param name="writer"></param>
		public static void WriteEmptySector( BinaryWriter writer ) {
			byte[] buffer = new byte[SectorSize];
			writer.Write( buffer );
		}

		/// <summary>
		/// Changes an integer's byte order (big endian->little endian || little endian->big endian).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static UInt32 ChangeEndian( UInt32 value ) {
			UInt32 mask0 = 0xFF000000;
			UInt32 mask1 = 0x00FF0000;
			UInt32 mask2 = 0x0000FF00;
			UInt32 mask3 = 0x000000FF;

			return ( ( value & mask0 ) >> 24 ) | 
				   ( ( value & mask1 ) >> 8 ) |
				   ( ( value & mask2 ) << 8 ) |
				   ( ( value & mask3 ) << 24 );
		}

		/// <summary>
		/// Changes a word's byte order (big endian->little endian || little endian->big endian).
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static UInt16 ChangeEndian( UInt16 value ) {
			return (UInt16)( ( value >> 8 ) | (UInt16)( ( value & 0x00FF ) << 8 ) );
		}

		#endregion
	}
}