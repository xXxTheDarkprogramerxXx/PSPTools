using System;

namespace pspsharp.util
{

	public class FileUtil
	{
		public static string getExtension(File file)
		{
			string @base = file.Name;
			int index = @base.LastIndexOf('.');
			if (index < 0)
			{
				return "";
			}
			return @base.Substring(index + 1).ToLower();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void copyStream(InputStream is, OutputStream os) throws IOException
		public static void copyStream(System.IO.Stream @is, System.IO.Stream os)
		{
			sbyte[] temp = new sbyte[0x10000];
			while (true)
			{
				int count = @is.Read(temp, 0, temp.Length);
				if (count < 0)
				{
					break;
				}
				os.Write(temp, 0, count);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[] readInputStream(InputStream is) throws IOException
		public static sbyte[] readInputStream(System.IO.Stream @is)
		{
			System.IO.MemoryStream os = new System.IO.MemoryStream();
			copyStream(@is, os);
			return os.toByteArray();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[] readURL(java.net.URL url) throws IOException
		public static sbyte[] readURL(URL url)
		{
			using (System.IO.Stream inputStream = url.openStream())
			{
				return readInputStream(inputStream);
			}
		}

		public static string getURLBaseName(URL url)
		{
			if (url == null)
			{
				return null;
			}
			string path = url.Path;
			int i = path.LastIndexOf('/');
			if (i < 0)
			{
				return path;
			}
			return path.Substring(i + 1);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void writeBytes(File file, byte[] data) throws FileNotFoundException
		public static void writeBytes(File file, sbyte[] data)
		{
			try
			{
					using (System.IO.FileStream os = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write))
					{
					os.Write(data, 0, data.Length);
					}
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readAll(InputStream is, byte[] buffer, int offset, int len) throws IOException
		public static void readAll(System.IO.Stream @is, sbyte[] buffer, int offset, int len)
		{
			int aoffset = offset;
			int remaining = len;
			while (remaining > 0)
			{
				int read = @is.Read(buffer, aoffset, remaining);
				if (read > 0)
				{
					remaining -= read;
					aoffset += read;
				}
			}
		}

		public static File findFolderNameInAncestors(File @base, string name)
		{
			File current = @base;
			while (current != null)
			{
				File file = new File(current, name);
				if (file.exists())
				{
					return file.AbsoluteFile;
				}
				current = current.ParentFile;
			}
			return null;
		}
	}

}