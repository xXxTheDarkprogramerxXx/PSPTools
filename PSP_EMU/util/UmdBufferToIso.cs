using System;

namespace pspsharp.util
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.ISectorDevice_Fields.sectorLength;


	public class UmdBufferToIso
	{
		public static void Main(string[] args)
		{
			try
			{
				RandomAccessFile inToc = new RandomAccessFile("tmp/umdbuffer.toc", "r");
				RandomAccessFile inIso = new RandomAccessFile("tmp/umdbuffer.iso", "r");
				RandomAccessFile outIso = new RandomAccessFile("tmp/umd.iso", "rw");

				int numSectors = inToc.readInt();
				Console.WriteLine(string.Format("numSectors={0:D}", numSectors));
				sbyte[] buffer = new sbyte[sectorLength];
				for (int i = 4; i < inToc.length(); i += 8)
				{
					int sectorNumber = inToc.readInt();
					int bufferedSectorNumber = inToc.readInt();
					inIso.seek(bufferedSectorNumber * (long) sectorLength);
					inIso.readFully(buffer);

					outIso.seek(sectorNumber * (long) sectorLength);
					outIso.write(buffer);
				}
				inToc.close();
				inIso.close();
				outIso.close();
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}
	}

}