/*
 ORIGNAL CODE POWERED BY 
 https://sourceforge.net/projects/iso-creator-cs/ 
 https://github.com/simonwgill/isocreator-4.5
 All credit for ISO creation goes to them
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using BER.CDCat.Export;
using ISO9660.Enums;
using IsoCreator.IsoWrappers;
using IsoCreator.DirectoryTree;

namespace IsoCreator {
	public class IsoCreator {

		#region Iso Creator Args Class

		/// <summary>
		/// Used for sending parameters to ParameterizedThreadStart delegate function.
		/// Contains the natural arguments for Folder2Iso function.
		/// </summary>
		public class IsoCreatorFolderArgs {

			#region Fields

			private string m_folderPath;
			private string m_isoPath;
			private string m_volumeName;

			#endregion

			#region Properties

			public string FolderPath {
				get {
					return m_folderPath;
				}
			}

			public string IsoPath {
				get {
					return m_isoPath;
				}
			}

			public string VolumeName {
				get {
					return m_volumeName;
				}
			}

			#endregion

			#region Constructors

			public IsoCreatorFolderArgs( string folderPath, string isoPath, string volumeName ) {
				m_folderPath = folderPath;
				m_isoPath = isoPath;
				m_volumeName = volumeName;
			}

			#endregion
		}

		/// <summary>
		/// Used for sending parameters to ParameterizedThreadStart delegate function.
		/// Contains the natural arguments for Folder2Iso function.
		/// </summary>
		public class IsoCreatorTreeArgs {

			#region Fields

			private BER.CDCat.Export.TreeNode m_volume;
			private string m_isoPath;

			#endregion

			#region Properties

			public BER.CDCat.Export.TreeNode Volume {
				get {
					return m_volume;
				}
			}

			public string IsoPath {
				get {
					return m_isoPath;
				}
			}

			#endregion

			#region Constructors

			public IsoCreatorTreeArgs( BER.CDCat.Export.TreeNode volume, string isoPath ) {
				m_volume = volume;
				m_isoPath = isoPath;
			}

			#endregion
		}

		#endregion

		#region Old Code (Demo code - Not verified on latest updates, but may still clarify some things)

		public void WriteDemoImage( BinaryWriter writer ) {
			int currentSector;

			for ( currentSector=0; currentSector<16; currentSector++ ) {
				IsoAlgorithm.WriteEmptySector( writer );
			}

			DirectoryRecordWrapper rootDir = new DirectoryRecordWrapper( 19, IsoAlgorithm.SectorSize, DateTime.Now, true, "." );
			VolumeDescriptorWrapper volumeDescriptor = new VolumeDescriptorWrapper( "EPURASU", 28, 26, 21, 22, rootDir, DateTime.Now, DateTime.Now.Subtract( TimeSpan.FromDays( 2 ) ), 8 );

			//rootDir.SetDirectoryRecord( 19, ISO9660.SectorSize, DateTime.Now, true, "." );

			// [ Sect 16 ] Primary volume descriptor
			volumeDescriptor.VolumeDescriptorType = VolumeType.Primary;
			//			volumeDescriptor.SetVolumeDescriptor( "EPURASU", 28, 26, 21, 22, rootDir, DateTime.Now, DateTime.Now.Subtract( TimeSpan.FromDays( 2 ) ), 8 );

			volumeDescriptor.Write( writer );

			// [ Sect 17 ] Suplementary volume descriptor (in care scriem cu unicode)
			rootDir.SetDirectoryRecord( 23, IsoAlgorithm.SectorSize, DateTime.Now, true, "." );
			volumeDescriptor.VolumeDescriptorType = VolumeType.Suplementary;
			volumeDescriptor.SetVolumeDescriptor( "Epurasu", 28, 38, 25, 26, rootDir, DateTime.Now, DateTime.Now.Subtract( TimeSpan.FromDays( 2 ) ), 8 );

			volumeDescriptor.Write( writer );

			// [ Sect 18 ] Volume descriptor set termnator
			volumeDescriptor.VolumeDescriptorType = VolumeType.SetTerminator;

			volumeDescriptor.Write( writer );


			// [ Sect 19 ] Continutul directorului radacina:
			// Directorul curent: "."
			rootDir.SetDirectoryRecord( 19, IsoAlgorithm.SectorSize, DateTime.Now, true, "." );
			rootDir.Write( writer );

			// Directorul parinte: "..". Fiind vorba de radacina, directorul parinte e o referinta la directorul curent.
			rootDir.SetDirectoryRecord( 19, IsoAlgorithm.SectorSize, DateTime.Now, true, ".." );
			rootDir.Write( writer );

			// Director copil: "Director"
			DirectoryRecordWrapper childDir = new DirectoryRecordWrapper( 20, IsoAlgorithm.SectorSize, DateTime.Now, true, "DIRECTOR" );
			//childDir.SetDirectoryRecord( 20, ISO9660.SectorSize, DateTime.Now, true, "DIRECTOR" );
			int bytesWritten = childDir.Write( writer );

			writer.Write( new byte[2048 - 34 - 34 - bytesWritten] );

			// [ Sect 20 ] Continutul directorului "Director"
			childDir.SetDirectoryRecord( 20, IsoAlgorithm.SectorSize, DateTime.Now, true, "." );
			childDir.Write( writer );
			childDir.SetDirectoryRecord( 19, IsoAlgorithm.SectorSize, DateTime.Now, true, ".." );
			childDir.Write( writer );
			childDir.SetDirectoryRecord( 27, 45, DateTime.Now, false, "NUMELEFI.TXT" );
			bytesWritten = childDir.Write( writer );

			writer.Write( new byte[2048 - 34 - 34 - bytesWritten] );

			// [ Sect 21 ] Pathtable pt little endian

			// Root:
			PathTableRecordWrapper record = new PathTableRecordWrapper();
			record.Endian = Endian.LittleEndian;
			record.SetPathTableRecord( 19, 1, "." );
			bytesWritten = record.Write( writer );

			// "Director":
			record.SetPathTableRecord( 20, 1, "DIRECTOR" );
			bytesWritten += record.Write( writer );

			writer.Write( new byte[2048 - bytesWritten] );

			// [ Sect 22 ] Pathtable pt big endian

			// Root:
			record = new PathTableRecordWrapper();
			record.Endian = Endian.BigEndian;
			record.SetPathTableRecord( 19, 1, "." );
			record.Write( writer );

			// "Director":
			record.SetPathTableRecord( 20, 1, "DIRECTOR" );
			record.Write( writer );

			writer.Write( new byte[2048 - bytesWritten] );




			// [ Sect 23 ] Continutul directorului radacina:
			rootDir.VolumeDescriptorType = VolumeType.Suplementary;

			// Directorul curent: "."
			rootDir.SetDirectoryRecord( 23, IsoAlgorithm.SectorSize, DateTime.Now, true, "." );
			rootDir.Write( writer );

			// Directorul parinte: "..". Fiind vorba de radacina, directorul parinte e o referinta la directorul curent.
			rootDir.SetDirectoryRecord( 23, IsoAlgorithm.SectorSize, DateTime.Now, true, ".." );
			rootDir.Write( writer );

			// Director copil: "Director"
			childDir = new DirectoryRecordWrapper( 24, IsoAlgorithm.SectorSize, DateTime.Now, true, "Directorul" );
			//childDir.SetDirectoryRecord( 24, ISO9660.SectorSize, DateTime.Now, true, "Directorul" );
			childDir.VolumeDescriptorType = VolumeType.Suplementary;
			bytesWritten = childDir.Write( writer );

			writer.Write( new byte[2048 - 34 - 34 - bytesWritten] );

			// [ Sect 24 ] Continutul directorului "Director"
			childDir.SetDirectoryRecord( 24, IsoAlgorithm.SectorSize, DateTime.Now, true, "." );
			childDir.Write( writer );
			childDir.SetDirectoryRecord( 23, IsoAlgorithm.SectorSize, DateTime.Now, true, ".." );
			childDir.Write( writer );
			childDir.SetDirectoryRecord( 27, 45, DateTime.Now, false, "numeleFisierului.txt" );
			bytesWritten = childDir.Write( writer );

			writer.Write( new byte[2048 - 34 - 34 - bytesWritten] );

			// [ Sect 25 ] Pathtable pt little endian

			// Root:
			record = new PathTableRecordWrapper();
			record.Endian = Endian.LittleEndian;
			record.VolumeDescriptorType = VolumeType.Suplementary;
			record.SetPathTableRecord( 23, 1, "." );
			bytesWritten = record.Write( writer );

			// "Director":
			record.SetPathTableRecord( 24, 1, "Directorul" );
			bytesWritten += record.Write( writer );

			writer.Write( new byte[2048 - bytesWritten] );

			// [ Sect 26 ] Pathtable pt big endian

			// Root:
			record = new PathTableRecordWrapper();
			record.Endian = Endian.BigEndian;
			record.VolumeDescriptorType = VolumeType.Suplementary;
			record.SetPathTableRecord( 23, 1, "." );
			record.Write( writer );

			// "Director":
			record.SetPathTableRecord( 24, 1, "Directorul" );
			record.Write( writer );

			writer.Write( new byte[2048 - bytesWritten] );

			// [ Sect 27 ] Continutul fisierului (45 B):
			writer.Write( IsoAlgorithm.StringToByteArray( "Catelus cu parul cretz fura ratza din cotetz." ) );
			writer.Write( new byte[2048-45] );
		}

		#endregion

		#region Writing Methods

		#region Helper Methods (SetDirectoryNumbers(dirArray))

		/// <summary>
		/// Sets the directory numbers according to the ISO 9660 standard, so that Path Tables could be built. (root=1, first child=2, etc.)
		/// The order of the directories is as following:
		/// 1. If two directories are on different levels, then the one on the lowest level comes first;
		/// 2. If the directories are on the same level, but have different parents, then they are ordered in the same order as their parents.
		/// 3. If the directories have the same parent, then they are sorted according to their name (lexicographic).
		/// </summary>
		/// <param name="dirArray">An array of SORTED IsoDirectories according to the ISO 9660 standard.</param>
		private void SetDirectoryNumbers( IsoDirectory[] dirArray ) {
			if ( dirArray == null ) {
				return;
			}
			for ( int i=0; i<dirArray.Length; i++ ) {
				( (IsoDirectory)dirArray[i] ).Number = (UInt16)( i+1 );
			}
		}

		#endregion

		/// <summary>
		/// Writes the first 16 empty sectors of an ISO image.
		/// </summary>
		/// <param name="writer">A binary writer to write the data.</param>
		private void WriteFirst16EmptySectors( BinaryWriter writer ) {
			for ( int i=0; i<16; i++ ) {
				writer.Write( new byte[IsoAlgorithm.SectorSize] );
			}
		}

		/// <summary>
		/// Writes three volume descriptors speciffic to the ISO 9660 Joliet:
		/// 1. Primary volume descriptor;
		/// 2. Suplementary volume descriptor;
		/// 3. Volume descriptor set terminator.
		/// </summary>
		/// <param name="writer">A binary writer to write the data.</param>
		/// <param name="volumeName">A normal string representing the desired name of the volume. 
		/// (the maximum standard length for this string is 16 for Joliet, so if the name is larger 
		/// than 16 characters, it is truncated.)</param>
		/// <param name="root">The root IsoDirectory, representing the root directory for the volume.</param>
		/// <param name="volumeSpaceSize">The ISO total space size IN SECTORS. 
		/// (For example, if the ISO space size is 1,427,456 bytes, then the volumeSpaceSize will be 697)</param>
		/// <param name="pathTableSize1">The first path table size (for the primary volume) IN BYTES.</param>
		/// <param name="pathTableSize2">The second path table size (for the suplementary volume) IN BYTES.</param>
		/// <param name="typeLPathTable1">The location (sector) of the first LITTLE ENDIAN path table.</param>
		/// <param name="typeMPathTable1">The location (sector) of the first BIG ENDIAN path table.</param>
		/// <param name="typeLPathTable2">The location (sector) of the second LITTLE ENDIAN path table.</param>
		/// <param name="typeMPathTable2">The location (sector) of the second BIG ENDIAN path table.</param>
		private void WriteVolumeDescriptors( BinaryWriter writer,
											 string volumeName,
											 IsoDirectory root,
											 UInt32 volumeSpaceSize,
											 UInt32 pathTableSize1, UInt32 pathTableSize2,
											 UInt32 typeLPathTable1, UInt32 typeMPathTable1,
											 UInt32 typeLPathTable2, UInt32 typeMPathTable2 ) {

			// Throughout this program I have respected the convention of refering to the root as "."; 
			// However, one should not confuse the root with the current directory, also known as "." (along with the parent directory, "..").

			// Primary Volume Descriptor:

			// Create a Directory Record of the root and the volume descriptor.
			DirectoryRecordWrapper rootRecord = new DirectoryRecordWrapper( root.Extent1, root.Size1, root.Date, root.IsDirectory, "." );
			VolumeDescriptorWrapper volumeDescriptor = new VolumeDescriptorWrapper( volumeName, volumeSpaceSize, pathTableSize1, typeLPathTable1, typeMPathTable1, rootRecord, DateTime.Now, DateTime.Now, 8 );
			volumeDescriptor.VolumeDescriptorType = VolumeType.Primary;
			volumeDescriptor.Write( writer );

			// Suplementary volume descriptor:

			rootRecord = new DirectoryRecordWrapper( root.Extent2, root.Size2, root.Date, root.IsDirectory, "." );
			volumeDescriptor = new VolumeDescriptorWrapper( volumeName, volumeSpaceSize, pathTableSize2, typeLPathTable2, typeMPathTable2, rootRecord, DateTime.Now, DateTime.Now, 8 );
			volumeDescriptor.VolumeDescriptorType = VolumeType.Suplementary;
			volumeDescriptor.Write( writer );

			// Volume descriptor set terminator:

			volumeDescriptor.VolumeDescriptorType = VolumeType.SetTerminator;
			volumeDescriptor.Write( writer );
		}

		/// <summary>
		/// Writes the containings of each directory
		/// </summary>
		/// <param name="writer">A binary writer to write the data.</param>
		/// <param name="dirArray">An array of IsoDirectories to be written.</param>
		/// <param name="type">The type of writing to be performed:
		/// Primary - corresponding to the Primary Volume Descriptor (DOS Speciffic - 8 letter ASCII upper case names)
		/// Suplementary - corresponding to the Suplementary Volume Descriptor (Windows speciffic - 101 letter Unicode names)</param>
		private void WriteDirectories( BinaryWriter writer, IsoDirectory[] dirArray, VolumeType type ) {
			if ( dirArray == null ) {
				return;
			}
			for ( int i=0; i<dirArray.Length; i++ ) {
				dirArray[i].Write(writer, type);
				this.OnProgress( (int)(writer.BaseStream.Length/IsoAlgorithm.SectorSize) );
			}
		}

		/// <summary>
		/// Writes a path table corresponding to a given directory structure.
		/// The order of the directories is as following (ISO 9660 standard):
		/// 1. If two directories are on different levels, then the one on the lowest level comes first;
		/// 2. If the directories are on the same level, but have different parents, then they are ordered in the same order as their parents.
		/// 3. If the directories have the same parent, then they are sorted according to their name (lexicographic).
		/// </summary>
		/// <param name="writer">A binary writer to write the data.</param>
		/// <param name="dirArray">An array of IsoDirectories representing the directory structure.</param>
		/// <param name="endian">The byte order of numbers (little endian or big endian).</param>
		/// <param name="type">The type of writing to be performed:
		/// Primary - corresponding to the Primary Volume Descriptor (DOS Speciffic - 8 letter ASCII upper case names)
		/// Suplementary - corresponding to the Suplementary Volume Descriptor (Windows speciffic - 101 letter Unicode names)</param>
		/// <returns>An integer representing the total number of bytes written.</returns>
		private int WritePathTable( BinaryWriter writer, IsoDirectory[] dirArray, Endian endian, VolumeType type ) {
			if ( dirArray == null ) {
				return 0;
			}

			int bytesWritten = 0;
			for ( int i=0; i<dirArray.Length; i++ ) {
				// The directory list is sorted according to the ISO 9660 standard, so the first one (0) should be the root.
				bytesWritten += dirArray[i].WritePathTable( writer, ( i==0 ), endian, type );
			}

			// A directory must ocupy a number of bytes multiple of 2048 (the sector size).
			writer.Write( new byte[IsoAlgorithm.SectorSize - ( bytesWritten%IsoAlgorithm.SectorSize )] );

			return bytesWritten;
		}

		#endregion

		#region Folder to ISO

		/// <summary>
		/// Writes an ISO with the contains of the folder given as a parameter.
		/// </summary>
		/// <param name="rootDirectoryInfo">The folder to be turned into an iso.</param>
		/// <param name="writer">A binary writer to write the data.</param>
		/// <param name="volumeName">The name of the volume created.</param>
		private void Folder2Iso( DirectoryInfo rootDirectoryInfo, BinaryWriter writer, string volumeName ) {

			ArrayList dirList;
			IsoDirectory[] dirArray;

			this.OnProgress( "Initializing ISO root directory...", 0, 1 );

			IsoDirectory root = new IsoDirectory( rootDirectoryInfo, 1, "0", Progress );

			//
			// Folder structure and path tables corresponding to the Primary Volume Descriptor:
			//

			this.OnProgress( "Preparing first set of directory extents...", 0, 1 );

			dirList = new ArrayList();
			dirList.Add( root );

			// Set all extents corresponding to the primary volume descriptor;
			// Memorize the SORTED directories in the dirList list.
			// The first extent (corresponding to the root) should be at the 19th sector 
			// (which is the first available sector: 0-15 are empty and the next 3 (16-18) 
			// are occupied by the volume descriptors).
			IsoDirectory.SetExtent1( dirList, 0, 19 );

			this.OnProgress( 1 );

			this.OnProgress( "Calculating directory numbers...", 0, 1 );

			dirArray = new IsoDirectory[dirList.Count];
			dirList.ToArray().CopyTo( dirArray, 0 );		// Copy to an array the sorted directory list.

			this.SetDirectoryNumbers( dirArray );			// Set the directory numbers, used in the path tables.

			this.OnProgress( 1 );

			this.OnProgress( "Preparing first set of path tables...", 0, 2 );

			// Create a memory stream where to temporarily save the path tables. 
			// (We can't write them directly to the file, because we first have to write - by convention - 
			// the directories. For now, we cannot do that, since we don't know the files' extents.
			// Those will be calculated later, when we know the actual size of the path tables, because
			// the files come at the end of the file, after the path tables.)
			// I used this algorihm, although a little backword, since this is the algorithm NERO uses,
			// and I gave them credit for choosing the best one ;)
			MemoryStream memory1 = new MemoryStream();
			BinaryWriter memoryWriter1 = new BinaryWriter( memory1 );

			// Calculate the position of the first little endian path table, which comes right after the last directory.
			IsoDirectory lastDir = dirArray[dirArray.Length-1];
			UInt32 typeLPathTable1 = lastDir.Extent1 + lastDir.Size1/IsoAlgorithm.SectorSize;

			this.WritePathTable( memoryWriter1, dirArray, Endian.LittleEndian, VolumeType.Primary );

			this.OnProgress( 1 );

			// Calculate the position of the first big endian path table.
			UInt32 typeMPathTable1 = typeLPathTable1 + (UInt32)( memory1.Length )/IsoAlgorithm.SectorSize;

			UInt32 pathTableSize1 = (UInt32)this.WritePathTable( memoryWriter1, dirArray, Endian.BigEndian, VolumeType.Primary );

			this.OnProgress( 2 );

			//
			// end
			//

			//
			// Folder structure and path tables corresponding to the Suplementary Volume Descriptor:
			//

			this.OnProgress( "Preparing second set of directory extents...", 0, 1 );

			dirList = new ArrayList();
			dirList.Add( root );

			UInt32 currentExtent = typeLPathTable1 + (UInt32)( memory1.Length )/IsoAlgorithm.SectorSize;

			IsoDirectory.SetExtent2( dirList, 0, currentExtent );

			dirArray = new IsoDirectory[dirList.Count];
			dirList.ToArray().CopyTo( dirArray, 0 );

			this.OnProgress( 1 );

			this.OnProgress( "Preparing second set of path tables...", 0, 2 );

			MemoryStream memory2 = new MemoryStream();
			BinaryWriter memoryWriter2 = new BinaryWriter( memory2 );

			lastDir = dirArray[dirArray.Length-1];
			UInt32 typeLPathTable2 = lastDir.Extent2 + lastDir.Size2/IsoAlgorithm.SectorSize;

			this.WritePathTable( memoryWriter2, dirArray, Endian.LittleEndian, VolumeType.Suplementary );

			this.OnProgress( 1 );

			UInt32 typeMPathTable2 = typeLPathTable2 + (UInt32)( memory2.Length )/IsoAlgorithm.SectorSize;

			UInt32 pathTableSize2 = (UInt32)this.WritePathTable( memoryWriter2, dirArray, Endian.BigEndian, VolumeType.Suplementary );

			this.OnProgress( 2 );

			//
			// end
			//

			this.OnProgress( "Initializing...", 0, 1 );

			// Now that we know the extents and sizes of all directories and path tables, 
			// all that remains is to calculate files extent:
			currentExtent = typeLPathTable2 + (UInt32)( memory2.Length )/IsoAlgorithm.SectorSize;
			root.SetFilesExtent( ref currentExtent );

			// Calculate the total size in sectors of the file to be made.
			UInt32 volumeSpaceSize = 19;
			volumeSpaceSize += root.TotalSize;

//			volumeSpaceSize += root.TotalDirSize;

			volumeSpaceSize += (UInt32)memory1.Length / IsoAlgorithm.SectorSize;
			volumeSpaceSize += (UInt32)memory2.Length / IsoAlgorithm.SectorSize;

			// Prepare the buffers for the path tables.
			byte[] pathTableBuffer1 = memory1.GetBuffer();
			Array.Resize( ref pathTableBuffer1, (int)memory1.Length );

			byte[] pathTableBuffer2 = memory2.GetBuffer();
			Array.Resize( ref pathTableBuffer2, (int)memory2.Length );

			// Close the memory streams.
			memory1.Close();
			memory2.Close();
			memoryWriter1.Close();
			memoryWriter2.Close();

			this.OnProgress( 1 );

			//
			// Now all we have to do is to write all information to the ISO:
			//

			this.OnProgress( "Writing data to file...", 0, (int)volumeSpaceSize );

			// First, write the 16 empty sectors.
			this.WriteFirst16EmptySectors( writer );

			this.OnProgress( (int)(writer.BaseStream.Length/IsoAlgorithm.SectorSize) );

			// Write the three volume descriptors.
			this.WriteVolumeDescriptors(
				writer,	volumeName,	root,
				volumeSpaceSize,
				pathTableSize1, pathTableSize2,
				typeLPathTable1, typeMPathTable1,
				typeLPathTable2, typeMPathTable2 );

			this.OnProgress( (int)( writer.BaseStream.Length/IsoAlgorithm.SectorSize ) );

			// Write the directories in a manner corresponding to the Primary Volume Descriptor.
			this.WriteDirectories( writer, dirArray, VolumeType.Primary );

			// Write the first two path tables.
			writer.Write( pathTableBuffer1 );

			this.OnProgress( (int)( writer.BaseStream.Length/IsoAlgorithm.SectorSize ) );

			// Write the directories in a manner corresponding to the Suplementary Volume Descriptor.
			this.WriteDirectories( writer, dirArray, VolumeType.Suplementary );

			// Write the other two path tables.
			writer.Write( pathTableBuffer2 );

			this.OnProgress( (int)( writer.BaseStream.Length/IsoAlgorithm.SectorSize ) );

			// Write the files.
			root.WriteFiles( writer, Progress );

			// That's it ;)
		}

		/// <summary>
		/// Writes an ISO with the contains of the folder given as a parameter.
		/// </summary>
		/// <param name="folderPath">The path of the folder to be turned into an iso.</param>
		/// <param name="isoPath">The path of the iso file.</param>
		/// <param name="volumeName">The name of the volume to be created.</param>
		public void Folder2Iso( string folderPath, string isoPath, string volumeName ) {
			try {
				FileStream isoFileStream = new FileStream( isoPath, FileMode.Create );
				BinaryWriter writer = new BinaryWriter( isoFileStream );
				DirectoryInfo rootDirectoryInfo = new DirectoryInfo( folderPath );
				try {
					Folder2Iso( rootDirectoryInfo, writer, volumeName );

					writer.Close();
					isoFileStream.Close();

					this.OnFinished( "ISO writing process finished succesfully" );
				} catch ( Exception ex ) {
					writer.Close();
					isoFileStream.Close();
					throw ex;
				}
			} catch ( System.Threading.ThreadAbortException ex ) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
				this.OnAbort( "Aborted by user" );
			} catch ( Exception ex ) {
				this.OnAbort( ex.Message );
			}
		}

		/// <summary>
		/// Writes an ISO with the speciffications contained in the IsoCreatorArgs object given as parameter.
		/// </summary>
		/// <param name="data">An IsoCreatorFolderArgs object.</param>
		public void Folder2Iso( object data ) {
			if ( data.GetType() != typeof( IsoCreatorFolderArgs ) ) {
				return;
			}

			IsoCreatorFolderArgs args = (IsoCreatorFolderArgs)data;
			this.Folder2Iso( args.FolderPath, args.IsoPath, args.VolumeName );
		}

		#endregion

		#region Tree to ISO

		/// <summary>
		/// Writes an ISO with the contains of the tree given as a parameter.
		/// This is a "virtual" ISO, which means that you will find on it only a directory structure;
		/// files will actually not ocupy any space on it. (For a better picture of what happens here,
		/// run the VirtualIsoCreator form in Forms namespace. There is a demo. Also, if you have CDCat 
		/// installed on your PC, you should know by now the effect of the method below. Within CDCat, 
		/// this method is used through the ExportIso class in BER.CDCat.Export namespace)
		/// </summary>
		/// <param name="volume">The directory structure to be turned into an iso.</param>
		/// <param name="writer">A binary writer to write the data.</param>
		private void Tree2Iso( BER.CDCat.Export.TreeNode volume, BinaryWriter writer ) {

			ArrayList dirList;
			IsoDirectory[] dirArray;

			this.OnProgress( "Initializing ISO root directory...", 0, 1 );

			IsoDirectory root = new IsoDirectory( volume, 1, "0", Progress );

			//
			// Folder structure and path tables corresponding to the Primary Volume Descriptor:
			//

			this.OnProgress( "Preparing first set of directory extents...", 0, 1 );

			dirList = new ArrayList();
			dirList.Add( root );

			// Set all extents corresponding to the primary volume descriptor;
			// Memorize the SORTED directories in the dirList list.
			// The first extent (corresponding to the root) should be at the 19th sector 
			// (which is the first available sector: 0-15 are empty and the next 3 (16-18) 
			// are occupied by the volume descriptors).
			IsoDirectory.SetExtent1( dirList, 0, 19 );

			this.OnProgress( 1 );

			this.OnProgress( "Calculating directory numbers...", 0, 1 );

			dirArray = new IsoDirectory[dirList.Count];
			dirList.ToArray().CopyTo( dirArray, 0 );		// Copy to an array the sorted directory list.

			this.SetDirectoryNumbers( dirArray );			// Set the directory numbers, used in the path tables.

			this.OnProgress( 1 );

			this.OnProgress( "Preparing first set of path tables...", 0, 2 );

			// Create a memory stream where to temporarily save the path tables. 
			// (We can't write them directly to the file, because we first have to write - by convention - 
			// the directories. For now, we cannot do that, since we don't know the files' extents.
			// Those will be calculated later, when we know the actual size of the path tables, because
			// the files come at the end of the file, after the path tables.)
			// I used this algorihm, although a little backword, since this is the algorithm NERO uses,
			// and I gave them credit for choosing the best one ;)
			MemoryStream memory1 = new MemoryStream();
			BinaryWriter memoryWriter1 = new BinaryWriter( memory1 );

			// Calculate the position of the first little endian path table, which comes right after the last directory.
			IsoDirectory lastDir = dirArray[dirArray.Length-1];
			UInt32 typeLPathTable1 = lastDir.Extent1 + lastDir.Size1/IsoAlgorithm.SectorSize;

			this.WritePathTable( memoryWriter1, dirArray, Endian.LittleEndian, VolumeType.Primary );

			this.OnProgress( 1 );

			// Calculate the position of the first big endian path table.
			UInt32 typeMPathTable1 = typeLPathTable1 + (UInt32)( memory1.Length )/IsoAlgorithm.SectorSize;

			UInt32 pathTableSize1 = (UInt32)this.WritePathTable( memoryWriter1, dirArray, Endian.BigEndian, VolumeType.Primary );

			this.OnProgress( 2 );

			//
			// end
			//

			//
			// Folder structure and path tables corresponding to the Suplementary Volume Descriptor:
			//

			this.OnProgress( "Preparing second set of directory extents...", 0, 1 );

			dirList = new ArrayList();
			dirList.Add( root );

			UInt32 currentExtent = typeLPathTable1 + (UInt32)( memory1.Length )/IsoAlgorithm.SectorSize;

			IsoDirectory.SetExtent2( dirList, 0, currentExtent );

			dirArray = new IsoDirectory[dirList.Count];
			dirList.ToArray().CopyTo( dirArray, 0 );

			this.OnProgress( 1 );

			this.OnProgress( "Preparing second set of path tables...", 0, 2 );

			MemoryStream memory2 = new MemoryStream();
			BinaryWriter memoryWriter2 = new BinaryWriter( memory2 );

			lastDir = dirArray[dirArray.Length-1];
			UInt32 typeLPathTable2 = lastDir.Extent2 + lastDir.Size2/IsoAlgorithm.SectorSize;

			this.WritePathTable( memoryWriter2, dirArray, Endian.LittleEndian, VolumeType.Suplementary );

			this.OnProgress( 1 );

			UInt32 typeMPathTable2 = typeLPathTable2 + (UInt32)( memory2.Length )/IsoAlgorithm.SectorSize;

			UInt32 pathTableSize2 = (UInt32)this.WritePathTable( memoryWriter2, dirArray, Endian.BigEndian, VolumeType.Suplementary );

			this.OnProgress( 2 );

			//
			// end
			//

			this.OnProgress( "Initializing...", 0, 1 );

			// Now that we know the extents and sizes of all directories and path tables, 
			// all that remains is to calculate files extent. However, this being a virtual ISO,
			// it won't memorize real files, but only images of files, which will apear to have a real size,
			// but in fact, won't occupy any more space. So we will leave all the files' extents null (0).

			// Calculate the total size in sectors of the file to be made.
			UInt32 volumeSpaceSize = 19;
			volumeSpaceSize += root.TotalDirSize;	// This only calculates the size of the directories, without the files.
			volumeSpaceSize += (UInt32)memory1.Length / IsoAlgorithm.SectorSize;
			volumeSpaceSize += (UInt32)memory2.Length / IsoAlgorithm.SectorSize;

			// Prepare the buffers for the path tables.
			byte[] pathTableBuffer1 = memory1.GetBuffer();
			Array.Resize( ref pathTableBuffer1, (int)memory1.Length );

			byte[] pathTableBuffer2 = memory2.GetBuffer();
			Array.Resize( ref pathTableBuffer2, (int)memory2.Length );

			// Close the memory streams.
			memory1.Close();
			memory2.Close();
			memoryWriter1.Close();
			memoryWriter2.Close();

			this.OnProgress( 1 );

			//
			// Now all we have to do is to write all information to the ISO:
			//

			this.OnProgress( "Writing data to file...", 0, (int)volumeSpaceSize );

			// First, write the 16 empty sectors.
			this.WriteFirst16EmptySectors( writer );

			this.OnProgress( (int)(writer.BaseStream.Length/IsoAlgorithm.SectorSize) );

			// Write the three volume descriptors.
			this.WriteVolumeDescriptors(
				writer, volume.Name, root,
				volumeSpaceSize,
				pathTableSize1, pathTableSize2,
				typeLPathTable1, typeMPathTable1,
				typeLPathTable2, typeMPathTable2 );

			this.OnProgress( (int)( writer.BaseStream.Length/IsoAlgorithm.SectorSize ) );

			// Write the directories in a manner corresponding to the Primary Volume Descriptor.
			this.WriteDirectories( writer, dirArray, VolumeType.Primary );

			// Write the first two path tables.
			writer.Write( pathTableBuffer1 );

			this.OnProgress( (int)( writer.BaseStream.Length/IsoAlgorithm.SectorSize ) );

			// Write the directories in a manner corresponding to the Suplementary Volume Descriptor.
			this.WriteDirectories( writer, dirArray, VolumeType.Suplementary );

			// Write the other two path tables.
			writer.Write( pathTableBuffer2 );

			this.OnProgress( (int)( writer.BaseStream.Length/IsoAlgorithm.SectorSize ) );

			// If this were an ISO with real files, this is the part where we would write the files.

			// That's it ;)
		}

		/// <summary>
		/// Writes an ISO with the contains of the tree given as a parameter, to the specified path.
		/// </summary>
		/// <param name="volume">The directory structure to be turned into an iso.</param>
		/// <param name="isoPath">The path of the iso file to be created.</param>
		public void Tree2Iso( BER.CDCat.Export.TreeNode volume, string isoPath ) {
			try {
				FileStream isoFileStream = new FileStream( isoPath, FileMode.Create );
				BinaryWriter writer = new BinaryWriter( isoFileStream );
				try {
					this.Tree2Iso( volume, writer );

					writer.Close();
					isoFileStream.Close();

					this.OnFinished( "ISO writing process finished succesfully" );
				} catch ( Exception ex ) {
					writer.Close();
					isoFileStream.Close();
					throw ex;
				}
			} catch ( System.Threading.ThreadAbortException ex ) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
				this.OnAbort( "Aborted by user" );
			} catch ( Exception ex ) {
				this.OnAbort( ex.Message );
			}
		}

		/// <summary>
		/// Writes an ISO with the speciffications contained in the IsoCreatorTreeArgs object given as parameter.
		/// </summary>
		/// <param name="data">An IsoCreatorTreeArgs object.</param>
		public void Tree2Iso( object data ) {
			if ( data.GetType() != typeof( IsoCreatorTreeArgs ) ) {
				return;
			}

			IsoCreatorTreeArgs args = (IsoCreatorTreeArgs)data;
			this.Tree2Iso( args.Volume, args.IsoPath );
		}

		#endregion

		#region Events

		public event ProgressDelegate Progress;

		public event FinishDelegate Finish;

		public event AbortDelegate Abort;

		private void OnFinished( string message ) {
			if ( Finish != null ) {
				Finish( this, new FinishEventArgs( message ) );
			}
		}

		private void OnProgress( int current ) {
			if ( Progress!=null ) {
				this.Progress( this, new ProgressEventArgs( current ) );
			}
		}

		private void OnProgress( int current, int maximum ) {
			if ( Progress!=null ) {
				this.Progress( this, new ProgressEventArgs( current, maximum ) );
			}
		}

		private void OnProgress( string action, int current, int maximum ) {
			if ( Progress!=null ) {
				this.Progress( this, new ProgressEventArgs( action, current, maximum ) );
			}
		}

		private void OnAbort( string message ) {
			if ( Abort != null ) {
				this.Abort( this, new AbortEventArgs( message ) );
			}
		}

		#endregion
	}
}
