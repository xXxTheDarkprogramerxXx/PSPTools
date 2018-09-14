using System;
using System.Collections.Generic;

namespace pspsharp.util
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.ISectorDevice_Fields.sectorLength;


	public class UmdBufferMerge
	{
		public static void Main(string[] args)
		{
			File fileToc1 = new File("tmp/umdbuffer1.toc");
			File fileIso1 = new File("tmp/umdbuffer1.iso");
			File fileToc2 = new File("tmp/umdbuffer2.toc");
			File fileIso2 = new File("tmp/umdbuffer2.iso");
			File fileToc = new File("tmp/umdbuffer.toc");
			File fileIso = new File("tmp/umdbuffer.iso");

			try
			{
				System.IO.FileStream fosToc = new System.IO.FileStream(fileToc, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				System.IO.FileStream fosIso = new System.IO.FileStream(fileIso, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				System.IO.FileStream fisToc1 = new System.IO.FileStream(fileToc1, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				System.IO.FileStream fisIso1 = new System.IO.FileStream(fileIso1, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				System.IO.FileStream fisToc2 = new System.IO.FileStream(fileToc2, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				DataOutput toc = new DataOutputStream(fosToc);
				DataOutput iso = new DataOutputStream(fosIso);
				DataInput toc1 = new DataInputStream(fisToc1);
				DataInput iso1 = new DataInputStream(fisIso1);
				DataInput toc2 = new DataInputStream(fisToc2);
				RandomAccessFile iso2 = new RandomAccessFile(fileIso2, "r");

				int numSectors = toc1.readInt();
				int numSectorsMerge = toc2.readInt();
				toc.writeInt(System.Math.Max(numSectors, numSectorsMerge));

				Dictionary<int, int> tocHashMap = new Dictionary<int, int>();
				for (int i = 4; i < fileToc1.length(); i += 8)
				{
					int sectorNumber = toc1.readInt();
					int bufferedSectorNumber = toc1.readInt();
					tocHashMap[sectorNumber] = bufferedSectorNumber;
					toc.writeInt(sectorNumber);
					toc.writeInt(bufferedSectorNumber);
				}

				sbyte[] buffer = new sbyte[sectorLength];
				for (int i = 0; i < fileIso1.length(); i += buffer.Length)
				{
					iso1.readFully(buffer);
					iso.write(buffer);
				}

				int nextFreeBufferedSectorNumber = (int)(fileIso1.length() / sectorLength);
				for (int i = 4; i < fileToc2.length(); i += 8)
				{
					int sectorNumber = toc2.readInt();
					int bufferedSectorNumber = toc2.readInt();
					if (!tocHashMap.ContainsKey(sectorNumber))
					{
						iso2.seek(bufferedSectorNumber * (long) sectorLength);
						iso2.readFully(buffer);
						iso.write(buffer);

						toc.writeInt(sectorNumber);
						toc.writeInt(nextFreeBufferedSectorNumber);
						tocHashMap[sectorNumber] = nextFreeBufferedSectorNumber;
						nextFreeBufferedSectorNumber++;
					}
				}

				fosToc.Close();
				fosIso.Close();
				fisToc1.Close();
				fisIso1.Close();
				fisToc2.Close();
				iso2.close();
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